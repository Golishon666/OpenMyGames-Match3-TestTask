using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GravityService : IGravityService
{
    public async Task<bool> ApplyGravityAsync(ElementType[,] elements, int width, int height, HashSet<Vector2Int> busyCells,
        Func<List<(Vector2Int from, Vector2Int to)>, Task> onElementsMoved, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var changed = false;
        var gravityChanged = true;

        while (gravityChanged)
        {
            token.ThrowIfCancellationRequested();
            gravityChanged = false;
            var falling = new List<Vector2Int>();

            for (var y = 1; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var below = new Vector2Int(x, y - 1);
                    if (elements[x, y] != ElementType.None && elements[x, y - 1] == ElementType.None
                        && !busyCells.Contains(pos) && !busyCells.Contains(below))
                    {
                        falling.Add(pos);
                    }
                }
            }

            if (falling.Count > 0)
            {
                var gravityMoves = new List<(Vector2Int from, Vector2Int to)>();
                try
                {
                    foreach (var pos in falling)
                    {
                        var below = new Vector2Int(pos.x, pos.y - 1);
                        elements[below.x, below.y] = elements[pos.x, pos.y];
                        elements[pos.x, pos.y] = ElementType.None;

                        busyCells.Add(pos);
                        busyCells.Add(below);

                        gravityMoves.Add((pos, below));
                    }

                    if (onElementsMoved != null) await onElementsMoved.Invoke(gravityMoves);
                    token.ThrowIfCancellationRequested();
                }
                finally
                {
                    foreach (var move in gravityMoves)
                    {
                        busyCells.Remove(move.from);
                        busyCells.Remove(move.to);
                    }
                }

                gravityChanged = true;
                changed = true;
            }
            else
            {
                if (busyCells.Any(p =>
                        p.y > 0 && elements[p.x, p.y] != ElementType.None &&
                        elements[p.x, p.y - 1] == ElementType.None))
                {
                    await Task.Delay(50, token);
                    token.ThrowIfCancellationRequested();
                    gravityChanged = true;
                }
            }
        }

        return changed;
    }
}
