using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : AbstractSingleton<UnitSelectionManager>
{
    [SerializeField] private Color selectionColor = new Color(0.0f, 1.0f, 0.0f, 0.3f);
    public ShortcutActionSO selectAction;
    public ShortcutActionSO commandAction;
    //[HideInInspector]
    public List<GameObject> selectedEntities = new List<GameObject>();

    private List<EntitySelectionArea> selectableEntities = new List<EntitySelectionArea>();

    private Vector2 selectionStart;
    private bool isSelecting = false;
    private Vector2 selectionStartScreen;
    private Vector2 currentMouseScreen;

    #region Public Methods
    public void RegisterSelectableEntity(EntitySelectionArea entity)
    {
        selectableEntities.Add(entity);
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if (selectAction == null) Debug.LogError("Select action not assigned in UnitSelectionManager. Please assign it in the inspector.");
        if (commandAction == null) Debug.LogError("Command action not assigned in UnitSelectionManager. Please assign it in the inspector.");
    }
    private void Update()
    {
        HandleSelection();
        HandleCommand();
    }
    private void HandleSelection()
    {
        if (selectAction.IsDown())
        {
            selectionStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectionStartScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            isSelecting = true;
        }

        if (isSelecting && selectAction.IsPressed())
        {
            currentMouseScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            // While selecting, could draw rectangle here if needed
        }

        if (isSelecting && selectAction.IsUp())
        {
            Vector2 selectionEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RemoveOldContours();
            PerformSelection(selectionStart, selectionEnd);
            SetNewContours();
            isSelecting = false;
        }
    }
    private void HandleCommand()
    {
        if (commandAction.IsDown() && selectedEntities.Count > 0)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach (var entity in selectedEntities)
            {
                ICommandable commandable = entity.GetComponent<ICommandable>();
                if (commandable != null)
                {
                    commandable.MoveTo(targetPosition);
                }
            }
        }
    }

    private void RemoveOldContours() => selectedEntities.ForEach(entity => ContourDrawer.Instance.RemoveAlly(entity));

    private void SetNewContours() => selectedEntities.ForEach(entity => ContourDrawer.Instance.AddAlly(entity));


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
