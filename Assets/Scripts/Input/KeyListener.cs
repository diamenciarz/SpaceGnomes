using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// KeyListener is responsible for listening to player input and notifying the appropriate action controllers based on the current key bindings defined in KeybindManager.
/// </summary>
public class KeyListener : MonoBehaviour
{
    private Dictionary<ShipAction, List<AbstractActionController>> actionControllers = new Dictionary<ShipAction, List<AbstractActionController>>();

    private void Awake()
    {
        // Find all IWeaponController components in children
        AbstractActionController[] weaponControllers = GetComponentsInChildren<AbstractActionController>();

        // Initialize actionControllers dictionary
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            actionControllers[action] = new List<AbstractActionController>();
        }

        // Map controllers to their action types
        foreach (var controller in weaponControllers)
        {
            ShipAction action = controller.GetActionType();
            actionControllers[action].Add(controller);
        }
    }

    private void Update()
    {
        // Turn off all actions initially
        Dictionary<ShipAction, bool> activeActions = new Dictionary<ShipAction, bool>();
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            activeActions[action] = false;
        }

        // Check actions using KeybindManager
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            if (KeybindManager.Instance.IsActionPressed(action))
            {
                activeActions[action] = true;
            }
        }

        // Notify controllers based on active actions (Observer Pattern)
        foreach (var action in activeActions)
        {
            if (actionControllers.ContainsKey(action.Key))
            {
                foreach (AbstractActionController controller in actionControllers[action.Key])
                {
                    if (controller.isControlledByPlayer) controller.SetAction(action.Value, null);
                }
            }
        }
    }

    // Method to dynamically remap keys at runtime
    public void RemapKey(KeyCode oldKey, KeyCode newKey)
    {
        // Find the action for oldKey
        foreach (ShipAction action in Enum.GetValues(typeof(ShipAction)))
        {
            var bindings = KeybindManager.Instance.GetBindings(action);
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].key == oldKey)
                {
                    KeyBinding newBinding = new KeyBinding { key = newKey, modifiers = new KeyCode[0] };
                    KeybindManager.Instance.SetBinding(action, newBinding, i);
                    return;
                }
            }
        }
    }
}