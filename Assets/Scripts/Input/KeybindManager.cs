using UnityEngine;
using System.Collections.Generic;

public class KeybindManager : AbstractSingleton<KeybindManager>
{
    [SerializeField] private KeyBindings keyBindings;
    
    protected override void Awake()
    {
        base.Awake();
        if (keyBindings == null)
        {
            Debug.LogError("KeyBindings asset not assigned in KeybindManager. Please assign it in the inspector.");
            return;
        }
        keyBindings.Load(keyBindings.savePath);
    }
    public void ChangeKeyBindings(KeyBindings bindings)
    {
        keyBindings = bindings;
        keyBindings.Load();
    }

    public void SaveKeyBindings()
    {
        if (keyBindings == null)
        {
            Debug.LogError("KeyBindings asset not assigned in KeybindManager. Cannot save keybindings.");
            return;
        }
        keyBindings.Save();
    }

    public bool IsActionPressed(ShipAction action)
    {
        if (keyBindings == null) return false;
        var bindings = keyBindings.GetBindings(action);
        foreach (var binding in bindings)
        {
            if (binding.IsPressed())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsActionDown(ShipAction action)
    {
        if (keyBindings == null) return false;
        var bindings = keyBindings.GetBindings(action);
        foreach (var binding in bindings)
        {
            if (binding.IsDown())
            {
                return true;
            }
        }
        return false;
    }

    public bool IsActionUp(ShipAction action)
    {
        if (keyBindings == null) return false;
        var bindings = keyBindings.GetBindings(action);
        foreach (var binding in bindings)
        {
            if (binding.IsUp())
            {
                return true;
            }
        }
        return false;
    }

    public List<KeyBinding> GetBindings(ShipAction action)
    {
        if (keyBindings == null) return new List<KeyBinding>();
        return keyBindings.GetBindings(action);
    }

    public void SetBinding(ShipAction action, KeyBinding binding, int index = 0)
    {
        if (keyBindings == null) return;
        keyBindings.SetBinding(action, binding, index);
    }

    public void AddBinding(ShipAction action, KeyBinding binding)
    {
        if (keyBindings == null) return;
        keyBindings.AddBinding(action, binding);
    }

    public void RemoveBinding(ShipAction action, int index)
    {
        if (keyBindings == null) return;
        keyBindings.RemoveBinding(action, index);
    }

    public bool HasConflict(KeyBinding binding, ShipAction excludeAction = ShipAction.PrimaryShipAction)
    {
        if (keyBindings == null) return false;
        return keyBindings.HasConflict(binding, excludeAction);
    }

    public KeyBindings GetKeyBindings()
    {
        return keyBindings;
    }
}