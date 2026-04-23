using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlicker : MonoBehaviour
{
    [SerializeField] private float flickerDuration = 0.1f;
    [SerializeField] private float minWaitTime = 30f;
    [SerializeField] private float maxWaitTime = 60f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Wait for a random interval
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            // Flicker: fade to 0 alpha and back to 1
            yield return FadeAlpha(0f, flickerDuration);
            yield return FadeAlpha(1f, flickerDuration);
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        float startAlpha = spriteRenderer.color.a;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetSpriteAlpha(alpha);
            yield return null;
        }
        SetSpriteAlpha(targetAlpha);
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
