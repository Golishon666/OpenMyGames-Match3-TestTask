using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IBoardService
{
    LevelData LevelData { get;}
    int TotalWidth { get; }
    int TotalHeight { get; }
    ElementType GetElement(int x, int y);
    bool IsInside(Vector2Int pos);
    bool IsNormalizing { get; }
    Task TryMoveAsync(Vector2Int from, Vector2Int dir);
    Task LoadLevelAsync(LevelData levelData);
    Task RestartLevelAsync();
    Task NextLevelAsync();
    Task LoadInitialState();
    bool IsBusy(Vector2Int pos);

    event Func<List<(Vector2Int from, Vector2Int to)>, Task> OnElementsMoved;
    event Func<List<Vector2Int>, Task> OnElementsDestroyed;
    event Func<LevelData, Task> OnLevelLoaded;
    event Func<Task> OnWin;
}