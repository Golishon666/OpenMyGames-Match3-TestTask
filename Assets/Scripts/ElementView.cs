    using UnityEngine;
    using System.Threading;
    using System.Threading.Tasks;
    using DG.Tweening;

    public class ElementView : MonoBehaviour
    {
        private static readonly int DestroyAnim = Animator.StringToHash("Destroy");

        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private Animator _animator;
        
        public Vector2Int GridPosition { get; private set; }
        public ElementType Type { get; private set; }
        private Collider2D _collider;
        
        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }
        
        public async Task PlayDestroyAnimationAsync(CancellationToken token)
        {
            if (_animator != null)
            {
                await PlayAnimationAsync(DestroyAnim, token);
            }
            
            token.ThrowIfCancellationRequested();
        }

        public bool ContainsScreenPoint(Vector2 screenPoint)
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
            worldPoint.z = 0;
            return _collider.OverlapPoint(worldPoint);
        }

        public void Init(ElementType type, Vector2Int gridPos, int sortingOrder)
        {
            Type = type;
            GridPosition = gridPos;
            _spriteRenderer.sortingOrder = sortingOrder;
        }

        public void SetGridPosition(Vector2Int newPos)
        {
            GridPosition = newPos;
        }

        public void SetSortingOrder(int order)
        {
            _spriteRenderer.sortingOrder = order;
        }

        private async Task PlayAnimationAsync(int stateName, CancellationToken token)
        {
            _animator.Play(stateName, 0, 0f);
            
            await Task.Yield();
            token.ThrowIfCancellationRequested();

            var clip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            var duration = clip.length;
            
            await Task.Delay((int)(duration * 1000), token);
            token.ThrowIfCancellationRequested();
        }

    }