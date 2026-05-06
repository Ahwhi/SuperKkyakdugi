using UnityEngine;

public class FallingObstacleSpawner : MonoBehaviour {
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private float _spawnHeight = 8f;

    float _timer;

    void Update() {
        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval) {
            _timer = 0f;

            Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + _spawnHeight);
            Instantiate(_obstaclePrefab, spawnPos, Quaternion.identity);
        }
    }
}