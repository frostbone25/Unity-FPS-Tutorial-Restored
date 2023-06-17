var visibleLayers : LayerMask = 0;
var drawSolidGizmo = false;

function OnEnable () {
	VisibilityCuller.areas.Add (this);
}

function OnDisable () {
	VisibilityCuller.areas.Remove (this);
}

function OnDrawGizmosSelected ()
{
	// Draw camera outline
	Gizmos.color = Color.yellow;
	GL.PushMatrix();
	GL.MultMatrix(transform.localToWorldMatrix);
	if (drawSolidGizmo)
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	else
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	GL.PopMatrix();
}