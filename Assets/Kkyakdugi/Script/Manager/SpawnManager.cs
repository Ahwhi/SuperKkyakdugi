using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Ground")]
    [SerializeField] private GameObject[] _groundTilePrefabs;
    [SerializeField] private float _tileWidth = 1f;

    [Header("Segment")]
    [SerializeField] private int _minTilesPerSegment = 3;
    [SerializeField] private int _maxTilesPerSegment = 8;
    [SerializeField] private float _minGapWidth = 1.5f;
    [SerializeField] private float _maxGapWidth = 4f;

    [Header("Objects")]
    [SerializeField] private GameObject[] _spawnablePrefabs;
    [SerializeField] private int _minObjectsPerSegment = 0;
    [SerializeField] private int _maxObjectsPerSegment = 3;
    [SerializeField] private float _objectHeightOffset = 1f;
    [SerializeField] private int _maxTotalObjects = -1;

    [Header("Falling Obstacles")]
    [SerializeField] private GameObject _fallingSpawnerPrefab;
    [SerializeField] private int _minObstaclesPerSegment = 0;
    [SerializeField] private int _maxObstaclesPerSegment = 2;

    [Header("Culling")]
    [SerializeField] private float _spawnAheadDistance = 20f;
    [SerializeField] private float _despawnBehindDistance = 10f;

    [Header("Safe Zone")]
    [SerializeField] private float _safeZoneWidth = 10f;

    public float ObjectSpawnY => transform.position.y + _objectHeightOffset;

    private float _nextSpawnX;
    private float _safeZoneEndX;
    private int _totalObjectsSpawned;
    private Camera _mainCamera;
    private readonly List<GameObject> _activeObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _nextSpawnX = transform.position.x;
        _safeZoneEndX = transform.position.x + _safeZoneWidth;
        FillAhead();
    }

    private void Update()
    {
        FillAhead();
        CullBehind();
    }

    public GameObject Spawn(GameObject prefab, Vector2 position)
    {
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        _activeObjects.Add(obj);
        return obj;
    }

    private void FillAhead()
    {
        float cameraRightEdge = _mainCamera.transform.position.x + _spawnAheadDistance;
        while (_nextSpawnX < cameraRightEdge)
            _nextSpawnX = SpawnSegment(_nextSpawnX);
    }

    private float SpawnSegment(float startX)
    {
        int tileCount = Random.Range(_minTilesPerSegment, _maxTilesPerSegment + 1);

        for (int i = 0; i < tileCount; i++)
        {
            float tileX = startX + (i + 0.5f) * _tileWidth;
            GameObject prefab = _groundTilePrefabs[Random.Range(0, _groundTilePrefabs.Length)];
            Spawn(prefab, new Vector2(tileX, transform.position.y));
        }

        float segmentEndX = startX + tileCount * _tileWidth;
        if (startX >= _safeZoneEndX) {
            SpawnObjectsOnSegment(startX, segmentEndX);
            SpawnFallingObstacles(startX, segmentEndX);
        }

        return segmentEndX + Random.Range(_minGapWidth, _maxGapWidth);
    }

    private void SpawnObjectsOnSegment(float segmentStartX, float segmentEndX)
    {
        if (_spawnablePrefabs == null || _spawnablePrefabs.Length == 0) return;
        if (_maxTotalObjects >= 0 && _totalObjectsSpawned >= _maxTotalObjects) return;

        float minX = segmentStartX + _tileWidth;
        float maxX = segmentEndX - _tileWidth;
        if (minX >= maxX) return;

        int count = Random.Range(_minObjectsPerSegment, _maxObjectsPerSegment + 1);
        if (_maxTotalObjects >= 0)
            count = Mathf.Min(count, _maxTotalObjects - _totalObjectsSpawned);

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(minX, maxX);
            GameObject prefab = _spawnablePrefabs[Random.Range(0, _spawnablePrefabs.Length)];
            Spawn(prefab, new Vector2(x, ObjectSpawnY));
            _totalObjectsSpawned++;
        }
    }

    private void SpawnFallingObstacles(float segmentStartX, float segmentEndX) {
        if (_fallingSpawnerPrefab == null) return;

        int count = Random.Range(_minObstaclesPerSegment, _maxObstaclesPerSegment + 1);
        for (int i = 0; i < count; i++) {
            float x = Random.Range(segmentStartX, segmentEndX);
            Spawn(_fallingSpawnerPrefab, new Vector2(x, transform.position.y));
        }
    }

    private void CullBehind()
    {
        float cameraLeftEdge = _mainCamera.transform.position.x - _despawnBehindDistance;
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            if (_activeObjects[i] == null)
            {
                _activeObjects.RemoveAt(i);
                continue;
            }
            if (_activeObjects[i].transform.position.x < cameraLeftEdge)
            {
                Destroy(_activeObjects[i]);
                _activeObjects.RemoveAt(i);
            }
        }
    }
}
