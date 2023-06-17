enum PickupType { Health = 0, Rocket = 1 }
var pickupType = PickupType.Health;
var amount = 20;
var sound : AudioClip;

private var used = false;

function ApplyPickup (player : FPSPlayer) {
	if (pickupType == PickupType.Health) {
		if (player.hitPoints >= player.maximumHitPoints)
			return false;
		
		player.hitPoints += amount;
		player.hitPoints = Mathf.Min(player.hitPoints, player.maximumHitPoints);
	} else if (pickupType == PickupType.Rocket) {
		var launcher : RocketLauncher = player.GetComponentInChildren(RocketLauncher);
		if (launcher)
			launcher.ammoCount += amount;
	}
	
	return true;
}

function OnTriggerEnter (col : Collider) {
	var player : FPSPlayer = col.GetComponent(FPSPlayer);
	
	//* Make sure we are running into a player
	//* prevent picking up the trigger twice, because destruction
	//  might be delayed until the animation has finnished
	if (used || player == null)
		return;
	
	if (!ApplyPickup (player))
		return;
	used = true;
	
	// Play sound
	if (sound)
		AudioSource.PlayClipAtPoint(sound, transform.position);
	
	// If there is an animation attached.
	// Play it.
	if (animation && animation.clip) {
		animation.Play();
		Destroy(gameObject, animation.clip.length);
	} else {
		Destroy(gameObject);
	}
}

// Auto setup the pickup
function Reset () {
	if (collider == null)	
		gameObject.AddComponent(BoxCollider);
	collider.isTrigger = true;
}