var range = 100.0;
var fireRate = 0.05;
var force = 10.0;
var damage = 5.0;
var bulletsPerClip = 40;
var clips = 20;
var reloadTime = 0.5;
private var hitParticles : ParticleEmitter;
var muzzleFlash : Renderer;

private var bulletsLeft : int = 0;
private var nextFireTime = 0.0;
private var m_LastFrameShot = -1;

function Start () {
	hitParticles = GetComponentInChildren(ParticleEmitter);
	
	// We don't want to emit particles all the time, only when we hit something.
	if (hitParticles)
		hitParticles.emit = false;
	bulletsLeft = bulletsPerClip;
}

function LateUpdate() {
	if (muzzleFlash) {
		// We shot this frame, enable the muzzle flash
		if (m_LastFrameShot == Time.frameCount) {
			muzzleFlash.transform.localRotation = Quaternion.AngleAxis(Random.value * 360, Vector3.forward);
			muzzleFlash.enabled = true;

			if (audio) {
				if (!audio.isPlaying)
					audio.Play();
				audio.loop = true;
			}
		} else {
		// We didn't, disable the muzzle flash
			muzzleFlash.enabled = false;
			enabled = false;
			
			// Play sound
			if (audio)
			{
				audio.loop = false;
			}
		}
	}
}

function Fire () {
	if (bulletsLeft == 0)
		return;
	
	// If there is more than one bullet between the last and this frame
	// Reset the nextFireTime
	if (Time.time - fireRate > nextFireTime)
		nextFireTime = Time.time - Time.deltaTime;
	
	// Keep firing until we used up the fire time
	while( nextFireTime < Time.time && bulletsLeft != 0) {
		FireOneShot();
		nextFireTime += fireRate;
	}
}

function FireOneShot () {
	var direction = transform.TransformDirection(Vector3.forward);
	var hit : RaycastHit;
	
	// Did we hit anything?
	if (Physics.Raycast (transform.position, direction, hit, range)) {
		// Apply a force to the rigidbody we hit
		if (hit.rigidbody)
			hit.rigidbody.AddForceAtPosition(force * direction, hit.point);
		
		// Place the particle system for spawing out of place where we hit the surface!
		// And spawn a couple of particles
		if (hitParticles) {
			hitParticles.transform.position = hit.point;
			hitParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			hitParticles.Emit();
		}

		// Send a damage message to the hit object			
		hit.collider.SendMessageUpwards("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
	}
	
	bulletsLeft--;

	// Register that we shot this frame,
	// so that the LateUpdate function enabled the muzzleflash renderer for one frame
	m_LastFrameShot = Time.frameCount;
	enabled = true;
	
	// Reload gun in reload Time		
	if (bulletsLeft == 0)
		Reload();			
}

function Reload () {

	// Wait for reload time first - then add more bullets!
	yield WaitForSeconds(reloadTime);

	// We have a clip left reload
	if (clips > 0) {
		clips--;
		bulletsLeft = bulletsPerClip;
	}
}

function GetBulletsLeft () {
	return bulletsLeft;
}