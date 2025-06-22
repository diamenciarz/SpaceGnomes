using UnityEngine;
using System;
using System.Collections.Generic;

public enum ShipAction
{
    ShootPrimary,
    ShootSecondary,
    ShootTernary,
    ShootUltimate
}

public class KeyListener : MonoBehaviour
{
    [System.Serializable]
    public struct KeyActionPair
    {
        public KeyCode key;
        public ShipAction action;
    }

    [SerializeField] private List<KeyActionPair> keyBindings = new List<KeyActionPair>();
    private Dictionary<KeyCode, ShipAction> keyActionMap = new Dictionary<KeyCode, ShipAction>();
    private Dictionary<ShipAction, List<IWeaponController>> actionControllers = new Dictionary<ShipAction, List<IWeaponController>>();

    private void Awake()
    {
        // Find all IWeaponController components in children
        IWeaponController[] weaponControllers = GetComponentsInChildren<IWeaponController>();

        // Initialize actionControllers dictionary
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            actionControllers[action] = new List<IWeaponController>();
        }

        // Map controllers to their action types
        foreach (var controller in weaponControllers)
        {
            ShipAction action = controller.GetActionType();
            actionControllers[action].Add(controller);
        }

        // Initialize key bindings
        InitializeKeyBindings();
    }

    private void InitializeKeyBindings()
    {
        keyActionMap.Clear();
        foreach (var binding in keyBindings)
        {
            if (keyActionMap.ContainsKey(binding.key))
            {
                Debug.LogWarning($"Duplicate key binding for {binding.key} on {gameObject.name}");
                continue;
            }
            keyActionMap[binding.key] = binding.action;
        }
    }

    private void Update()
    {
        // Track which actions are active
        Dictionary<ShipAction, bool> activeActions = new Dictionary<ShipAction, bool>();
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            activeActions[action] = false;
        }

        // Process key presses
        foreach (var kvp in keyActionMap)
        {
            if (Input.GetKey(kvp.Key))
            {
                activeActions[kvp.Value] = true;
            }
        }

        // Notify controllers based on active actions (Observer Pattern)
        foreach (var action in activeActions)
        {
            if (actionControllers.ContainsKey(action.Key))
            {
                foreach (var controller in actionControllers[action.Key])
                {
                    controller.SetShooting(action.Value);
                }
            }
        }
    }

    // Method to dynamically remap keys at runtime
    public void RemapKey(KeyCode oldKey, KeyCode newKey)
    {
        if (keyActionMap.ContainsKey(oldKey))
        {
            ShipAction action = keyActionMap[oldKey];
            keyActionMap.Remove(oldKey);
            keyActionMap[newKey] = action;

            // Update serialized keyBindings
            var index = keyBindings.FindIndex(b => b.key == oldKey);
            if (index >= 0)
            {
                keyBindings[index] = new KeyActionPair { key = newKey, action = keyBindings[index].action };
            }
        }
    }
}