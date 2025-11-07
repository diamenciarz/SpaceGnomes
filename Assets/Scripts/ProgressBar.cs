using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ObjectFollower))]
public class ProgressBar : MonoBehaviour
{
    [Header("Appearance Settings")]
    [SerializeField] bool disappearAfterInactive = true;
    [SerializeField] float delayBeforeDisappear = 3f;
    [SerializeField] float disappearDuration = 1f;
    [SerializeField] float appearAlpha = 0.7f;
    [SerializeField] bool disappearOnlyWhenFull = false;

    private Slider slider;
    private Image fillImage;
    private bool disappearing = false;
    private float currentDelay;
    private Coroutine disappearCoroutine;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
        currentDelay = delayBeforeDisappear;
        SetAlpha(appearAlpha);
    }
    public void SetScale(float scale)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(scale, scale, scale);
    }
    public void SetProgress(float progress)
    {
        // Updating the progress does not necessarily make the bar visible
        slider.value = progress;
    }
    public void ShowBar()
    {
        ShowBar(delayBeforeDisappear);
    }
    public void ShowBar(float duration)
    {
        // Showing the bar activates it and resets the disappearance timer
        currentDelay = duration;
        gameObject.SetActive(true);
        SetAlpha(appearAlpha);
        if (disappearCoroutine != null)
        {
            StopCoroutine(disappearCoroutine);
        }
        disappearing = true;
        disappearCoroutine = StartCoroutine(DisappearAfterDelay());
    }
    private void Update()
    {
        if (disappearAfterInactive)
        {
            bool shouldDisappear = disappearOnlyWhenFull ? slider.value >= 1f : true;
            if (shouldDisappear)
            {
                if (!disappearing)
                {
                    disappearing = true;
                    Debug.Log("ProgressBar: Starting disappearance coroutine.");
                    if (disappearCoroutine != null)
                    {
                        StopCoroutine(disappearCoroutine);
                    }
                    disappearCoroutine = StartCoroutine(DisappearAfterDelay());
                }
            }
            else
            {
                disappearing = false;
                SetAlpha(appearAlpha);
            }
        }
    }
    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(currentDelay);
        float startAlpha = fillImage != null ? fillImage.color.a : 1f;
        float time = 0f;
        while (time < disappearDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, time / disappearDuration);
            SetAlpha(alpha);
            yield return null;
        }
        currentDelay = delayBeforeDisappear;
        disappearCoroutine = null;
        gameObject.SetActive(false);
    }
    private void SetAlpha(float alpha)
    {
        if (fillImage != null)
        {
            Color color = fillImage.color;
            color.a = alpha;
            fillImage.color = color;
        }
    }
}
