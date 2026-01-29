using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Camera _cam;
    private float _lastAspectRatio;
    private float _lastOrthoSize;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cam = Camera.main;
    }

    private void Start()
    {
        Scale();
    }

    private void Update()
    {
        if (_cam == null) return;

        if (!Mathf.Approximately(_cam.aspect, _lastAspectRatio) || !Mathf.Approximately(_cam.orthographicSize, _lastOrthoSize))
        {
            Scale();
        }
    }

    private void Scale()
    {
        if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

        _lastAspectRatio = _cam.aspect;
        _lastOrthoSize = _cam.orthographicSize;

        var worldScreenHeight = _cam.orthographicSize * 2f;
        var worldScreenWidth = worldScreenHeight * _cam.aspect;

        var spriteSize = _spriteRenderer.sprite.bounds.size;

        var scaleX = worldScreenWidth / spriteSize.x;
        var scaleY = worldScreenHeight / spriteSize.y;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);
        
        transform.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y, transform.position.z);
    }
}
