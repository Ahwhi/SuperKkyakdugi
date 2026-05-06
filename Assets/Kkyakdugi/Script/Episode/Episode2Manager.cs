using UnityEngine;
using System.Collections.Generic;

public class Episode2Manager : MonoBehaviour {
    [Header("BGM Key")]
    [SerializeField] private string _bgmKey;

    [Header("Background")]
    [SerializeField] private SpriteRenderer _backgroundRenderer1;
    [SerializeField] private SpriteRenderer _backgroundRenderer2;
    [SerializeField] private Sprite         _backgroundSprite;

    [Header("Quest")]
    [SerializeField] private Sprite _image;

    [Header("Friends & Home")]
    [SerializeField] private GameObject[] _friendPrefabs;
    [SerializeField] private GameObject   _homePrefab;
    [SerializeField] private float _spawnStartOffset = 15f;
    [SerializeField] private float _minSpawnSpacing  = 8f;
    [SerializeField] private float _maxSpawnSpacing  = 15f;
    [SerializeField] private float _homeSpawnOffset  = 10f;

    [Header("Settings")]
    [SerializeField] private float _timeLimit = 60f;

    private float _remainingTime;
    private int   _rescuedCount;
    private bool  _allFriendsRescued;
    private bool  _isRunning;

    private readonly Dictionary<string, Player> _subscribedPlayers = new();
    private readonly List<GameObject> _spawnedObjects = new();

    private int FriendCount => _friendPrefabs != null ? _friendPrefabs.Length : 0;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────
    private void OnEnable() {
        if (_backgroundRenderer1 != null && _backgroundRenderer2 != null && _backgroundSprite != null) {
            _backgroundRenderer1.sprite = _backgroundSprite;
            _backgroundRenderer2.sprite = _backgroundSprite;
        }

        _rescuedCount      = 0;
        _allFriendsRescued = false;
        _isRunning         = false;
        _remainingTime     = _timeLimit;

        UIManager.Instance.SetEpisodeName("에피소드2 - 집으로");
        UIManager.Instance.SetEpisodeDiscription("제한 시간 내에 꺅두기들과 꺅로워를 데리고 집에가세요!");
        UIManager.Instance.SetEpisodeImage(_image);

        UIManager.Instance.SetRescueCount(0, FriendCount);
        UIManager.Instance.ShowRescueCount(true);
        UIManager.Instance.ShowTimer(false);

        GameManager.Instance.OnPlayerFell += HandlePlayerFell;

        PlayerManager.Instance.OnPlayerRegistered   += SubscribeToPlayer;
        PlayerManager.Instance.OnPlayerUnregistered += UnsubscribeFromPlayer;
        foreach (var kv in PlayerManager.Instance.Players)
            SubscribeToPlayer(kv.Key, kv.Value);

        DialogueManager.Instance.OnSequenceEnd += OnIntroDialogueEnd;
        DialogueManager.Instance.Show("episode2_intro");

        SpawnEpisodeObjects();

        var bgmName = SoundManager.Instance.soundDatabase.GetName(_bgmKey);
        if (bgmName != null)
            UIManager.Instance.SetBGMName(bgmName);
        SoundManager.Instance.PlayBGM(_bgmKey);
    }

    private void OnDisable() {
        if (DialogueManager.Instance != null) {
            DialogueManager.Instance.OnSequenceEnd -= OnIntroDialogueEnd;
            DialogueManager.Instance.OnSequenceEnd -= OnClearDialogueEnd;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerFell -= HandlePlayerFell;

        if (PlayerManager.Instance != null) {
            PlayerManager.Instance.OnPlayerRegistered   -= SubscribeToPlayer;
            PlayerManager.Instance.OnPlayerUnregistered -= UnsubscribeFromPlayer;
        }

        foreach (var kv in _subscribedPlayers) {
            kv.Value.OnTaggedTrigger -= HandleTaggedTrigger;
            kv.Value.OnObstacleHit   -= HandleObstacleHit;
        }
        _subscribedPlayers.Clear();

        foreach (var obj in _spawnedObjects)
            if (obj != null) Destroy(obj);
        _spawnedObjects.Clear();

        if (UIManager.Instance != null) {
            UIManager.Instance.ShowTimer(false);
            UIManager.Instance.ShowRescueCount(false);
        }
    }

    // ──────────────────────────────────────────────
    // Spawning
    // ──────────────────────────────────────────────
    private void SpawnEpisodeObjects() {
        if (_friendPrefabs == null || _friendPrefabs.Length == 0) {
            Debug.LogWarning("[Episode2Manager] No friend prefabs assigned.");
            return;
        }
        if (_homePrefab == null) {
            Debug.LogWarning("[Episode2Manager] No home prefab assigned.");
            return;
        }

        var player = PlayerManager.Instance?.ActivePlayer;
        float startX = player != null ? player.transform.position.x : 0f;
        float groundY = SpawnManager.Instance != null ? SpawnManager.Instance.ObjectSpawnY : 0f;

        var shuffled = new List<GameObject>(_friendPrefabs);
        for (int i = shuffled.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        float currentX = startX + _spawnStartOffset;
        foreach (var prefab in shuffled) {
            currentX += Random.Range(_minSpawnSpacing, _maxSpawnSpacing);
            _spawnedObjects.Add(Instantiate(prefab, new Vector2(currentX, groundY), Quaternion.identity));
        }

        currentX += _homeSpawnOffset;
        _spawnedObjects.Add(Instantiate(_homePrefab, new Vector2(currentX, groundY + 1.2f), Quaternion.identity));
    }

    // ──────────────────────────────────────────────
    // Dialogue
    // ──────────────────────────────────────────────
    private void OnIntroDialogueEnd() {
        DialogueManager.Instance.OnSequenceEnd -= OnIntroDialogueEnd;
        _isRunning = true;
        UIManager.Instance.SetTimer(_remainingTime);
        UIManager.Instance.ShowTimer(true);
    }

    // ──────────────────────────────────────────────
    // Timer
    // ──────────────────────────────────────────────
    private void Update() {
        if (!_isRunning) return;

        _remainingTime -= Time.deltaTime;
        UIManager.Instance.SetTimer(_remainingTime);

        if (_remainingTime <= 0f)
            GameOver(false);
    }

    // ──────────────────────────────────────────────
    // Player Subscription
    // ──────────────────────────────────────────────
    private void SubscribeToPlayer(string id, Player player) {
        player.OnTaggedTrigger += HandleTaggedTrigger;
        player.OnObstacleHit   += HandleObstacleHit;
        _subscribedPlayers[id]  = player;
    }

    private void UnsubscribeFromPlayer(string id) {
        if (!_subscribedPlayers.TryGetValue(id, out var player)) return;
        player.OnTaggedTrigger -= HandleTaggedTrigger;
        player.OnObstacleHit   -= HandleObstacleHit;
        _subscribedPlayers.Remove(id);
    }

    // ──────────────────────────────────────────────
    // Game Logic
    // ──────────────────────────────────────────────
    private void HandleTaggedTrigger(string tag, GameObject obj) {
        switch (tag) {
            case "Friend": HandleFriendCollected(obj); break;
            case "Home":   HandleHomeReached();         break;
        }
    }

    private void HandleFriendCollected(GameObject obj) {
        if (!_isRunning) return;

        obj.SetActive(false);
        _rescuedCount++;
        UIManager.Instance.SetRescueCount(_rescuedCount, FriendCount);
        SoundManager.Instance.PlaySFX("Get");

        if (_rescuedCount >= FriendCount)
            _allFriendsRescued = true;
    }

    private void HandleObstacleHit(GameObject obj) {
        if (!_isRunning) return;
        GameOver(false);
    }

    private void HandlePlayerFell() {
        if (!_isRunning) return;
        GameOver(false);
    }

    private void HandleHomeReached() {
        if (!_isRunning || !_allFriendsRescued) return;
        GameOver(true);
    }

    public void GameOver(bool clear) {
        _isRunning = false;
        SoundManager.Instance.StopBGM();
        UIManager.Instance.ShowTimer(false);

        if (clear) {
            DialogueManager.Instance.OnSequenceEnd += OnClearDialogueEnd;
            DialogueManager.Instance.Show("episode2_clear");
        } else {
            Time.timeScale = 0f;
            UIManager.Instance.ShowResultPanel(false, "잘 몰르겠는 두기");
        }
    }

    private void OnClearDialogueEnd() {
        DialogueManager.Instance.OnSequenceEnd -= OnClearDialogueEnd;
        Time.timeScale = 0f;
        UIManager.Instance.ShowResultPanel(true, "슈퍼 꺅두기");
    }
}
