var maximumHitPoints = 100.0;
var hitPoints = 100.0;

var bulletGUI : GUIText;
var rocketGUI : DrawRockets;
var healthGUI : GUITexture;

var walkSounds : AudioClip[];
var painLittle : AudioClip;
var painBig : AudioClip;
var die : AudioClip;
var audioStepLength = 0.3;

private var machineGun : MachineGun;
private var rocketLauncher : RocketLauncher;
private var healthGUIWidth = 0.0;
private var gotHitTimer = -1.0;

var rocketTextures : Texture[];

function Awake () {
	machineGun = GetComponentInChildren(MachineGun);
	rocketLauncher = GetComponentInChildren(RocketLauncher);
	
	PlayStepSounds();

	healthGUIWidth = healthGUI.pixelInset.width;
}

function ApplyDamage (damage : float) {
	if (hitPoints < 0.0)
		return;

	// Apply damage
	hitPoints -= damage;

	// Play pain sound when getting hit - but don't play so often
	if (Time.time > gotHitTimer && painBig && painLittle) {
		// Play a big pain sound
		if (hitPoints < maximumHitPoints * 0.2 || damage > 20) {
			audio.PlayOneShot(painBig, 1.0 / audio.volume);
			gotHitTimer = Time.time + Random.Range(painBig.length * 2, painBig.length * 3);
		} else {
			// Play a small pain sound
			audio.PlayOneShot(painLittle, 1.0 / audio.volume);
			gotHitTimer = Time.time + Random.Range(painLittle.length * 2, painLittle.length * 3);
		}
	}

	// Are we dead?
	if (hitPoints < 0.0)
		Die();
}

function Die () {
	if (die)
		AudioSource.PlayClipAtPoint(die, transform.position);
	
	// Disable all script behaviours (Essentially deactivating player control)
	var coms : Component[] = GetComponentsInChildren(MonoBehaviour);
	for (var b in coms) {
		var p : MonoBehaviour = b as MonoBehaviour;
		if (p)
			p.enabled = false;
	}

	LevelLoadFade.FadeAndLoadLevel(Application.loadedLevel, Color.white, 2.0);
}

function LateUpdate () {
	// Update gui every frame
	// We do this in late update to make sure machine guns etc. were already executed
	UpdateGUI();
}

function PlayStepSounds () {
	var controller : CharacterController = GetComponent(CharacterController);

	while (true) {
		if (controller.isGrounded && controller.velocity.magnitude > 0.3) {
			audio.clip = walkSounds[Random.Range(0, walkSounds.length)];
			audio.Play();
			yield WaitForSeconds(audioStepLength);
		} else {
			yield;
		}
	}
}


function UpdateGUI () {
	// Update health gui
	// The health gui is rendered using a overlay texture which is scaled down based on health
	// - Calculate fraction of how much health we have left (0...1)
	var healthFraction = Mathf.Clamp01(hitPoints / maximumHitPoints);

	// - Adjust maximum pixel inset based on it
	healthGUI.pixelInset.xMax = healthGUI.pixelInset.xMin + healthGUIWidth * healthFraction;

	// Update machine gun gui
	// Machine gun gui is simply drawn with a bullet counter text
	if (machineGun) {
		bulletGUI.text = machineGun.GetBulletsLeft().ToString();
	}
	
	// Update rocket gui
	// This is changed from the tutorial PDF. You need to assign the 20 Rocket textures found in the GUI/Rockets folder
	// to the RocketTextures property.
	if (rocketLauncher)	{
		rocketGUI.UpdateRockets(rocketLauncher.ammoCount);
		/*if (rocketTextures.Length == 0) {
			Debug.LogError ("The tutorial was changed with Unity 2.0 - You need to assign the 20 Rocket textures found in the GUI/Rockets folder to the RocketTextures property.");
		} else {
			rocketGUI.texture = rocketTextures[rocketLauncher.ammoCount];
		}*/
	}
}