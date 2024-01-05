using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class Pickup : MonoBehaviour
{
    public enum PickupType 
    {
        Health = 0, 
        Rocket = 1 
    }

    public PickupType pickupType = PickupType.Health;
    public int amount = 20;
    public AudioClip sound;

    private bool used = false;
    private Animation animation;
	private Collider collider;

    private void Awake()
    {
		animation = GetComponent<Animation>();
		collider = GetComponent<Collider>();
	}

    private bool ApplyPickup(FPSPlayer player)
	{
		if (pickupType == PickupType.Health)
		{
			if (player.hitPoints >= player.maximumHitPoints)
				return false;

			player.hitPoints += amount;
			player.hitPoints = Mathf.Clamp(player.hitPoints, 0, player.maximumHitPoints);
		}
		else if (pickupType == PickupType.Rocket)
		{
			RocketLauncher launcher = player.rocketLauncher;

			if (launcher)
				launcher.ammoCount += amount;
		}

		return true;
	}

    private void OnTriggerEnter(Collider other)
    {
		FPSPlayer player = other.GetComponent<FPSPlayer>();

		//* Make sure we are running into a player
		//* prevent picking up the trigger twice, because destruction
		//  might be delayed until the animation has finnished
		if (used || !player)
			return;

		if (!ApplyPickup(player))
			return;

		used = true;

		// Play sound
		if (sound)
			AudioSource.PlayClipAtPoint(sound, transform.position);

		// If there is an animation attached.
		// Play it.
		if (animation && animation.clip)
		{
			animation.Play();

			Destroy(gameObject, animation.clip.length);
		}
		else
		{
			Destroy(gameObject);
		}
	}

    private void Reset()
    {
		if (collider == null)
			gameObject.AddComponent<BoxCollider>();

		collider.isTrigger = true;
	}
}
