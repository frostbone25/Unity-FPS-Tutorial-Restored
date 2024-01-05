using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class DragRigidbody : MonoBehaviour
{
    public float spring = 50.0f;
    public float damper = 5.0f;
    public float drag = 10.0f;
    public float angularDrag = 5.0f;
    public float distance = 0.2f;
    public bool attachToCenterOfMass = false;

    private SpringJoint springJoint;

    // Update is called once per frame
    private void Update()
    {
		// Make sure the user pressed the mouse down
		if (!Input.GetMouseButtonDown(0))
			return;

		Camera mainCamera = Camera.main;

		// We need to actually hit an object
		RaycastHit hit;

		if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100.0f))
			return;

		// We need to hit a rigidbody that is not kinematic
		if (!hit.rigidbody || hit.rigidbody.isKinematic)
			return;

		if (!springJoint)
		{
			GameObject go = new GameObject("Rigidbody dragger");
			Rigidbody body = go.AddComponent<Rigidbody>();
			SpringJoint springJoint = go.AddComponent<SpringJoint>();

			body.isKinematic = true;
		}

		springJoint.transform.position = hit.point;

		if (attachToCenterOfMass)
		{
			Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
			anchor = springJoint.transform.InverseTransformPoint(anchor);
			springJoint.anchor = anchor;
		}
		else
		{
			springJoint.anchor = Vector3.zero;
		}

		springJoint.spring = spring;
		springJoint.damper = damper;
		springJoint.maxDistance = distance;
		springJoint.connectedBody = hit.rigidbody;

		StartCoroutine("DragObject", hit.distance);
	}

	private void DragObject(float distance)
	{
		float oldDrag = springJoint.connectedBody.drag;
		float oldAngularDrag = springJoint.connectedBody.angularDrag;

		springJoint.connectedBody.drag = drag;
		springJoint.connectedBody.angularDrag = angularDrag;

		Camera mainCamera = Camera.main;

		while (Input.GetMouseButton(0))
		{
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			springJoint.transform.position = ray.GetPoint(distance);

			return;
			//yield;
		}

		if (springJoint.connectedBody)
		{
			springJoint.connectedBody.drag = oldDrag;
			springJoint.connectedBody.angularDrag = oldAngularDrag;
			springJoint.connectedBody = null;
		}
	}
}
