// Plays back one of the audio choosing randomly between them.
var clips : AudioClip[] = new AudioClip[1];

function Start ()
{
	DontDestroyOnLoad(this);
	audio.loop = false;
	while (true)
	{
		audio.clip = clips[Random.Range(0, clips.length)];
		audio.Play();	
		if (audio.clip)
			yield WaitForSeconds(audio.clip.length);
		else
			yield;
	}
}

@script RequireComponent(AudioSource)