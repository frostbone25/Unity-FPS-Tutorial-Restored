using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PassengerShipSequenceTrigger : MonoBehaviour
{
    private bool runOnce = false;

    public PlayableDirector playableDirector;

    private void Awake()
    {
        playableDirector.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Player"))
        {
            if(!runOnce)
            {
                playableDirector.gameObject.SetActive(true);
                playableDirector.RebindPlayableGraphOutputs();
                playableDirector.RebuildGraph();
                playableDirector.Play();

                runOnce = true;
            }
        }
    }
}
