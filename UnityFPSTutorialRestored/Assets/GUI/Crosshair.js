var crosshairTexture : Texture2D;
var position : Rect;

function Start()
{
		position = Rect( ( Screen.width - crosshairTexture.width ) / 2, ( Screen.height - crosshairTexture.height ) / 2, crosshairTexture.width, crosshairTexture.height );
}

function OnGUI()
{
	GUI.DrawTexture(position, crosshairTexture);	
}
