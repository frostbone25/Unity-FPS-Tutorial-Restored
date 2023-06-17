/*
    GENERAL NOTE: 
        Most of these scripts are a direct translation from the original UnityScript .js files.
        For the sake of "restoration" I've attempted to keep their original functionality as close as possible to the original scripts.
        However, there are occasionally some improvements or changes made to the scripts and these noted.
        Most of these are necessary due to script and API changes, but others are to leverage unity features that were implemented after the fact since unity 2.6.0
        If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)

    For this AI script instead of converting the original script, I decided almost the entire script and do the following...
        1. Leverage the NavMesh Pathfinding system that was implemented since Unity 3.5.0
        2. Implement sightlights and a Field Of View for a more controlled target visibility
        3. Utilize the Animator animation system for improved animation

    Some of the original code is kept, but everything else has been redone for sake of simplicity and improved functionality
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//ORIGINAL: Make sure there is always a character controller
//NEW NOTE: This character controller is not used at all currently...
//It used to be used to move the player, but since that is now handled by the NavMeshAgent the character controller functionally is only here for physics collisions/hit detection
//I suppose if one wanted you can simply replace this with a capsule collider.
[RequireComponent(typeof(CharacterController))]
public class AI : MonoBehaviour
{
    //NEW: the native walking speed of the AI when we are not targeting the target
    public float walkSpeed;

    //NEW: the speed at which the AI will move to pursue when a targeting the target
    public float runSpeed;

    //NEW: the size of the vision cone for the AI to do visibility checks
    public float sightFOV;

    //ORIGINAL: the speed at which the AI will rotate towards it's target
    public float rotationSpeed = 5.0f;

    //ORIGINAL: the range at which the AI will need to be at in order to start shooting at the target
    public float shootRange = 15.0f;

    //ORIGINAL: the range at which the AI will need to be in order to start attacking a target
    public float attackRange = 30.0f;

    //NEW: the angle at which the AI needs to be in to shoot at the player (they will rotate themselves to this range so they can shoot at the target)
    public float shootAngle = 4.0f;

    //ORIGINAL: the minimum distance for the AI to attack the target regardless of sight
    public float dontComeCloserRange = 5.0f;

    //ORIGINAL: the transform of the given target
    public Transform target;

    //NEW: our script that handles animation for the AI
    public AIAnimation aiAnimation;

    //----------------- PRIVATE -----------------
    //NEW: is the AI currently moving?
    private bool state_moving;

    //NEW: does the AI see the target?
    private bool state_sighted;

    //NEW: has the AI recently seen the target?
    private bool state_seenTarget;
    
    //NEW: the last position at which the target was seen before loosing visibility
    private Vector3 lastSeenPosition;

    //NEW: the main component
    private NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        lastSeenPosition = transform.position;

        // Auto setup player as target through tags
        if (target == null && GameObject.FindWithTag("Player"))
            target = GameObject.FindWithTag("Player").transform;
    }

    //NEW: public method that can be called by other scripts to alert the AI to the target if you feed it a position 
    public void AlertTarget(Vector3 position)
    {
        state_seenTarget = true;

        lastSeenPosition = position;
    }

    //NEW: runs for every physics update, this function would be most similar to the original Patrol() method
    private void FixedUpdate()
    {
        //recalculate the speeds, so that they are tuned to match the actual animation when sped up or slowed down
        float newWalkSpeed = walkSpeed * aiAnimation.walkSpeedMultiplier;
        float newRunSpeed = runSpeed * aiAnimation.runSpeedMultiplier;

        //ORIGINAL: find the closest waypoint
        AutoWayPoint curWayPoint = AutoWayPoint.FindClosest(transform.position);

        bool canSeeTarget_physics = CanSeeTarget();
        bool canSeeTarget_navmesh = TargetNavMeshRaycastCheck(navMeshAgent, target.position);
        bool insideSightFOV = InsideConeAngle(transform.position, transform.forward, target.position, sightFOV);
        bool tooClose = Vector3.Distance(transform.position, target.position) < dontComeCloserRange;

        state_sighted = (canSeeTarget_physics || canSeeTarget_navmesh) && insideSightFOV || tooClose;

        if (!state_sighted)
        {
            //patrol mode
            if (!state_seenTarget)
            {
                //make sure we are not stationary
                navMeshAgent.isStopped = false;

                //get the current waypoint post
                Vector3 waypointPosition = curWayPoint.transform.position;

                navMeshAgent.SetDestination(waypointPosition);

                //if we reach the current waypoint distance
                if (Vector3.Distance(transform.position, waypointPosition) < navMeshAgent.stoppingDistance)
                {
                    //find another one
                    curWayPoint = PickNextWaypoint(curWayPoint);
                }
            }
            //find player
            else
            {
                //make sure we are not stationary
                navMeshAgent.isStopped = false;

                //go to the last known position
                navMeshAgent.SetDestination(lastSeenPosition);

                //if we reach the last known position (and we don't catch sight)
                if (Vector3.Distance(transform.position, lastSeenPosition) < navMeshAgent.stoppingDistance)
                {
                    navMeshAgent.isStopped = true;

                    //we lost the target
                    state_seenTarget = false;
                }
            }
        }
        //sighted
        else
        {
            //update our last seen position
            lastSeenPosition = target.position;

            //we have sight of the target
            state_seenTarget = true;

            //if the target is far from the attack range, keep moving
            if (Vector3.Distance(transform.position, lastSeenPosition) > attackRange)
            {
                //make sure we are not stationary
                navMeshAgent.isStopped = false;

                //go towards the target
                navMeshAgent.SetDestination(lastSeenPosition);
            }
            //when we are within attacking range
            else
            {
                //we are within attacking range and have sight at the player, so start shooting
                navMeshAgent.isStopped = true;

                //get the target direction
                Vector3 targetDirection = transform.position - lastSeenPosition;
                float angle = Vector3.Angle(targetDirection, transform.forward);

                //if the angle to the target is too large
                if(angle > shootAngle)
                {
                    //orient ourselves toward the target
                    RotateTowards(lastSeenPosition);
                }
            }
        }

        state_moving = !navMeshAgent.isStopped;

        navMeshAgent.speed = state_seenTarget ? newRunSpeed : newWalkSpeed;

        aiAnimation.UpdateAnimator(state_moving, state_seenTarget);
    }

    public bool CanSeeTarget()
    {
        if (Vector3.Distance(transform.position, target.position) > attackRange)
            return false;

        RaycastHit hit;

        if (Physics.Linecast(transform.position, target.position, out hit))
            return hit.transform == target;

        return false;
    }

    //CHANGED - This is handled by an animation event on the Robot Fire animation clip
    public void Shoot()
    {
        // Fire gun
        BroadcastMessage("Fire");
    }

    private void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0;

        if (direction.magnitude < 0.1)
            return;

        // Rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    private AutoWayPoint PickNextWaypoint(AutoWayPoint currentWaypoint)
    {
        // We want to find the waypoint where the character has to turn the least

        // The direction in which we are walking
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        // The closer two vectors, the larger the dot product will be.
        AutoWayPoint best = currentWaypoint;
        float bestDot = -10.0f;

        foreach(AutoWayPoint cur in currentWaypoint.connected)
        {
            Vector3 direction = Vector3.Normalize(cur.transform.position - transform.position);
            float dot = Vector3.Dot(direction, forward);

            if (dot > bestDot && cur != currentWaypoint)
            {
                bestDot = dot;
                best = cur;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns true if the target vector is within the cone angle, otherwise it returns false.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceDirection"></param>
    /// <param name="target"></param>
    /// <param name="fieldOfView"></param>
    /// <returns></returns>
    public static bool InsideConeAngle(Vector3 source, Vector3 sourceDirection, Vector3 target, float fieldOfView, bool reverseDirection = false)
    {
        //get the direction from this transform to the target
        Vector3 targetDir = target - source;

        Vector3 direction = reverseDirection ? -sourceDirection : sourceDirection;

        //run an angle check to see if its within the fov
        return Vector3.Angle(targetDir, direction) < fieldOfView;
    }

    /// <summary>
    /// Returns true if there is no occlusion between the target position and the source position on the navmesh.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool TargetNavMeshRaycastCheck(NavMeshAgent agent, Vector3 target)
    {
        NavMeshHit navMeshHit;

        return agent.Raycast(target, out navMeshHit) == false;
    }

    //||||||||||||||||||||||||||||||||||| GIZMOS |||||||||||||||||||||||||||||||||||
    //||||||||||||||||||||||||||||||||||| GIZMOS |||||||||||||||||||||||||||||||||||
    //||||||||||||||||||||||||||||||||||| GIZMOS |||||||||||||||||||||||||||||||||||
    //These are here to help with the placement of AI, drawing some of their zones/field of view
    //These are not critical to the function of the AI but help considerably with the placement/configuration of them.

    //NEW
    private void OnDrawGizmosSelected()
    {
        //Draw wire spheres to represent the different ranges in the editor

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dontComeCloserRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        DrawFOVGizmo(transform.position, transform.forward, sightFOV, attackRange, Color.green);
    }

    /// <summary>
    /// Draws a flat field of view gizmo
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="fov"></param>
    /// <param name="range"></param>
    /// <param name="col"></param>
    public static void DrawFOVGizmo(Vector3 origin, Vector3 direction, float fov, float range, Color col)
    {
        //set the gizmos color
        Gizmos.color = col;

        //perform our calculations
        float halfFOV = fov / 2.0f; //divide by half because we need it for each side
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        //get our final directions
        Vector3 leftRayDirection = leftRayRotation * direction;
        Vector3 rightRayDirection = rightRayRotation * direction;

        //draw gizmos arrays to represent the FOV
        Gizmos.DrawRay(origin, leftRayDirection * range);
        Gizmos.DrawRay(origin, rightRayDirection * range);
    }
}
