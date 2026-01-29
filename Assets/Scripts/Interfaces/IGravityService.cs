using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IGravityService
{
    Task<bool> ApplyGravityAsync(ElementType[,] elements, int width, int height, HashSet<Vector2Int> busyCells, 
        Func<List<(Vector2Int from, Vector2Int to)>, Task> onElementsMoved, CancellationToken token);
}
