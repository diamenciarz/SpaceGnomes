using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractActionController : MonoBehaviour
{
    [HideInInspector]
    public bool isControlledByPlayer;
    [SerializeField] ShortcutActionSO activateOnShortcut;

    #region Public Methods
    public void SetControlledByPlayer(bool isPlayerControlled)
    {
        isControlledByPlayer = isPlayerControlled;
        UpdateListener();
    }
    public abstract void Activate();
    public abstract void Deactivate();
    /// <summary>
    /// When the GameObject this controller is connected to is detached from the main entity, it stop ongoing effects related to the action.
    /// </summary>
    public abstract void Detach();
    #endregion
    
    private void Awake()
    {
        if(!activateOnShortcut) Debug.LogError("ActivateOnShortcut not assigned in " + gameObject.name + ". Please assign it in the inspector.");
    }
    protected void OnEnable()
    {
        UpdateListener();
    }

    protected void OnDisable()
    {
        activateOnShortcut.RemoveListenerOnUp(Activate);
        activateOnShortcut.RemoveListenerOnDown(Deactivate);
    }
    private void UpdateListener()
    {
        if (isControlledByPlayer)
        {
            activateOnShortcut.AddListenerOnDown(Activate);
            activateOnShortcut.AddListenerOnUp(Deactivate);
        }
        else
        {
            activateOnShortcut.RemoveListenerOnDown(Activate);
            activateOnShortcut.RemoveListenerOnUp(Deactivate);
        }
    }
}
