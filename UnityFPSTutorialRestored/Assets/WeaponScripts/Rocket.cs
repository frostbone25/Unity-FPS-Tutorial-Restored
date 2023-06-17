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
public class Rocket : MonoBehaviour
{
    // The reference to the explosion prefab
    public GameObject explosion;
    public float timeOut = 3.0f;

    //new
    public bool detatchChildren;
    public AudioClip impactSound;

    // Start is called before the first frame update
    private void Start()
    {
        Invoke("Kill", timeOut);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Instantiate explosion at the impact point and rotate the explosion
        // so that the y-axis faces along the surface normal
        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Instantiate(explosion, contact.point, rotation);

        // And kill our selves
        Kill();
    }

    private void Kill()
    {
        // Stop emitting particles in any children
        ParticleSystem emitter = GetComponentInChildren<ParticleSystem>();

        if (emitter)
            emitter.Stop();

        // Detach children - We do this to detach the trail rendererer which should be set up to auto destruct
        if(detatchChildren)
            transform.DetachChildren();

        //new
        AudioSource.PlayClipAtPoint(impactSound, transform.position);

        // Destroy the projectile
        Destroy(gameObject);
    }
}
