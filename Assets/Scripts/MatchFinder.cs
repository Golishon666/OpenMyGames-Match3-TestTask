using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchFinder : IMatchFinder
{
    public List<Vector2Int> FindMatches(ElementType[,] elements, int width, int height)
    {
        var totalToDestroy = new HashSet<Vector2Int>();
        var visited = new bool[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (visited[x, y] || elements[x, y] == ElementType.None) continue;

                var region = GetRegion(elements, width, height, x, y);
                foreach (var p in region) visited[p.x, p.y] = true;

                if (HasLine(region))
                {
                    foreach (var p in region) totalToDestroy.Add(p);
                }
            }
        }

        return totalToDestroy.ToList();
    }

    private bool HasLine(List<Vector2Int> region)
    {
        if (region.Count < 3) return false;

        var regionSet = new HashSet<Vector2Int>(region);

        foreach (var p in region)
        {
            var horizontalCount = 1;
            var curr = p + Vector2Int.right;
            while (regionSet.Contains(curr))
            {
                horizontalCount++;
                curr += Vector2Int.right;
            }
            curr = p + Vector2Int.left;
            while (regionSet.Contains(curr))
            {
                horizontalCount++;
                curr += Vector2Int.left;
            }

            if (horizontalCount >= 3) return true;

            var verticalCount = 1;
            var currV = p + Vector2Int.up;
            while (regionSet.Contains(currV))
            {
                verticalCount++;
                currV += Vector2Int.up;
            }
            currV = p + Vector2Int.down;
            while (regionSet.Contains(currV))
            {
                verticalCount++;
                currV += Vector2Int.down;
            }

            if (verticalCount >= 3) return true;
        }

        return false;
    }

    private List<Vector2Int> GetRegion(ElementType[,] elements, int width, int height, int x, int y)
    {
        var type = elements[x, y];
        var region = new List<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        var start = new Vector2Int(x, y);
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            region.Add(p);

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                var next = p + dir;
                if (IsInside(next, width, height) && !visited.Contains(next) && elements[next.x, next.y] == type)
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return region;
    }

    private bool IsInside(Vector2Int pos, int width, int height)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}
