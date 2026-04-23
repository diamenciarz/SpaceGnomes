using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathfindingManager : AbstractSingleton<PathfindingManager>
{

    [Header("Pathfinding Settings (2D Space RTS)")]
    [Tooltip("Size of each square tile in world units. Larger = fewer tiles checked per line (faster but coarser avoidance). Tune based on your asteroid/battleship scale.")]
    [SerializeField] private float tileSize = 5f;

    private readonly HashSet<Vector2Int> blockedTiles = new HashSet<Vector2Int>();

    // Hashmap from 
    private readonly Dictionary<GameObject, Vector2> entityDestinations = new Dictionary<GameObject, Vector2>();

    #region Static API for Entity Path Destinations
    public static void SetEntityDestination(GameObject entity, Vector2 destination)
    {
        Instance.entityDestinations.Add(entity, destination);
    }
    public static bool GetEntityDestination(GameObject entity, out Vector2 destination)
    {
        bool found = Instance.entityDestinations.TryGetValue(entity, out Vector2 dest);
        destination = dest;
        return found;
    }
    #endregion

    #region Static API for Obstacle Entities

    /// <summary>
    /// Call this from any obstacle entity (asteroid, battleship, etc.) whenever it moves.
    /// The entity is responsible for calculating its own occupied tiles (supports single-tile asteroids or multi-tile huge battleships).
    /// Example usage in your Obstacle script:
    /// 
    /// private HashSet<Vector2Int> myCurrentTiles = new();
    /// 
    /// public void OnMoved() // call this after transform.position changes
    /// {
    ///     var newTiles = CalculateMyOccupiedTiles(); // your logic here (see example at bottom)
    ///     PathfindingManager.NotifyObstacleMoved(myCurrentTiles, newTiles);
    ///     myCurrentTiles = newTiles;
    /// }
    /// </summary>
    public static void NotifyObstacleMoved(HashSet<Vector2Int> oldOccupiedTiles, HashSet<Vector2Int> newOccupiedTiles)
    {
        if (Instance == null) return;

        Instance.RemoveBlockedTiles(oldOccupiedTiles ?? Enumerable.Empty<Vector2Int>());
        Instance.AddBlockedTiles(newOccupiedTiles ?? Enumerable.Empty<Vector2Int>());
    }

    private void AddBlockedTiles(IEnumerable<Vector2Int> tiles)
    {
        foreach (var tile in tiles)
        {
            blockedTiles.Add(tile);
        }
    }

    private void RemoveBlockedTiles(IEnumerable<Vector2Int> tiles)
    {
        foreach (var tile in tiles)
        {
            blockedTiles.Remove(tile);
        }
    }

    /// <summary>
    /// Main pathfinding call from any unit. Returns a list of waypoints (straight-line segments).
    /// Fully matches your spec: straight-line checks, sparse blocked-tile detection,
    /// minimum-effort side-check detour around the first blocked tile, wall-hugging via adjacent free tiles,
    /// constant LOS re-check while going around, and seamless handling of distant secondary obstacles.
    /// </summary>
    public static List<Vector2> FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        if (Instance == null)
        {
            Debug.LogWarning("PathfindingManager not found - returning direct path");
            return new List<Vector2> { startPosition, targetPosition };
        }

        return Instance.ComputePath(startPosition, targetPosition);
    }

    private List<Vector2> ComputePath(Vector2 start, Vector2 target)
    {
        List<Vector2> path = new List<Vector2> { start };
        Vector2 current = start;

        const int maxIterations = 200; // safety against rare closed-loop obstacles (space maps are open)
        int iterations = 0;

        while (iterations < maxIterations)
        {
            iterations++;

            // 1. Straight-line check to target (your core "check if there are any untraversable square tiles")
            if (IsLineClear(current, target))
            {
                path.Add(target);
                return path; // done - continue straight
            }

            // 2. Find the FIRST blocked tile along the line (minimum effort - only care about the immediate blocker)
            Vector2Int? blockedTile = GetFirstBlockedTile(current, target);
            if (blockedTile == null)
            {
                // Should never happen after the clear check, but safety
                path.Add(target);
                return path;
            }

            // 3. Get free tiles to the sides of the blocked tile (8-connected for smooth hugging on clumps)
            List<Vector2Int> freeAdjacent = GetAdjacentFreeTiles(blockedTile.Value);

            // 4. Among free sides, prefer the one that lets us go straight to target again (minimum effort detour)
            //    If none allow immediate LOS, pick the best hugging step and continue around the obstacle
            Vector2 bestWaypoint = Vector2.zero;
            float bestCost = float.MaxValue;
            bool hasImmediateClearLOS = false;

            foreach (Vector2Int adj in freeAdjacent)
            {
                Vector2 waypoint = TileToWorldCenter(adj);

                // Quick safety: can we even reach this side tile from current? (rarely fails in sparse space)
                if (!IsLineClear(current, waypoint)) continue;

                bool clearToTargetFromHere = IsLineClear(waypoint, target);
                float detourCost = (current - waypoint).sqrMagnitude + (waypoint - target).sqrMagnitude;

                if (clearToTargetFromHere)
                {
                    // Priority: any side that immediately restores LOS
                    if (!hasImmediateClearLOS || detourCost < bestCost)
                    {
                        bestCost = detourCost;
                        bestWaypoint = waypoint;
                        hasImmediateClearLOS = true;
                    }
                }
                else if (!hasImmediateClearLOS && detourCost < bestCost)
                {
                    // No immediate LOS yet → continue hugging (will check again next iteration)
                    bestCost = detourCost;
                    bestWaypoint = waypoint;
                }
            }

            if (bestWaypoint != Vector2.zero)
            {
                // Add the detour waypoint and continue the process from there
                // (this is the "hugging the wall" + "constantly check LOS or another distant obstacle" loop)
                path.Add(bestWaypoint);
                current = bestWaypoint;
                continue;
            }

            // Extremely rare in open space: completely surrounded (no free adjacent tiles)
            Debug.LogWarning($"Pathfinding stuck at blocked tile {blockedTile} - no free sides. Returning partial path.");
            break;
        }

        // Fallback: at least return what we have
        path.Add(target);
        return path;
    }
    #endregion

    #region Core Sparse Tile Helpers

    /// <summary>
    /// Converts world position to tile coordinates. Each tile is a square of size tileSize x tileSize.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector2Int WorldToTile(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / tileSize),
            Mathf.FloorToInt(worldPos.y / tileSize)
        );
    }

    /// <summary>
    /// Converts tile coordinates back to world position at the center of the tile.
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public Vector2 TileToWorldCenter(Vector2Int tile)
    {
        return new Vector2(
            (tile.x + 0.5f) * tileSize,
            (tile.y + 0.5f) * tileSize
        );
    }

    /// <summary>
    /// Returns ALL tiles the straight line crosses (in order from start to end).
    /// Uses Bresenham-style traversal - extremely fast for sparse empty space.
    /// </summary>
    public List<Vector2Int> GetCrossedTiles(Vector2 startWorld, Vector2 endWorld)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        Vector2 start = startWorld / tileSize;
        Vector2 end = endWorld / tileSize;

        int x0 = Mathf.FloorToInt(start.x);
        int y0 = Mathf.FloorToInt(start.y);
        int x1 = Mathf.FloorToInt(end.x);
        int y1 = Mathf.FloorToInt(end.y);

        tiles.Add(new Vector2Int(x0, y0));

        if (x0 == x1 && y0 == y1) return tiles;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }

            tiles.Add(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1) break;
        }

        return tiles;
    }

    private bool IsLineClear(Vector2 start, Vector2 end)
    {
        var tiles = GetCrossedTiles(start, end);
        foreach (var tile in tiles)
        {
            if (blockedTiles.Contains(tile))
                return false;
        }
        return true;
    }

    private Vector2Int? GetFirstBlockedTile(Vector2 start, Vector2 end)
    {
        var tiles = GetCrossedTiles(start, end);
        foreach (var tile in tiles)
        {
            if (blockedTiles.Contains(tile))
                return tile;
        }
        return null;
    }

    private List<Vector2Int> GetAdjacentFreeTiles(Vector2Int blocked)
    {
        List<Vector2Int> free = new List<Vector2Int>(8);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int adj = blocked + new Vector2Int(dx, dy);
                if (!blockedTiles.Contains(adj))
                    free.Add(adj);
            }
        }
        return free;
    }

    // Debug visualization of a list of tiles by drawing squares using Debug.DrawLine
    public void DebugDrawTiles(IEnumerable<Vector2Int> tiles, Color color, float duration = 0.1f)
    {
        foreach (var tile in tiles)
        {
            Vector2 center = TileToWorldCenter(tile);
            float half = tileSize / 2f;
            Vector3 topLeft = new Vector3(center.x - half, center.y + half, 0);
            Vector3 topRight = new Vector3(center.x + half, center.y + half, 0);
            Vector3 bottomLeft = new Vector3(center.x - half, center.y - half, 0);
            Vector3 bottomRight = new Vector3(center.x + half, center.y - half, 0);
            Debug.DrawLine(topLeft, topRight, color, duration);
            Debug.DrawLine(topRight, bottomRight, color, duration);
            Debug.DrawLine(bottomRight, bottomLeft, color, duration);
            Debug.DrawLine(bottomLeft, topLeft, color, duration);
        }
    }

    #endregion
}