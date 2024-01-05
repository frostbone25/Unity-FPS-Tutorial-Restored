using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Physics_Audio : MonoBehaviour
{
    [Header("Impacts")]
    public float impulseForceThreshold;

    public List<AudioClip> impactSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude > impulseForceThreshold)
        {
            if (audioSource.isActiveAndEnabled)
            {
                int randomInt = Random.Range(0, impactSounds.Count - 1);

                audioSource.PlayOneShot(impactSounds[randomInt]);
            }
        }
    }

}
