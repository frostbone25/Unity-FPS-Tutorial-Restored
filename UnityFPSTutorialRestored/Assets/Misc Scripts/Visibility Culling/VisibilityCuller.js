// Storage for all active visibility culling areas
static var areas = ArrayList();
var alwaysEnabledLayers : LayerMask = 1; 
var outsideAreaLayers : LayerMask = -1; 
function OnPreCull () {
	// Calculate the culling mask
	// Go through all areas and take the union of all visible layers
	var layerMask = 0;
	var cameraPos = transform.position;
	var anyArea = false;
	for (var area : VisibilityCullingArea in areas)
	{
		var areaTransform : Transform = area.transform;
		var relative = areaTransform.InverseTransformPoint(cameraPos);
		var bounds = new Bounds (Vector3.zero, transform.localScale);
		if (bounds.Contains(relative))
		{
			layerMask |= area.visibleLayers.value;
			anyArea = true;
		}
	}
	// If we are not in a visibility area. We render everything	
	layerMask |= alwaysEnabledLayers.value;
	if (!anyArea)
		layerMask = outsideAreaLayers.value;	

	camera.cullingMask = layerMask;
}

