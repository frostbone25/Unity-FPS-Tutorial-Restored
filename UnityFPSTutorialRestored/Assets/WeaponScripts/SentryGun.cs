using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class SentryGun : MonoBehaviour
{
    public float attackRange = 30.0f;
    public float shootAngleDistance = 10.0f;
    public Transform target;

    // Start is called before the first frame update
    private void Start()
    {
        if (target == null && GameObject.FindWithTag("Player"))
            target = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    private void Update()
    {
		if (target == null)
			return;

		if (!CanSeeTarget())
			return;

		// Rotate towards target	
		var targetPoint = target.position;
		var targetRotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);

		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);

		// If we are almost rotated towards target - fire one clip of ammo
		var forward = transform.TransformDirection(Vector3.forward);
		var targetDir = target.position - transform.position;

		if (Vector3.Angle(forward, targetDir) < shootAngleDistance)
			SendMessage("Fire");
	}

    private bool CanSeeTarget()
    {
	    if (Vector3.Distance(transform.position, target.position) > attackRange)
		    return false;

        RaycastHit hit;

	    if (Physics.Linecast (transform.position, target.position, out hit))
		    return hit.transform == target;

	    return false;
    }
}
