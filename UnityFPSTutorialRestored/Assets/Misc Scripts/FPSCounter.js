private var show = false;

function Update () {
	if (Input.GetKeyDown("0"))
		show = !show;
	if (show)
	{
		if (Time.timeScale != 0.0)
		{
			var fps : int = 1.0 / Time.deltaTime;
			guiText.text = fps.ToString();
		}
		else
		{
			guiText.text = "0";
		}
	}
	else
	{
		guiText.text = "";	
	}
}

@script RequireComponent(GUIText)