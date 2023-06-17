var behaviours : MonoBehaviour[];

function DidPause (pause:boolean) {
	for(var b in behaviours)
	{
		b.enabled = !pause;
	}
}