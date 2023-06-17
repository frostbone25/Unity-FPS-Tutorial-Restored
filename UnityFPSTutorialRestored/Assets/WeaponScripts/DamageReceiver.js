var hitPoints = 100.0;
var detonationDelay = 0.0;
var explosion : Transform;
var deadReplacement : Rigidbody;

function ApplyDamage (damage : float) {
	// We already have less than 0 hitpoints, maybe we got killed already?
	if (hitPoints <= 0.0)
		return;
		
	hitPoints -= damage;
	if (hitPoints <= 0.0) {
		// Start emitting particles
		var emitter : ParticleEmitter = GetComponentInChildren(ParticleEmitter);
		if (emitter)
			emitter.emit = true;

		Invoke("DelayedDetonate", detonationDelay);
	}
}

function DelayedDetonate () {
	BroadcastMessage ("Detonate");
}

function Detonate () {
	// Destroy ourselves
	Destroy(gameObject);

	// Create the explosion
	if (explosion)
		Instantiate (explosion, transform.position, transform.rotation);

	// If we have a dead barrel then replace ourselves with it!
	if (deadReplacement) {
		var dead : Rigidbody = Instantiate(deadReplacement, transform.position, transform.rotation);

		// For better effect we assign the same velocity to the exploded barrel
		dead.rigidbody.velocity = rigidbody.velocity;
		dead.angularVelocity = rigidbody.angularVelocity;
	}
	
	// If there is a particle emitter stop emitting and detach so it doesnt get destroyed
	// right away
	var emitter : ParticleEmitter = GetComponentInChildren(ParticleEmitter);
	if (emitter) {
		emitter.emit = false;
		emitter.transform.parent = null;
	}
}

// We require the barrel to be a rigidbody, so that it can do nice physics
@script RequireComponent (Rigidbody)