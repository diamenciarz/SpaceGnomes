using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ObjectFollower))]
public class ProgressBar : MonoBehaviour
{
    [Header("Appearance Settings")]
    [SerializeField][Tooltip("After delayBeforeDisappear since the last SetProgress() call, will disappear over disappearDuration. If false, will only react to ShowBar() and HideBar()")]
    private bool _showWhenUpdatingProgress;
    public bool showWhenUpdatingProgress
    {
        get => _showWhenUpdatingProgress;
        set
        {
            _showWhenUpdatingProgress = value;
            if (!value) SetAlpha(appearAlpha);
        }
    }
    [SerializeField][Range(0f, 10f)][Tooltip("0 is instant")] float delayBeforeDisappear = 3f;
    [SerializeField][Range(0f, 10f)][Tooltip("0 is instant")] float disappearDuration = 1f;
    [SerializeField][Range(0f,10f)][Tooltip("0 is instant")] float appearDuration = 1f;
    [SerializeField][Range(0f, 1f)][Tooltip("maximum opacity at full visibility")] float appearAlpha = 0.7f;
    [SerializeField][Tooltip("Once full progress, will wait delayBeforeDisappear and disappear over disappearDuration")]
    bool disappearOnlyWhenFullProgress = false;

    private Slider slider;
    private Image fillImage;
    [Tooltip("If true, it will start disappearing after delayBeforeDisappear,\notherwise it will stay visible until ShowBar() or HideBar() is called.\nIt will calculate the alpha based on the time since the last SetProgress() call,\ndelayBeforeDisappear and the disappearDuration.")]
    private bool isDisappearing = false;
    private float lastActivity; // Time of the last SetProgress() call, used to track when to start disappearing
    private bool transitionIsInstant => appearDuration == 0f && disappearDuration == 0f;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
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
        // Updating the progress does not necessarily make the bar visible
        slider.value = progress;
        if(_showWhenUpdatingProgress && isDisappearing)
        {
            float currentAlpha = fillImage.color.a;
            // Calculate the alpha we should be at if it was not disappearing, and use the higher of the two to avoid sudden jumps in opacity
            float calculatedLastActivityTime = transitionIsInstant ? 0f : (Time.time - (1 - currentAlpha / appearAlpha) * disappearDuration - delayBeforeDisappear);
            lastActivity = Time.time;
            isDisappearing = false;
        }
    }
    public void ShowBar()
    {
        lastActivity = Time.time;
        isDisappearing = false;
    }
    public void HideBar()
    {
        lastActivity = Time.time;
        isDisappearing = true;
    }
    private void Update()
    {
        UpdateDisappearingMode();
        HandleDisappearingOnFullProgress();
        UpdateVisibility();
    }
    private void UpdateDisappearingMode()
    {
        if (showWhenUpdatingProgress && !transitionIsInstant)
        {
            //disappearOnlyWhenFullProgress
            // If it is time to start disappearing
            if (disappearOnlyWhenFullProgress)
            {
                if(fillImage.color.a == slider.maxValue) // If we are at full progress, start the timer to disappear
                {
                    if (!isDisappearing && (Time.time - lastActivity) > appearDuration + delayBeforeDisappear)
                    {
                        lastActivity = Time.time;
                        isDisappearing = true;
                    }
                }
            }
            else
            {
                if (!isDisappearing && (Time.time - lastActivity) > appearDuration + delayBeforeDisappear)
                {
                    lastActivity = Time.time;
                    isDisappearing = true;
                } 
            }
        }
    }
    private float CalculateLastActivityTimeFromAlpha(float currentAlpha)
    {
        if(isDisappearing) return Time.time - (1 - currentAlpha / appearAlpha) * disappearDuration - delayBeforeDisappear;
        else return Time.time - (currentAlpha / appearAlpha) * appearDuration;
    }
    private void HandleDisappearingOnFullProgress()
    {
        if (disappearOnlyWhenFullProgress && slider.value >= slider.maxValue && !isDisappearing)
        {
            lastActivity = Time.time;
            isDisappearing = true;
        }
    }
    private void UpdateVisibility()
    {
        if (transitionIsInstant)
        {
            float newAlpha = isDisappearing ? 0f : appearAlpha;
            SetAlpha(newAlpha);
            return;
        }
        if (isDisappearing)
        {
            if(disappearDuration==0) SetAlpha(0f);
            if(Time.time - lastActivity < delayBeforeDisappear) return; // Not time to disappear yet
            float newAlpha = Mathf.Lerp(appearAlpha, 0f, (Time.time - lastActivity - delayBeforeDisappear) / disappearDuration);
            SetAlpha(newAlpha);
        }
        else
        {
            if (appearDuration == 0) SetAlpha(appearAlpha);
            float newAlpha = Mathf.Lerp(0f, appearAlpha, (Time.time - lastActivity) / appearDuration);
            SetAlpha(newAlpha);
        }
    }
    private void SetAlpha(float alpha)
    {
        Color color = fillImage.color;
        color.a = alpha;
        fillImage.color = color;
    }
}
