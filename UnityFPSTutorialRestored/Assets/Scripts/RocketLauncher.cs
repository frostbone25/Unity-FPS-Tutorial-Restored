using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class RocketLauncher : MonoBehaviour
{
	public Transform launchPoint;
    public Rigidbody projectile;
    public float initialSpeed = 20.0f;
    public float reloadTime = 0.5f;
    public int ammoCount = 20;
	public int maxAmmoCount = 20;
	public bool infinteAmmo = false;
    private float lastShot = -10.0f;

	public void Fire()
	{
		//force a limit
		if (ammoCount > maxAmmoCount)
			ammoCount = maxAmmoCount;

		if (infinteAmmo)
			ammoCount = maxAmmoCount;

		// Did the time exceed the reload time?
		if (Time.time > reloadTime + lastShot && ammoCount > 0)
		{
			Rigidbody instantiatedProjectile;

			// create a new projectile, use the same position and rotation as the Launcher.
			if (launchPoint)
				instantiatedProjectile = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
			else
				instantiatedProjectile = Instantiate(projectile, transform.position, transform.rotation);

			// Give it an initial forward velocity. The direction is along the z-axis of the missile launcher's transform.
			instantiatedProjectile.velocity = transform.TransformDirection(new Vector3(0, 0, initialSpeed));

			// Ignore collisions between the missile and the character controller
			Collider projectileCollider = instantiatedProjectile.GetComponent<Collider>();
			Collider rootCollider = transform.root.GetComponent<Collider>();

			if(rootCollider && projectileCollider)
				Physics.IgnoreCollision(projectileCollider, rootCollider);

			lastShot = Time.time;
			ammoCount--;
		}
	}
}
