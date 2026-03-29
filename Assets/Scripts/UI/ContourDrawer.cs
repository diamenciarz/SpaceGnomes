using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContourDrawer : AbstractSingleton<ContourDrawer>
{
    [SerializeField] public List<GameObject> selectedAllies = new List<GameObject>();
    [SerializeField] public List<GameObject> selectedEnemies = new List<GameObject>();
    [SerializeField] public List<GameObject> selectedNeutrals = new List<GameObject>();
    [SerializeField] private Color allyColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color neutralColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] Material lineMaterial;

    private Dictionary<GameObject, LineRenderer> contourRenderers = new Dictionary<GameObject, LineRenderer>();
    private List<ContourCategory> categories;

    private class ContourCategory
    {
        public List<GameObject> targets;
        public Color color;
    }

    private void Start()
    {
        contourRenderers = new Dictionary<GameObject, LineRenderer>();
        categories = new List<ContourCategory>
        {
            new ContourCategory { targets = selectedAllies, color = allyColor },
            new ContourCategory { targets = selectedEnemies, color = enemyColor },
            new ContourCategory { targets = selectedNeutrals, color = neutralColor }
        };

        // Create renderers for pre-populated targets
        foreach (var cat in categories)
        {
            foreach (var target in cat.targets)
            {
                if (target != null && !contourRenderers.ContainsKey(target))
                {
                    CreateLineRenderer(target, cat.color);
                }
            }
        }
    }

    private void Update()
    {
        DrawContours();
    }

    private void DrawContours()
    {
        List<GameObject> removeObjects = new List<GameObject>();
        foreach (var cat in categories)
        {
            foreach (var target in cat.targets)
            {
                if (!target || !target.activeInHierarchy)
                {
                    DestroyRenderer(target);
                    removeObjects.Add(target);
                    continue;
                }
                Draw(contourRenderers[target], target, cat.color);
            }
        }
        RemoveObjects(removeObjects);
    }
    private void RemoveObjects(List<GameObject> removeObjects)
    {
        foreach (var go in removeObjects)
        {
            foreach (var cat in categories)
            {
                if (cat.targets.Contains(go))
                {
                    cat.targets.Remove(go);
                    break;
                }
            }
        }
    }
    private void Draw(LineRenderer lr, GameObject target, Color color)
    {
        var collider = target.GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError($"GameObject {target.name} does not have a Collider2D component");
            return;
        }
        GeometryUtils.ColliderPoints? colliderPoints = GeometryUtils.CalculateColliderCrossection(collider, target.transform.position, true);
        if (!colliderPoints.HasValue)
        {
            Debug.LogError($"Failed to calculate collider points for {target.name}");
            return;
        }
        Vector2[] points = colliderPoints.Value.points;
        Vector3[] points3 = points.Select(p => (Vector3)p).ToArray();

        lr.positionCount = points3.Length;
        lr.SetPositions(points3);
    }
    public void AddAlly(GameObject go) => AddToCategory(selectedAllies, go, allyColor);
    public void RemoveAlly(GameObject go) => selectedAllies.Remove(go);
    public void AddEnemy(GameObject go) => AddToCategory(selectedEnemies, go, enemyColor);
    public void RemoveEnemy(GameObject go) => selectedEnemies.Remove(go);
    public void AddNeutral(GameObject go) => AddToCategory(selectedNeutrals, go, neutralColor);
    public void RemoveNeutral(GameObject go) => selectedNeutrals.Remove(go);

    private void AddToCategory(List<GameObject> list, GameObject go, Color color)
    {
        if (go == null)
        {
            Debug.LogError($"Cannot add null GameObject");
            return;
        }
        if (list.Contains(go))
        {
            Debug.LogWarning($"GameObject {go.name} is already added");
            return;
        }
        list.Add(go);
        if (!contourRenderers.ContainsKey(go))
        {
            CreateLineRenderer(go, color);
        }
    }
    private void CreateLineRenderer(GameObject go, Color color)
    {
        GameObject child = new GameObject("Contour_" + go.name);
        child.transform.SetParent(transform);
        LineRenderer lr = child.AddComponent<LineRenderer>();
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = lineMaterial;
        lr.sortingOrder = -10; // Ensure it renders behind other elements
        contourRenderers[go] = lr;
    }
    private void DestroyRenderer(GameObject go)
    {
        if(!contourRenderers.ContainsKey(go)) return;
        LineRenderer lr = contourRenderers[go];
        contourRenderers.Remove(go);
        Destroy(lr.gameObject);
    }
}
