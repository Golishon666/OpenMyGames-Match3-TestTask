using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class SwipeController: IInitializable, IDisposable
    {
        private readonly SwipeInput _input;
        private readonly IBoardService _board;
        private readonly BoardView _boardView;
        private readonly CancellationToken _globalToken;
        private bool _isProcessing;
        
        public SwipeController(SwipeInput input, IBoardService board, BoardView boardView, CancellationToken globalToken)
        {
            _input = input;
            _board = board;
            _boardView = boardView;
            _globalToken = globalToken;
        }
        public void Initialize()
        {
            _input.OnSwipe += HandleSwipeAsync;
        }

        public void Dispose()
        {
            _input.OnSwipe -= HandleSwipeAsync;
        }

        private async Task HandleSwipeAsync(Vector2 start, Vector2 end)
        {
            if (_isProcessing || _board.IsNormalizing)
                return;

            _isProcessing = true;
            try
            {
                var element = _boardView.GetElementAtScreenPoint(start);
                if (element == null)
                    return;

                var delta = end - start;
                var dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? (delta.x > 0 ? Vector2Int.right : Vector2Int.left)
                    : (delta.y > 0 ? Vector2Int.up : Vector2Int.down);

                await TrySwapAsync(element, dir);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task TrySwapAsync(ElementView element, Vector2Int dir)
        {
            await _board.TryMoveAsync(element.GridPosition, dir);
            _globalToken.ThrowIfCancellationRequested();
        }
    }
