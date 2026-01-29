using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class SwipeInput : MonoBehaviour
{
    public event Func<Vector2, Vector2, Task> OnSwipe;

    private Vector2 _startPos;
    private bool _isSwiping;

    [SerializeField] private float _minSwipeDistance = 2f;

    private void Update()
    {
        try
        {
#if UNITY_EDITOR
            HandleMouse();
#endif
#if UNITY_ANDROID || UNITY_IOS
            HandleTouchscreen();
#endif
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void HandleTouchscreen()
    {
        if (Touchscreen.current == null) return;
        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.isPressed)
        {
            if (_isSwiping)
                return;

            _isSwiping = true;
            _startPos = touch.position.ReadValue();
        }
        else
        {
            if (!_isSwiping)
                return;

            _isSwiping = false;
            var endPos = touch.position.ReadValue();

            if (Vector2.Distance(_startPos, endPos) >= _minSwipeDistance)
            {
                if (OnSwipe != null)
                    _ = OnSwipe.Invoke(_startPos, endPos);
            }
        }
    }

    private void HandleMouse()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _isSwiping = true;
            _startPos = Mouse.current.position.ReadValue();
        }

        if (!Mouse.current.leftButton.wasReleasedThisFrame || !_isSwiping)
            return;

        _isSwiping = false;
        var endPos = Mouse.current.position.ReadValue();

        if (Vector2.Distance(_startPos, endPos) >= _minSwipeDistance)
        {
            if (OnSwipe != null)
                _ = OnSwipe.Invoke(_startPos, endPos);
        }
    }
}