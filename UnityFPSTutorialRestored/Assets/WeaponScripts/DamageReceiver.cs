using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

[RequireComponent(typeof(Rigidbody))]
public class DamageReceiver : MonoBehaviour
{
    public float hitPoints = 100.0f;
    public float detonationDelay = 0.0f;
    public Transform explosion;
    public Rigidbody deadReplacement;

	public int particleSystemEmitCount = 5; //new

	private Rigidbody rigidbody; //new

    private void Awake()
    {
		rigidbody = GetComponent<Rigidbody>();
	}

    public void ApplyDamage(float damage)
	{
		// We already have less than 0 hitpoints, maybe we got killed already?
		if (hitPoints <= 0.0)
			return;

		hitPoints -= damage;
		if (hitPoints <= 0.0)
		{
			// Start emitting particles
			ParticleSystem emitter = GetComponentInChildren< ParticleSystem>();

			if (emitter)
				emitter.Emit(particleSystemEmitCount);

			Invoke("DelayedDetonate", detonationDelay);
		}
	}

	public void DelayedDetonate()
	{
		BroadcastMessage("Detonate");
	}

	public void Detonate()
	{
		// Destroy ourselves
		Destroy(gameObject);

		// Create the explosion
		if (explosion)
			Instantiate(explosion, transform.position, transform.rotation);

		// If we have a dead barrel then replace ourselves with it!
		if (deadReplacement)
		{
			Rigidbody dead = Instantiate(deadReplacement, transform.position, transform.rotation);

			// For better effect we assign the same velocity to the exploded barrel
			dead.velocity = rigidbody.velocity;
			dead.angularVelocity = rigidbody.angularVelocity;
		}

		// If there is a particle emitter stop emitting and detach so it doesnt get destroyed
		// right away
		ParticleSystem emitter = GetComponentInChildren<ParticleSystem>();

		if (emitter)
		{
			emitter.Stop();
			emitter.transform.parent = null;
		}
	}
}
