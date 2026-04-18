using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : AbstractSingleton<UnitSelectionManager>
{
    [SerializeField] private Color selectionColor = new Color(0.0f, 1.0f, 0.0f, 0.3f);
    public ShipAction selectAction = ShipAction.PrimaryShipAction;
    public ShipAction sendAction = ShipAction.SecondaryShipAction;
    //[HideInInspector]
    public List<GameObject> selectedEntities = new List<GameObject>();

    private List<EntitySelectionArea> selectableEntities = new List<EntitySelectionArea>();

    private Vector2 selectionStart;
    private bool isSelecting = false;
    private Vector2 selectionStartScreen;
    private Vector2 currentMouseScreen;

    private void Update()
    {
        if (KeybindManager.Instance.IsActionDown(selectAction))
        {
            selectionStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectionStartScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            isSelecting = true;
        }

        if (isSelecting && KeybindManager.Instance.IsActionPressed(selectAction))
        {
            currentMouseScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            // While selecting, could draw rectangle here if needed
        }

        if (KeybindManager.Instance.IsActionUp(selectAction) && isSelecting)
        {
            Vector2 selectionEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RemoveOldContours();
            PerformSelection(selectionStart, selectionEnd);
            SetNewContours();
            isSelecting = false;
        }
    }

    private void RemoveOldContours() => selectedEntities.ForEach(entity => ContourDrawer.Instance.RemoveAlly(entity));

    private void SetNewContours() => selectedEntities.ForEach(entity => ContourDrawer.Instance.AddAlly(entity));

    public void RegisterSelectableEntity(EntitySelectionArea entity)
    {
        selectableEntities.Add(entity);
    }

    private void OnGUI()
    {
        if (isSelecting)
        {
            Vector2 start = selectionStartScreen;
            Vector2 end = currentMouseScreen;
            Vector2 min = Vector2.Min(start, end);
            Vector2 max = Vector2.Max(start, end);
            Rect rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            GUI.color = selectionColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    private void PerformSelection(Vector2 start, Vector2 end)
    {
        selectedEntities.Clear();

        float x = Mathf.Min(start.x, end.x);
        float y = Mathf.Min(start.y, end.y);
        float width = Mathf.Abs(start.x - end.x);
        float height = Mathf.Abs(start.y - end.y);
        
        selectedEntities = GeometryUtils.GetEntitiesInRectangle(selectableEntities, new Rect(x, y, width, height));
    }
}
