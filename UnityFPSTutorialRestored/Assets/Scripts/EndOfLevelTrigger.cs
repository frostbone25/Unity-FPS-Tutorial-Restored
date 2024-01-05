using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FPSPlayer player = other.gameObject.GetComponent<FPSPlayer>();

        if(player != null)
        {
            player.screenFade.FadeIn();
            player.StartCoroutine("ReloadLevel");
        }
    }
}
