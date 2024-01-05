using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class CharacterDamage : MonoBehaviour
{
    public float hitPoints = 100;
    public Transform deadReplacement;
    public AudioClip dieSound;

	public void ApplyDamage(float damage)
	{
		// We already have less than 0 hitpoints, maybe we got killed already?
		if (hitPoints <= 0.0)
			return;

		hitPoints -= damage;

		if (hitPoints <= 0.0)
		{
			Detonate();
		}
	}

	public void Detonate()
	{
		// Destroy ourselves
		Destroy(gameObject);

		// Play a dying audio clip
		if (dieSound)
			AudioSource.PlayClipAtPoint(dieSound, transform.position);

		// Replace ourselves with the dead body
		if (deadReplacement)
		{
			Transform dead = Instantiate(deadReplacement, transform.position, transform.rotation);

			// Copy position & rotation from the old hierarchy into the dead replacement
			CopyTransformsRecurse(transform, dead);
		}
	}

	private static void CopyTransformsRecurse(Transform src, Transform dst)
	{
		dst.position = src.position;
		dst.rotation = src.rotation;

		foreach(Transform child in dst)
		{
			// Match the transform with the same name
			var curSrc = src.Find(child.name);

			if (curSrc)
				CopyTransformsRecurse(curSrc, child);
		}
	}
}
