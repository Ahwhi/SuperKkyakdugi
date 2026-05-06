using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private float _deathY = -5f;

    public event System.Action OnPlayerFell;

    private bool _fellTriggered;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Update() {
        if (_fellTriggered) return;
        var player = PlayerManager.Instance?.ActivePlayer;
        if (player != null && player.transform.position.y < _deathY) {
            _fellTriggered = true;
            OnPlayerFell?.Invoke();
        }
    }
}
