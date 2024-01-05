using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class ExplosionSimple : MonoBehaviour
{
    public float explosionRadius = 5.0f;
    public float explosionPower = 10.0f;
    public float explosionDamage = 100.0f;
    public float explosionTime = 1.0f;

    // Start is called before the first frame update
    private void Start()
    {
		Vector3 explosionPosition = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
	
		foreach(Collider hit in colliders)
		{
			if (!hit)
				continue;

			if (hit.attachedArticulationBody)
			{
				hit.attachedRigidbody.AddExplosionForce(explosionPower, explosionPosition, explosionRadius, 3.0f);

				var closestPoint = hit.attachedRigidbody.ClosestPointOnBounds(explosionPosition);
				var distance = Vector3.Distance(closestPoint, explosionPosition);

				// The hit points we apply fall decrease with distance from the hit point
				var hitPoints = 1.0 - Mathf.Clamp01(distance / explosionRadius);
				hitPoints *= explosionDamage;

				// Tell the rigidbody or any other script attached to the hit object 
				// how much damage is to be applied!
				hit.attachedRigidbody.SendMessageUpwards("ApplyDamage", hitPoints, SendMessageOptions.DontRequireReceiver);

				hit.SendMessageUpwards("AlertTarget", transform.position, SendMessageOptions.DontRequireReceiver);
			}
		}

		// destroy the explosion
		Destroy(gameObject, explosionTime);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}
