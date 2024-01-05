using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
NOTE: This is a new script not apart of the original FPS project.
*/

public class RobotAnimationEvents : MonoBehaviour
{
    public AI ai;
    public List<AudioClip> footstepSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RobotFireEvent()
    {
        ai.Shoot();
    }

    public void RobotFootstepEvent()
    {
        int randomIndex = Random.Range(0, footstepSounds.Count);

        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }
}
