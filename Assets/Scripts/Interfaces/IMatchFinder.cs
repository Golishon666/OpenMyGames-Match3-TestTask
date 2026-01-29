using System.Collections.Generic;
using UnityEngine;

public interface IMatchFinder
{
    List<Vector2Int> FindMatches(ElementType[,] elements, int width, int height);
}
