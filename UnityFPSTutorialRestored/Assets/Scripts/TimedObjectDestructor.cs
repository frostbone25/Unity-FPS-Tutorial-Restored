using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class TimedObjectDestructor : MonoBehaviour
{
    public float timeOut = 1.0f;
    public bool detachChildren = false;

    private void Awake()
    {
        Invoke("DestroyNow", timeOut);
    }

    private void DestroyNow()
    {
        if (detachChildren)
        {
            transform.DetachChildren();
        }

        Destroy(gameObject);
    }
}
