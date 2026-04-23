using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Shortcut Database", menuName = "Shortcuts/Shortcut Database")]
public class ShortcutDatabaseSO : ScriptableObject
{
    [Tooltip("Drag every ShortcutActionSO asset here. This is the master list the manager reads from.")]
    public List<ShortcutActionSO> allShortcuts = new List<ShortcutActionSO>();
}