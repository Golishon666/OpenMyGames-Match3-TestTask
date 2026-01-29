using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BoardService : IBoardService
{
    private readonly CancellationToken _globalToken;
    private readonly ISaveService _saveService;
    private readonly IMatchFinder _matchFinder;
    private readonly IGravityService _gravityService;
    private readonly ILevelService _levelService;

    private const int BufferSize = 1;

    public LevelData LevelData { get; private set; }
    public int TotalWidth => _totalWidth;
    public int TotalHeight => _totalHeight;
    private ElementType[,] _elements;
    private int _totalWidth;
    private int _totalHeight;

    private HashSet<Vector2Int> _busyCells = new();
    public bool IsNormalizing => _isNormalizing;
    private bool _isNormalizing;
    private bool _needsNormalization;
    private CancellationTokenSource _normalizationCts;
    private CancellationTokenSource _levelEndCts;

    public event Func<List<(Vector2Int from, Vector2Int to)>, Task> OnElementsMoved;
    public event Func<List<Vector2Int>, Task> OnElementsDestroyed;
    public event Func<LevelData, Task> OnLevelLoaded;
    public event Func<Task> OnWin;

    public BoardService(
        CancellationToken globalToken,
        ISaveService saveService,
        IMatchFinder matchFinder,
        IGravityService gravityService,
        ILevelService levelService)
    {
        _globalToken = globalToken;
        _saveService = saveService;
        _matchFinder = matchFinder;
        _gravityService = gravityService;
        _levelService = levelService;
    }

    private void SaveCurrentBoard() => _saveService.SaveBoard(_totalWidth, _totalHeight, _elements);

    public async Task LoadInitialState()
    {
        _globalToken.ThrowIfCancellationRequested();
        LevelData = _levelService.LoadInitialLevel();
        if (_saveService.HasSavedBoard())
        {
            var savedData = _saveService.LoadBoard();
            _totalWidth = savedData.width;
            _totalHeight = savedData.height;
            _elements = new ElementType[_totalWidth, _totalHeight];
            
            for (var x = 0; x < _totalWidth; x++)
            for (var y = 0; y < _totalHeight; y++)
                _elements[x, y] = savedData.GetElement(x, y);

            ResetState(false);
            if (OnLevelLoaded != null) await OnLevelLoaded.Invoke(LevelData);
            _globalToken.ThrowIfCancellationRequested();
            _ = StartNormalizationAsync();
        }
        else
        {
            await LoadLevelAsync(LevelData);
        }
    }

    public async Task LoadLevelAsync(LevelData levelData)
    {
        StopNormalization();
        CancelLevelEnd();

        LevelData = levelData;
        _totalWidth = levelData.width + BufferSize * 2;
        _totalHeight = levelData.height;
        _elements = new ElementType[_totalWidth, _totalHeight];

        for (var x = 0; x < _totalWidth; x++)
        for (var y = 0; y < _totalHeight; y++)
            _elements[x, y] = ElementType.None;

        for (var x = 0; x < levelData.width; x++)
        for (var y = 0; y < levelData.height; y++)
            _elements[x + BufferSize, y] = levelData.GetElement(x, y);

        ResetState(true);
        SaveCurrentBoard();
        
        if (OnLevelLoaded != null) await OnLevelLoaded.Invoke(levelData);
        _globalToken.ThrowIfCancellationRequested();
    }

    private void ResetState(bool clearNormalizing)
    {
        _busyCells.Clear();
        if (clearNormalizing) _isNormalizing = false;
    }

    private void StopNormalization()
    {
        _normalizationCts?.Cancel();
        _normalizationCts?.Dispose();
        _normalizationCts = null;
    }

    private void CancelLevelEnd()
    {
        _levelEndCts?.Cancel();
        _levelEndCts?.Dispose();
        _levelEndCts = null;
    }

    public async Task RestartLevelAsync()
    {
        _globalToken.ThrowIfCancellationRequested();
        _levelService.RestartLevel();
        await LoadLevelAsync(_levelService.GetCurrentLevel());
    }

    public async Task NextLevelAsync()
    {
        _globalToken.ThrowIfCancellationRequested();
        await LoadLevelAsync(_levelService.GetNextLevel());
    }

    public ElementType GetElement(int x, int y) => _elements[x, y];
    public bool IsBusy(Vector2Int pos) => _busyCells.Contains(pos);

    public bool IsInside(Vector2Int pos)
    {
        return _elements != null && pos.x >= 0 && pos.x < _totalWidth && pos.y >= 0 && pos.y < _totalHeight;
    }


    public async Task TryMoveAsync(Vector2Int from, Vector2Int dir)
    {
        if (_isNormalizing) return;
        
        var to = from + dir;
        if (!IsInside(to)) return;
        if (IsBusy(from) || IsBusy(to)) return;

        var typeFrom = _elements[from.x, from.y];
        if (typeFrom == ElementType.None) return;

        var typeTo = _elements[to.x, to.y];

        _busyCells.Add(from);
        _busyCells.Add(to);

        try
        {
            var moves = new List<(Vector2Int from, Vector2Int to)> { (from, to) };
            if (typeTo != ElementType.None)
            {
                moves.Add((to, from));
            }

            if (OnElementsMoved != null) await OnElementsMoved.Invoke(moves);
            
            _elements[from.x, from.y] = typeTo;
            _elements[to.x, to.y] = typeFrom;
            SaveCurrentBoard();

            _busyCells.Remove(from);
            _busyCells.Remove(to);
            await StartNormalizationAsync();
        }
        finally
        {
            _busyCells.Remove(from);
            _busyCells.Remove(to);
        }
    }

    private async Task StartNormalizationAsync()
    {
        _globalToken.ThrowIfCancellationRequested();
        _needsNormalization = true;
        if (_isNormalizing) return;

        _isNormalizing = true;
        var capturedElements = _elements;
        try
        {
            while (_needsNormalization)
            {
                _globalToken.ThrowIfCancellationRequested();
                if (capturedElements != _elements) break;
                _needsNormalization = false;

                _normalizationCts?.Cancel();
                _normalizationCts?.Dispose();
                _normalizationCts = CancellationTokenSource.CreateLinkedTokenSource(_globalToken);
                var token = _normalizationCts.Token;

                while (_busyCells.Count > 0)
                {
                    await Task.Delay(50, token);
                }

                await _gravityService.ApplyGravityAsync(capturedElements, _totalWidth, _totalHeight, _busyCells, OnElementsMoved, token);

                var toDestroy = _matchFinder.FindMatches(capturedElements, _totalWidth, _totalHeight);

                if (toDestroy.Count > 0)
                {
                    var positions = toDestroy.ToList();
                    foreach (var pos in positions) _busyCells.Add(pos);

                    try
                    {
                        if (OnElementsDestroyed != null)
                            await OnElementsDestroyed.Invoke(positions);
                        token.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        foreach (var pos in positions)
                        {
                            capturedElements[pos.x, pos.y] = ElementType.None;
                            _busyCells.Remove(pos);
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    SaveCurrentBoard();
                    _needsNormalization = true;
                }
            }

            if (capturedElements == _elements)
            {
                await CheckWinAsync();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _isNormalizing = false;
        }
    }

    private async Task CheckWinAsync()
    {
        _globalToken.ThrowIfCancellationRequested();
        for (var x = 0; x < _totalWidth; x++)
        for (var y = 0; y < _totalHeight; y++)
            if (_elements[x, y] != ElementType.None) return;

        if (OnWin != null)
            await OnWin.Invoke();
        _globalToken.ThrowIfCancellationRequested();

        CancelLevelEnd();
        _levelEndCts = CancellationTokenSource.CreateLinkedTokenSource(_globalToken);
        var token = _levelEndCts.Token;

        try
        {
            await Task.Delay(2000, token);
            token.ThrowIfCancellationRequested();

            await NextLevelAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }
}