function DidPause (pause : boolean)
{
	if (guiTexture)
		guiTexture.enabled = !pause;
	if (guiText)
		guiText.enabled = !pause;
}
