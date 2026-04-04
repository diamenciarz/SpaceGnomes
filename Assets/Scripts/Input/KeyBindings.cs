using UnityEngine;
using System;
using System.Collections.Generic;

public enum ShipAction
{
    PrimaryShipAction,
    SecondaryShipAction,
    TernaryShipAction,
    UltimateShipAction
}

[Serializable]
public struct KeyBinding
{
    public KeyCode key;
    public KeyCode[] modifiers;

    public bool IsPressed()
    {
        // Check if all modifiers are held
        foreach (var mod in modifiers)
        {
            if (!Input.GetKey(mod))
            {
                return false;
            }
        }
        // Check if the key is pressed
        return Input.GetKey(key);
    }

    public bool IsDown()
    {
        foreach (var mod in modifiers)
        {
            if (!Input.GetKey(mod))
            {
                return false;
            }
        }
        return Input.GetKeyDown(key);
    }

    public bool IsUp()
    {
        foreach (var mod in modifiers)
        {
            if (!Input.GetKey(mod))
            {
                return false;
            }
        }
        return Input.GetKeyUp(key);
    }
}

[System.Serializable]
public struct KeyBindingData
{
    public ShipAction action;
    public KeyBinding binding;
}

[CreateAssetMenu(fileName = "KeyBindings", menuName = "ScriptableObjects/Input/KeyBindings")]
public class KeyBindings : PersistentScriptableObject
{
    [SerializeField] private List<KeyBindingData> bindingData = new List<KeyBindingData>();
    [SerializeField] public string savePath = "PersistentData/Keybindings/keybindings.json";

    // The same action can have multiple bindings, so we use a list for each action
    private Dictionary<ShipAction, List<KeyBinding>> actionBindings = new Dictionary<ShipAction, List<KeyBinding>>();

    public void Save()
    {
        base.Save(savePath);
    }
    public void Load() => Load(savePath);
    public override void Load(string filename)
    {
        base.Load(savePath);
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        actionBindings.Clear();
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            actionBindings[action] = new List<KeyBinding>();
        }

        foreach (var data in bindingData)
        {
            actionBindings[data.action].Add(data.binding);
        }
    }

    public List<KeyBinding> GetBindings(ShipAction action)
    {
        if (actionBindings.ContainsKey(action))
        {
            return actionBindings[action];
        }
        return new List<KeyBinding>();
    }

    public void SetBinding(ShipAction action, KeyBinding newBinding, int index = 0)
    {
        if (!actionBindings.ContainsKey(action))
        {
            actionBindings[action] = new List<KeyBinding>();
        }

        if (index < actionBindings[action].Count)
        {
            actionBindings[action][index] = newBinding;
        }
        else
        {
            actionBindings[action].Add(newBinding);
        }

        // Update bindingData
        UpdateBindingData();
    }

    public void AddBinding(ShipAction action, KeyBinding binding)
    {
        if (!actionBindings.ContainsKey(action))
        {
            actionBindings[action] = new List<KeyBinding>();
        }
        actionBindings[action].Add(binding);
        UpdateBindingData();
    }

    public void RemoveBinding(ShipAction action, int index)
    {
        if (actionBindings.ContainsKey(action) && index < actionBindings[action].Count)
        {
            actionBindings[action].RemoveAt(index);
            UpdateBindingData();
        }
    }

    private void UpdateBindingData()
    {
        bindingData.Clear();
        foreach (var kvp in actionBindings)
        {
            foreach (var binding in kvp.Value)
            {
                bindingData.Add(new KeyBindingData { action = kvp.Key, binding = binding });
            }
        }
    }

    public bool HasConflict(KeyBinding binding, ShipAction excludeAction = ShipAction.PrimaryShipAction)
    {
        foreach (var kvp in actionBindings)
        {
            if (kvp.Key == excludeAction) continue;
            foreach (var b in kvp.Value)
            {
                if (BindingsEqual(b, binding))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool BindingsEqual(KeyBinding a, KeyBinding b)
    {
        if (a.key != b.key) return false;
        if (a.modifiers.Length != b.modifiers.Length) return false;
        for (int i = 0; i < a.modifiers.Length; i++)
        {
            if (a.modifiers[i] != b.modifiers[i]) return false;
        }
        return true;
    }
}