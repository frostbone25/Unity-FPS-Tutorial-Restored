function OnTriggerEnter (col : Collider) {
	var player : FPSPlayer = col.GetComponent(FPSPlayer);
	
	if (player) {
		player.ApplyDamage(10000);
	} else if (col.rigidbody) {	
		Destroy(col.rigidbody.gameObject);
	} else {
		Destroy(col.gameObject);
	}
}

function Reset () {
	if (collider == null)	
		gameObject.AddComponent(BoxCollider);
	collider.isTrigger = true;
}