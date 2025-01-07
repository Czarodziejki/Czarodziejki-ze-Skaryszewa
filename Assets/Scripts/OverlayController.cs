using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class OverlayController : MonoBehaviour
{
    public bool IsWorking { get; private set; } = false;


    private Image overlay;
    private Coroutine coroutine;


    private void Start()
    {
        if (!TryGetComponent(out overlay))
            Debug.LogError("Cannot find image in an overlay");
    }


    public void StartFadeIn(float fadeInTime, float targetAlpha = 1.0f)
    {
        if (IsWorking)
        {
            StopCoroutine(coroutine);
        } 

        IsWorking = true;
        coroutine = StartCoroutine(FadeInCoroutine(fadeInTime, targetAlpha));
    }


    public void StartFadeOut(float fadeOutTime, float targetAlpha = 0.0f)
    {
        if (IsWorking)
        {
            StopCoroutine(coroutine);
        }

        IsWorking = true;
        coroutine = StartCoroutine(FadeOutCoroutine(fadeOutTime, targetAlpha));
    }


    private IEnumerator FadeInCoroutine(float t, float targetAlpha)
    {
        float alphaToGo = targetAlpha - overlay.color.a;
        if (alphaToGo < 0 || alphaToGo > 1)
        {
            IsWorking = false;
            yield break;
        }

        while (overlay.color.a < targetAlpha)
        {
            overlay.color = new Color(
                overlay.color.r,
                overlay.color.g,
                overlay.color.b,
                overlay.color.a + (Time.deltaTime / t) * alphaToGo
            );

            yield return null;
        }

        IsWorking = false;
    }


    private IEnumerator FadeOutCoroutine(float t, float targetAlpha)
    {
        float alphaToGo = overlay.color.a - targetAlpha;
        if (alphaToGo < 0 || alphaToGo > 1)
        {
            IsWorking = false;
            yield break;
        }

        while (overlay.color.a > targetAlpha)
        {
            overlay.color = new Color(
                overlay.color.r,
                overlay.color.g,
                overlay.color.b,
                overlay.color.a - (Time.deltaTime / t) * alphaToGo
            );

            yield return null;
        }

        IsWorking = false;
    }
}
