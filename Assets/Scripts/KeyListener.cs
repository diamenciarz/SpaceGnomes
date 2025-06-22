using UnityEngine;
using System;
using System.Collections.Generic;

public enum ShipAction
{
    ThrustForward,
    ThrustBackward,
    TurnLeft,
    TurnRight,
    Shoot
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
    private Dictionary<KeyCode, Action> keyActionMap = new Dictionary<KeyCode, Action>();

    private ShipController shipController;
    private IWeaponController weaponController;

    private void Awake()
    {
        shipController = GetComponent<ShipController>();
        weaponController = GetComponentInChildren<IWeaponController>();

        if (shipController == null)
        {
            Debug.LogWarning("ShipController not found on GameObject!");
        }
        if (weaponController == null)
        {
            Debug.LogWarning("IWeaponController not found in children!");
        }

        // Initialize key bindings
        InitializeKeyBindings();
    }

    private void InitializeKeyBindings()
    {
        keyActionMap.Clear();
        foreach (var binding in keyBindings)
        {
            switch (binding.action)
            {
                case ShipAction.ThrustForward:
                    keyActionMap[binding.key] = () => shipController?.SetThrustInput(1f);
                    break;
                case ShipAction.ThrustBackward:
                    keyActionMap[binding.key] = () => shipController?.SetThrustInput(-1f);
                    break;
                case ShipAction.TurnLeft:
                    keyActionMap[binding.key] = () => shipController?.SetSteerInput(-1f);
                    break;
                case ShipAction.TurnRight:
                    keyActionMap[binding.key] = () => shipController?.SetSteerInput(1f);
                    break;
                case ShipAction.Shoot:
                    keyActionMap[binding.key] = () => weaponController?.SetShooting(true);
                    break;
            }
        }
    }

    private void Update()
    {
        bool isShooting = false;
        bool isThrustingForward = false;
        bool isThrustingBackward = false;
        bool isTurningLeft = false;
        bool isTurningRight = false;

        foreach (var kvp in keyActionMap)
        {
            if (Input.GetKey(kvp.Key))
            {
                kvp.Value?.Invoke();
                var binding = keyBindings.Find(b => b.key == kvp.Key);
                switch (binding.action)
                {
                    case ShipAction.Shoot:
                        isShooting = true;
                        break;
                    case ShipAction.ThrustForward:
                        isThrustingForward = true;
                        break;
                    case ShipAction.ThrustBackward:
                        isThrustingBackward = true;
                        break;
                    case ShipAction.TurnLeft:
                        isTurningLeft = true;
                        break;
                    case ShipAction.TurnRight:
                        isTurningRight = true;
                        break;
                }
            }
        }

        // Update input states
        float thrustInput = (isThrustingForward ? 1f : 0f) + (isThrustingBackward ? -1f : 0f);
        float steerInput = (isTurningRight ? 1f : 0f) + (isTurningLeft ? -1f : 0f);
        shipController?.SetThrustInput(thrustInput);
        shipController?.SetSteerInput(steerInput);

        // Stop shooting if shoot key is not pressed
        if (!isShooting)
        {
            weaponController?.SetShooting(false);
        }
    }

    // Method to dynamically remap keys at runtime
    public void RemapKey(KeyCode oldKey, KeyCode newKey)
    {
        if (keyActionMap.ContainsKey(oldKey))
        {
            Action action = keyActionMap[oldKey];
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