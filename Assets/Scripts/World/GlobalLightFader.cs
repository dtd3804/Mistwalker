using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Gắn vào Global Light 2D. LightZone gọi FadeTo để đổi màu và độ sáng của cả màn
[RequireComponent(typeof(Light2D))]
public class GlobalLightFader : MonoBehaviour
{
    private Light2D globalLight;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        globalLight = GetComponent<Light2D>();
    }

    public void FadeTo(Color color, float intensity, float time)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (time <= 0f)
        {
            globalLight.color = color;
            globalLight.intensity = intensity;
            return;
        }

        fadeRoutine = StartCoroutine(FadeRoutine(color, intensity, time));
    }

    private IEnumerator FadeRoutine(Color color, float intensity, float time)
    {
        Color startColor = globalLight.color;
        float startIntensity = globalLight.intensity;
        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / time);

            globalLight.color = Color.Lerp(startColor, color, t);
            globalLight.intensity = Mathf.Lerp(startIntensity, intensity, t);

            yield return null;
        }

        fadeRoutine = null;
    }
}