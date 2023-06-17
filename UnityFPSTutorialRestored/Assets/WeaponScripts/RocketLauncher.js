var projectile : Rigidbody;
var initialSpeed = 20.0;
var reloadTime = 0.5;
var ammoCount = 20;
private var lastShot = -10.0;

function Fire () {
	// Did the time exceed the reload time?
	if (Time.time > reloadTime + lastShot && ammoCount > 0) {
		// create a new projectile, use the same position and rotation as the Launcher.
		var instantiatedProjectile : Rigidbody = Instantiate (projectile, transform.position, transform.rotation);
			
		// Give it an initial forward velocity. The direction is along the z-axis of the missile launcher's transform.
		instantiatedProjectile.velocity = transform.TransformDirection(Vector3 (0, 0, initialSpeed));

		// Ignore collisions between the missile and the character controller
		Physics.IgnoreCollision(instantiatedProjectile.collider, transform.root.collider);
		
		lastShot = Time.time;
		ammoCount--;
	}
}