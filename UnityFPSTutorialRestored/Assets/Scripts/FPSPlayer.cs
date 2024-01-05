using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

/*
The original GUI system has been replaced with the new Unity UI
*/

public class FPSPlayer : MonoBehaviour
{
    public float maximumHitPoints = 100.0f;
    public float hitPoints = 100.0f;

    public Text bulletGUI;
    public Image rocketGUI;
    public RectTransform healthGUI;
	public RectTransform healthGUIParent;
	public ScreenFade screenFade;

    public List<AudioClip> walkSounds = new List<AudioClip>();
    public AudioClip painLittle;
    public AudioClip painBig;
    public AudioClip die;
    public float audioStepLength = 0.3f;

	[HideInInspector]
    public MachineGun machineGun;
	[HideInInspector]
	public RocketLauncher rocketLauncher;

    private float healthGUIWidth = 0.0f;
    private float gotHitTimer = -1.0f;

    public List<Sprite> rocketTextures = new List<Sprite>();

	private AudioSource audioSource;

    private void Awake()
    {
        machineGun = GetComponentInChildren<MachineGun>();
        rocketLauncher = GetComponentInChildren<RocketLauncher>();
		audioSource = GetComponent<AudioSource>();

		healthGUIWidth = healthGUIParent.rect.width;

		//StartCoroutine("PlayStepSounds");
	}

	private void LateUpdate()
	{
		// Update gui every frame
		// We do this in late update to make sure machine guns etc. were already executed
		UpdateGUI();
	}

	IEnumerator PlayStepSounds()
	{
		CharacterController controller = GetComponent<CharacterController>();

		while (true)
		{
			if (controller.isGrounded && controller.velocity.magnitude > 0.3)
			{
				audioSource.clip = walkSounds[Random.Range(0, walkSounds.Count)];
				audioSource.Play();

				yield return new WaitForSeconds(audioStepLength);
			}
			else
			{
				
			}
		}
	}

	public void ApplyDamage(float damage)
	{
		if (hitPoints < 0.0)
			return;

		// Apply damage
		hitPoints -= damage;

		// Play pain sound when getting hit - but don't play so often
		if (Time.time > gotHitTimer && painBig && painLittle)
		{
			// Play a big pain sound
			if (hitPoints < maximumHitPoints * 0.2 || damage > 20)
			{
				audioSource.PlayOneShot(painBig, 1.0f / audioSource.volume);
				gotHitTimer = Time.time + Random.Range(painBig.length * 2, painBig.length * 3);
			}
			else
			{
				// Play a small pain sound
				audioSource.PlayOneShot(painLittle, 1.0f / audioSource.volume);
				gotHitTimer = Time.time + Random.Range(painLittle.length * 2, painLittle.length * 3);
			}
		}

		// Are we dead?
		if (hitPoints < 0.0)
			Die();
	}

	private void Die()
	{
		if (die)
			AudioSource.PlayClipAtPoint(die, transform.position);

		// Disable all script behaviours (Essentially deactivating player control)
		Component[] coms = GetComponentsInChildren<MonoBehaviour>();

		foreach(var b in coms)
		{
			MonoBehaviour p = b as MonoBehaviour;

			if (p)
				p.enabled = false;
		}

		screenFade.FadeIn();
		StartCoroutine("ReloadLevel");
	}

	IEnumerator ReloadLevel()
    {
		yield return new WaitForSeconds(3);

		string currentSceneName = SceneManager.GetActiveScene().name;

		SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
    }

	private void UpdateGUI()
	{
		// Update health gui
		// The health gui is rendered using a overlay texture which is scaled down based on health
		// - Calculate fraction of how much health we have left (0...1)
		float healthFraction = Mathf.Clamp01(hitPoints / maximumHitPoints);

		//healthGUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthGUIWidth * healthFraction);
		healthGUI.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, healthGUIWidth * healthFraction);

		// Update machine gun gui
		// Machine gun gui is simply drawn with a bullet counter text
		if (machineGun)
		{
			bulletGUI.text = machineGun.GetBulletsLeft().ToString();
		}

		// Update rocket gui
		// This is changed from the tutorial PDF. You need to assign the 20 Rocket textures found in the GUI/Rockets folder
		// to the RocketTextures property.
		if (rocketLauncher)
		{
			int rocketIndex = Mathf.Clamp(rocketLauncher.ammoCount, 0, rocketLauncher.maxAmmoCount);

			if(rocketIndex < rocketTextures.Count && rocketIndex > -1)
				rocketGUI.sprite = rocketTextures[rocketIndex];
		}
	}
}
