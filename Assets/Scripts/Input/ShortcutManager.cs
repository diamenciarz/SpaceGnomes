using UnityEngine;

public class ShortcutManager : AbstractSingleton<ShortcutManager>
{
    [SerializeField] private ShortcutDatabaseSO database;
    private void Update()
    {
        if (database == null || database.allShortcuts == null) return;

        foreach (var shortcut in database.allShortcuts)
        {
            if (shortcut == null) continue;

            if (shortcut.IsDown()) shortcut.TriggerDown();
            if (shortcut.IsUp()) shortcut.TriggerUp();
            if (shortcut.IsPressed()) shortcut.TriggerPressed();
        }
    }
}