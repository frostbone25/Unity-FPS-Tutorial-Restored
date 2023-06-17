function Start ()
{
	// Start out in paused mode in web player
	
	if (Application.platform == RuntimePlatform.OSXWebPlayer ||
	    Application.platform == RuntimePlatform.WindowsWebPlayer)
	{
		SetPause(true);
	}
	else
	{
		SetPause(false);
		Screen.lockCursor = true;
	}
}

function OnApplicationQuit ()
{
	Time.timeScale = 1;
}

function SetPause (pause : boolean)
{
	Input.ResetInputAxes();
	var gos : Object[] = FindObjectsOfType(GameObject);
	for (var go : GameObject in gos)
		go.SendMessage("DidPause", pause, SendMessageOptions.DontRequireReceiver);
	
	transform.position = Vector3.zero;
	
	if (pause)
	{
		Time.timeScale = 0;
		transform.position = Vector3 (.5, .5, 0);
		guiText.anchor = TextAnchor.MiddleCenter;
	}
	else
	{
		guiText.anchor = TextAnchor.UpperLeft;
		transform.position = Vector3(0, 1, 0);
		Time.timeScale = 1;
	}
}

function DidPause (pause : boolean)
{
	if (pause)
	{
	    // Show the button again
	    guiText.enabled = true;
		guiText.text = "Click to start playing";
	}
	else
	{
	    // Disable the button
	    guiText.enabled = true;
	    guiText.text = "Escape to show the cursor";
	}
}

function OnMouseDown ()
{
    // Lock the cursor
    Screen.lockCursor = true;
}

private var wasLocked = false;

function Update ()
{
	if (Input.GetMouseButton(0))
		Screen.lockCursor = true;
	
    // Did we lose cursor locking?
    // eg. because the user pressed escape
    // or because he switched to another application
    // or because some script set Screen.lockCursor = false;
    if (!Screen.lockCursor && wasLocked)
    {
        wasLocked = false;
        SetPause(true);
    }
    // Did we gain cursor locking?
    else if (Screen.lockCursor && !wasLocked)
    {
        wasLocked = true;
        SetPause(false);
    }
}