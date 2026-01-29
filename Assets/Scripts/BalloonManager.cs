using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class BalloonManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _balloonPrefabs;
    [SerializeField] private int _maxBalloons = 3;
    [SerializeField] private float _spawnRate = 2f;
    [SerializeField] private float _spawnMinY = 3f;
    [SerializeField] private Vector2 _speedRange = new(1f, 3f);
    [SerializeField] private Vector2 _amplitudeRange = new(0.5f, 1.5f);
    [SerializeField] private Vector2 _frequencyRange = new(0.5f, 2f);
    
    private readonly List<GameObject> _activeBalloons = new();
    private float _nextSpawnTime;
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        CleanupBalloons();

        if (_activeBalloons.Count < _maxBalloons && Time.time >= _nextSpawnTime)
        {
            SpawnBalloon();
            _nextSpawnTime = Time.time + _spawnRate;
        }
    }

    private void SpawnBalloon()
    {
        if (_balloonPrefabs == null || _balloonPrefabs.Length == 0) return;

        var prefab = _balloonPrefabs[Random.Range(0, _balloonPrefabs.Length)];
        var balloonObj = Instantiate(prefab, transform);
        
        var balloon = balloonObj.GetComponent<Balloon>();
        if (balloon == null) balloon = balloonObj.AddComponent<Balloon>();

        var fromLeft = Random.value > 0.5f;
        var screenHeight = _cam.orthographicSize * 2f;
        var screenWidth = screenHeight * Screen.width / Screen.height;

        var startX = fromLeft ? -screenWidth / 2f - 2f : screenWidth / 2f + 2f;
        var startY = Random.Range(_spawnMinY, screenHeight / 2f);
        
        balloonObj.transform.localPosition = new Vector3(startX, startY, 10f);

        var speed = Random.Range(_speedRange.x, _speedRange.y);
        var amplitude = Random.Range(_amplitudeRange.x, _amplitudeRange.y);
        var frequency = Random.Range(_frequencyRange.x, _frequencyRange.y);
        
        balloon.Init(fromLeft ? Vector3.right : Vector3.left, speed, amplitude, frequency, screenWidth + 5f);
        
        _activeBalloons.Add(balloonObj);
    }

    private void CleanupBalloons()
    {
        for (var i = _activeBalloons.Count - 1; i >= 0; i--)
        {
            if (_activeBalloons[i] == null)
            {
                _activeBalloons.RemoveAt(i);
            }
        }
    }
}
