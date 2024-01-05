using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

[RequireComponent(typeof(AudioSource))]
public class MachineGun : MonoBehaviour
{
    public float range = 100.0f;
    public float fireRate = 0.05f;
    public float force = 10.0f;
    public float damage = 5.0f;
    public int bulletsPerClip = 40;
    public int clips = 20;
    public float reloadTime = 0.5f;
    public ParticleSystem hitParticles;
    public int hitParticlesEmitCount; //new - lets the user define how much sparks they want
    public Renderer muzzleFlash;
    public float muzzleDuration; //new - lets the user define how long they want the muzzle flash to show
    public LayerMask raycastLayers; //new - defines what the raycast will hit when shooting (used to avoid hitting the player itself)
    public bool infinteAmmo = false;

    private float nextMuzzleDuration; //new - used by muzzle duration to check the time since the last shot
    private int bulletsLeft = 0;
    private float nextFireTime = 0.0f;
    private float m_LastFrameShot = -1.0f; //not used

    private AudioSource audioSource; //new

    // Start is called before the first frame update
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if(hitParticles)
            hitParticlesEmitCount = hitParticles.emission.burstCount;

        // We don't want to emit particles all the time, only when we hit something.
        if (hitParticles)
            hitParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        bulletsLeft = bulletsPerClip;
    }

    private void LateUpdate()
    {
        //note: changed from original so the muzzle duration lasts for a longer period of time (defined by the user, it used to only show for a single frame which would be too fast)

        if (muzzleFlash)
        {
            if(nextMuzzleDuration > Time.time)
            {
                muzzleFlash.transform.localRotation = Quaternion.AngleAxis(Random.value * 360, Vector3.forward);
                muzzleFlash.enabled = true;

                if (audioSource)
                {
                    if (!audioSource.isPlaying)
                        audioSource.Play();

                    audioSource.loop = true;
                }
            }
            else
            {
                // We didn't, disable the muzzle flash
                muzzleFlash.enabled = false;
                enabled = false;

                // Play sound
                if (audioSource)
                {
                    audioSource.loop = false;
                }
            }
        }
    }

    public void Fire()
    {
        if (infinteAmmo)
            bulletsLeft = 50000;

        if (bulletsLeft == 0)
            return;

        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - fireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time && bulletsLeft != 0)
        {
            FireOneShot();
            nextFireTime += fireRate;
        }
    }

    public void FireOneShot()
    {
        Vector3 direction = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        // Did we hit anything?
        if (Physics.Raycast(transform.position, direction, out hit, range, raycastLayers))
        {
            // Apply a force to the rigidbody we hit
            if (hit.rigidbody)
                hit.rigidbody.AddForceAtPosition(force * direction, hit.point);

            // Place the particle system for spawing out of place where we hit the surface!
            // And spawn a couple of particles
            if (hitParticles)
            {
                hitParticles.transform.position = hit.point;
                hitParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                hitParticles.Emit(3);
            }

            // Send a damage message to the hit object			
            hit.collider.SendMessageUpwards("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);

            hit.collider.SendMessageUpwards("AlertTarget", transform.position, SendMessageOptions.DontRequireReceiver);
        }

        bulletsLeft--;

        // Register that we shot this frame,
        // so that the LateUpdate function enabled the muzzleflash renderer for one frame
        //m_LastFrameShot = Time.frameCount;
        nextMuzzleDuration = Time.time + muzzleDuration;

        enabled = true;

        // Reload gun in reload Time		
        if (bulletsLeft == 0)
            StartCoroutine(Reload());
    }

    public IEnumerator Reload()
    {
        // Wait for reload time first - then add more bullets!
        yield return new WaitForSeconds(reloadTime);

        // We have a clip left reload
        if (clips > 0)
        {
            clips--;
            bulletsLeft = bulletsPerClip;
        }
    }

    public int GetBulletsLeft()
    {
        return bulletsLeft;
    }
}
