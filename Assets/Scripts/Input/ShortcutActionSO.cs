using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Shortcut Action", menuName = "Shortcuts/Shortcut Action")]
public class ShortcutActionSO : ScriptableObject
{
    [Tooltip("Human-readable name for editor convenience")]
    public string displayName = "Unnamed Shortcut";
    [Tooltip("Category for grouping shortcuts")]
    public string shortcutCategory = "Unnamed Category";

    [Header("Keyboard Binding")]
    [Tooltip("The main key that must be pressed down this frame")]
    public KeyCode mainKey = KeyCode.None;

    [Tooltip("All modifier keys that must be held (any number, including zero). " +
             "Left/Right variants are treated separately if you add them.")]
    public KeyCode[] modifiers = new KeyCode[0];

    private UnityEvent onUp = new UnityEvent();
    private UnityEvent onDown = new UnityEvent();
    private UnityEvent onPressed = new UnityEvent();

    public void TriggerUp() => onUp?.Invoke();
    public void TriggerDown() => onDown?.Invoke();
    public void TriggerPressed() => onPressed?.Invoke();

    public void AddListenerOnUp(UnityAction listener) => onUp.AddListener(listener);
    public void RemoveListenerOnUp(UnityAction listener) => onUp.RemoveListener(listener);
    public void AddListenerOnDown(UnityAction listener) => onDown.AddListener(listener);
    public void RemoveListenerOnDown(UnityAction listener) => onDown.RemoveListener(listener);
    public void AddListenerOnPressed(UnityAction listener) => onPressed.AddListener(listener);
    public void RemoveListenerOnPressed(UnityAction listener) => onUp.RemoveListener(listener);

    public bool IsDown()
    {
        if (mainKey == KeyCode.None) return false;

        // Check every modifier is currently held
        foreach (var mod in modifiers)
        {
            if (mod == KeyCode.None) continue; // safety
            if (!Input.GetKey(mod))
                return false;
        }

        // Main key must have been pressed this exact frame
        return Input.GetKeyDown(mainKey);
    }
    public bool IsUp()
    {
        if (mainKey == KeyCode.None) return false;
        foreach (var mod in modifiers)
        {
            if (mod == KeyCode.None) continue; // safety
            if (!Input.GetKey(mod))
                return false;
        }
        return Input.GetKeyUp(mainKey);
    }
    public bool IsPressed()
    {
        if (mainKey == KeyCode.None) return false;
        foreach (var mod in modifiers)
        {
            if (mod == KeyCode.None) continue; // safety
            if (!Input.GetKey(mod))
                return false;
        }
        return Input.GetKey(mainKey);
    }
}