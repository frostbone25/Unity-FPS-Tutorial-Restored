using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class InstantDeathCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FPSPlayer player = other.GetComponent<FPSPlayer>();

        if (player)
        {
            player.ApplyDamage(10000);
        }
        else if (other.attachedRigidbody)
        {
            Destroy(other.gameObject);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    private void Reset()
    {
        
    }
}
