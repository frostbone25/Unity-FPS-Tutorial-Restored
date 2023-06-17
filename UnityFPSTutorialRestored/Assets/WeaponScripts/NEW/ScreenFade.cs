using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public Image GUIFade;
    public float fadeTime;
    public bool fadeOutOnAwake;

    private bool fade;

    private void Awake()
    {
        if(fadeOutOnAwake)
        {
            GUIFade.color = new Color(1, 1, 1, 1);

            FadeOut();
        }
    }

    public void FadeIn()
    {
        fade = true;
    }

    public void FadeOut()
    {
        fade = false;
    }

    private void Update()
    {
        if(fade)
        {
            GUIFade.color = Color.Lerp(GUIFade.color, new Color(1, 1, 1, 1), Time.deltaTime * fadeTime);
        }
        else
        {
            GUIFade.color = Color.Lerp(GUIFade.color, new Color(1, 1, 1, 0), Time.deltaTime * fadeTime);
        }
    }
}
