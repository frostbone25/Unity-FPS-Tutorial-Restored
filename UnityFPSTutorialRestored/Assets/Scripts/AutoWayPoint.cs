using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class AutoWayPoint : MonoBehaviour
{
    public static List<AutoWayPoint> waypoints = new List<AutoWayPoint>();
    public List<AutoWayPoint> connected;
    public static float kLineOfSightCapsuleRadius = 0.25f;

	public static AutoWayPoint FindClosest(Vector3 pos) 
	{
		// The closer two vectors, the larger the dot product will be.
		AutoWayPoint closest = null;
		float closestDistance = 100000.0f;

		foreach(AutoWayPoint cur in waypoints) 
		{
			var distance = Vector3.Distance(cur.transform.position, pos);

			if (distance < closestDistance)
			{
				closestDistance = distance;
				closest = cur;
			}
		}

		return closest;
	}

	[ContextMenu("Update Waypoints")]
	public void UpdateWaypoints()
	{
		RebuildWaypointList();
	}

	private void Awake()
	{
		RebuildWaypointList();
	}

	// Draw the waypoint pickable gizmo
	private void OnDrawGizmos()
    {
		Gizmos.DrawIcon(transform.position, "Waypoint.tif");
	}

	// Draw the waypoint lines only when you select one of the waypoints
	private void OnDrawGizmosSelected()
    {
		if (waypoints.Count == 0)
			RebuildWaypointList();

		foreach(AutoWayPoint p in connected)
		{
			if (Physics.Linecast(transform.position, p.transform.position))
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(transform.position, p.transform.position);
			}
			else
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(transform.position, p.transform.position);
			}
		}
	}

    public void RebuildWaypointList()
	{
		List<AutoWayPoint> objects = new List<AutoWayPoint>(FindObjectsOfType<AutoWayPoint>());

		waypoints = objects;

		foreach(AutoWayPoint point in waypoints)
		{
			point.RecalculateConnectedWaypoints();
		}
	}

	public void RecalculateConnectedWaypoints()
	{
		connected = new List<AutoWayPoint>();

		foreach(AutoWayPoint other in waypoints)
		{
			// Don't connect to ourselves
			if (other == this)
				continue;

			// Do we have a clear line of sight?
			if (!Physics.CheckCapsule(transform.position, other.transform.position, kLineOfSightCapsuleRadius))
			{
				connected.Add(other);
			}
		}
	}
}
