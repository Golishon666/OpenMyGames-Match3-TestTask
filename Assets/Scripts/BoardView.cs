using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

using System.Threading.Tasks;

public class BoardView : MonoBehaviour
{
    private float _scale;
    
    private IBoardService _boardService;
    private ElementViewFactory _elementFactory;
    private MainGameConfig _config;
    private CancellationToken _globalToken;
    private CancellationTokenSource _levelCts;
    private readonly List<ElementView> _allViews = new();
    private Vector2 _spawnOffset;

    [Inject]
    public void Construct(IBoardService boardService, ElementViewFactory elementFactory, MainGameConfig config, CancellationToken globalToken)
    {
        _boardService = boardService;
        _elementFactory = elementFactory;
        _config = config;
        _globalToken = globalToken;
        _scale = _config.defaultScale;
    }

    private void OnEnable()
    {
        _boardService.OnElementsMoved += HandleElementsMoved;
        _boardService.OnElementsDestroyed += HandleElementsDestroyed;
        _boardService.OnLevelLoaded += HandleLevelLoaded;
        _boardService.OnWin += HandleWin;
    }

    private void OnDisable()
    {
        _boardService.OnElementsMoved -= HandleElementsMoved;
        _boardService.OnElementsDestroyed -= HandleElementsDestroyed;
        _boardService.OnLevelLoaded -= HandleLevelLoaded;
        _boardService.OnWin -= HandleWin;
    }

    private async void Start()
    {
       await _boardService.LoadInitialState();
    }

    private async Task HandleLevelLoaded(LevelData data)
    {
        _levelCts?.Cancel();
        _levelCts?.Dispose();
        _levelCts = CancellationTokenSource.CreateLinkedTokenSource(_globalToken);
        var token = _levelCts.Token;

        try
        {
            token.ThrowIfCancellationRequested();
            ClearView();
            CalculateScaleAndOffset();
            BuildView();
            await Task.CompletedTask;
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private const int MovingSortingOrderBase = 10000;

    private void ClearView()
    {
        foreach (var view in _allViews.Where(view => view != null))
        {
            if (view.transform != null) view.transform.DOKill();
            Destroy(view.gameObject);
        }
        _allViews.Clear();
        
        foreach (Transform child in transform)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }
    }

    private async Task HandleElementsMoved(List<(Vector2Int from, Vector2Int to)> moves)
    {
        var token = _levelCts?.Token ?? _globalToken;
        token.ThrowIfCancellationRequested();
        var tasks = new List<Task>();
        var viewsToMove = new List<(ElementView view, Vector2Int to)>();

        foreach (var move in moves)
        {
            var view = _allViews.FirstOrDefault(v => v.GridPosition == move.from);
            if (view != null)
            {
                view.SetGridPosition(new Vector2Int(-1, -1));
                viewsToMove.Add((view, move.to));
            }
        }

        for (var i = 0; i < viewsToMove.Count; i++)
        {
            var (view, to) = viewsToMove[i];
            var tempOrder = MovingSortingOrderBase + (viewsToMove.Count - i);
            tasks.Add(MoveViewAsync(view, to, tempOrder, token));
        }

        await Task.WhenAll(tasks);
        token.ThrowIfCancellationRequested();
    }

    private async Task MoveViewAsync(ElementView view, Vector2Int to, int tempOrder, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var originalOrder = (to.y * _boardService.TotalWidth + to.x) + 1;
        view.SetSortingOrder(tempOrder);
        
        var targetPos = GetWorldPosition(to);
        try 
        {
            await view.transform.DOLocalMove(targetPos, _config.moveElementDuration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        }
        catch (System.Exception)
        {
        }

        token.ThrowIfCancellationRequested();
        view.SetGridPosition(to);
        view.SetSortingOrder(originalOrder);
    }

    private async Task HandleElementsDestroyed(List<Vector2Int> positions)
    {
        var token = _levelCts?.Token ?? _globalToken;
        token.ThrowIfCancellationRequested();
        var tasks = new List<Task>();
        var viewsToDestroy = positions
            .Select(pos => _allViews.FirstOrDefault(v => v.GridPosition == pos))
            .Where(view => view != null)
            .ToList();

        foreach (var view in viewsToDestroy)
        {
            _allViews.Remove(view);
            tasks.Add(DestroyViewAsync(view, token));
        }
        await Task.WhenAll(tasks);
        token.ThrowIfCancellationRequested();
    }

    private async Task DestroyViewAsync(ElementView view, CancellationToken token)
    {
        try
        {
            await view.PlayDestroyAnimationAsync(token);
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            if (view != null && view.gameObject != null)
            {
                Destroy(view.gameObject);
            }
        }
    }

    private async Task HandleWin()
    {
        _globalToken.ThrowIfCancellationRequested();
        Debug.Log("Level Win!");
        await Task.CompletedTask;
    }

    public ElementView GetElementAtScreenPoint(Vector2 screenPoint)
    {
        return _allViews.FirstOrDefault(view => view != null && view.ContainsScreenPoint(screenPoint));
    }

    private void CalculateScaleAndOffset()
    {
        var width = _boardService.TotalWidth;
        var height = _boardService.TotalHeight;

        var camHeight = Camera.main.orthographicSize * 2f;
        var camWidth = camHeight * Screen.width / Screen.height;

        var targetWidth = camWidth - _config.horizontalMargin * 2f;
        _scale = targetWidth / (width * _config.cellSize.x);
        
        var targetHeight = camHeight - _config.padding.y * 2f;
        if (height * _config.cellSize.y * _scale > targetHeight)
        {
            _scale = targetHeight / (height * _config.cellSize.y);
        }

        var fieldWidth = width * _config.cellSize.x * _scale;

        _spawnOffset = new Vector2(
            -fieldWidth / 2f + (_config.cellSize.x * _scale) / 2f,
            -camHeight / 2f + _config.padding.y
        );
        
        transform.localScale = new Vector3(_scale, _scale, 1f);
    }
    
    private void BuildView()
    {
        var width = _boardService.TotalWidth;
        var height = _boardService.TotalHeight;
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var type = _boardService.GetElement(x, y);
                if (type == ElementType.None) continue;

                var prefab = _elementFactory.GetPrefab(type);
                if (prefab == null) continue;

                var view = Instantiate(prefab, transform);
                var gridPos = new Vector2Int(x, y);
                var pos = GetWorldPosition(gridPos);

                var sortingOrder = y * width + x + 1;
                view.Init(type, gridPos, sortingOrder);
                view.transform.localPosition = pos;

                _allViews.Add(view);
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * _config.cellSize.x + _spawnOffset.x / _scale,
            gridPos.y * _config.cellSize.y + _spawnOffset.y / _scale,
            0f
        );
    }
}