using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ObjectFollower))]
public class ProgressBar : MonoBehaviour
{
    [Header("Appearance Settings")]
    [SerializeField][Tooltip("If true, will wait delayBeforeDisappear since the last SetProgress() call and then disappear over disappearDuration. If false, will now try to automatically disappear after a delay")]
    private bool showWhenUpdatingProgress;
    [SerializeField][Range(0f, 10f)][Tooltip("0 is instant")] float delayBeforeDisappear = 3f;
    [SerializeField][Range(0f, 10f)][Tooltip("0 is instant")] float disappearDuration = 1f;
    [SerializeField][Range(0f,10f)][Tooltip("0 is instant")] float appearDuration = 1f;
    [SerializeField][Range(0f, 1f)][Tooltip("maximum opacity at full visibility")] float appearAlpha = 0.7f;
    [SerializeField][Tooltip("Once full progress, will wait delayBeforeDisappear and disappear over disappearDuration")]
    bool disappearOnlyWhenFullProgress = false;

    [Header("Color Transition Settings")]
    [SerializeField][Range(0f, 10f)][Tooltip("0 is instant")] float colorTransitionDuration = 1f;


    private Slider slider;
    private Image fillImage;
    private Coroutine currentTransition;
    private Coroutine autoHideCoroutine;
    private float transitionStartAlpha;
    private float transitionTargetAlpha;
    private float transitionDuration;
    private bool shouldAutoHideAfterAppear = false;

    private void Awake()
    {
        AssignComponents();
        transitionStartAlpha = 0f;
        transitionTargetAlpha = 0f;
    }
    private void AssignComponents()
    {
        slider = GetComponent<Slider>();
        if(!slider) Debug.LogError("ProgressBar requires a Slider component on the same GameObject.");
        if(!slider.fillRect)  Debug.LogError("ProgressBar's Slider component must have a Fill Rect assigned.");
        fillImage = slider.fillRect.GetComponent<Image>();
    }
    public void SetColor(Color color)
    {
        float alpha = fillImage.color.a;
        color.a = alpha;
        fillImage.color = color;
    }
    public void Initialize(float alpha, float scale, float progress)
    {
        SetAlpha(appearAlpha);
        SetScale(scale);
        SetProgress(progress);
    }
    public void SetScale(float scale)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(scale, scale, scale);
    }
    public void SetProgress(float progress)
    {
        slider.value = progress;
        if (showWhenUpdatingProgress)
        {
            ShowBarAndHideAfterDelay();
        }
    }
    public void ShowBarAndHideAfterDelay()
    {
        shouldAutoHideAfterAppear = true;
        ShowBar();
    }
    public void ShowBar()
    {
        if (currentTransition != null && transitionTargetAlpha == appearAlpha) return;
        StartAlphaTransition(0f, appearAlpha, appearDuration);
    }
    public void HideBar()
    {
        if (currentTransition != null && transitionTargetAlpha == 0f) return;
        StartAlphaTransition(appearAlpha, 0f, disappearDuration);
    }
    // Private methods
    private void HideAfterDelay()
    {
        if (autoHideCoroutine != null) StopCoroutine(autoHideCoroutine);
        autoHideCoroutine = StartCoroutine(AutoHide());
    }
    private void SetAlpha(float alpha)
    {
        Color color = fillImage.color;
        color.a = alpha;
        fillImage.color = color;
    }
    private void StartAlphaTransition(float start, float target, float dur)
    {
        float currentAlpha = fillImage.color.a;
        if (currentAlpha == target) return;

        transitionTargetAlpha = target;
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            float remainingProportion = (transitionTargetAlpha - currentAlpha) / (transitionTargetAlpha - start);
            dur = dur * remainingProportion;
        }
        transitionStartAlpha = currentAlpha;
        transitionDuration = dur;
        currentTransition = StartCoroutine(AlphaTransition());
    }
    private IEnumerator AlphaTransition()
    {
        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float alpha = Mathf.Lerp(transitionStartAlpha, transitionTargetAlpha, t);
            SetAlpha(alpha);
        }
        SetAlpha(transitionTargetAlpha);
        currentTransition = null;
        if (transitionTargetAlpha == appearAlpha && shouldAutoHideAfterAppear)
        {
            shouldAutoHideAfterAppear = false;
            HideAfterDelay();
        }
    }
    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(delayBeforeDisappear);
        if (!disappearOnlyWhenFullProgress || slider.value >= 1f)
        {
            HideBar();
        }
    }
}
