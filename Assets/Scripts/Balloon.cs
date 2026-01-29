using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    private float _amplitude;
    private float _frequency;
    private float _maxDistance;
    private Vector3 _startPos;
    private float _startTime;

    public void Init(Vector3 direction, float speed, float amplitude, float frequency, float maxDistance)
    {
        _direction = direction;
        _speed = speed;
        _amplitude = amplitude;
        _frequency = frequency;
        _maxDistance = maxDistance;
        _startPos = transform.localPosition;
        _startTime = Time.time;
    }

    private void Update()
    {
        var elapsed = Time.time - _startTime;
        var move = _direction * (_speed * elapsed);
        var wave = Mathf.Sin(elapsed * _frequency) * _amplitude;
        
        transform.localPosition = _startPos + move + Vector3.up * wave;

        if (Vector3.Distance(_startPos, transform.localPosition) > _maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
