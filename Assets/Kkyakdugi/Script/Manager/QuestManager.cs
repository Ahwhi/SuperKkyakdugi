using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour {
    public static QuestManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;

        if (PlayerManager.Instance == null) return;
        PlayerManager.Instance.OnPlayerRegistered -= SubscribeToPlayer;
        PlayerManager.Instance.OnPlayerUnregistered -= UnsubscribeFromPlayer;
    }

    public event System.Action<string> OnQuestObjectCollected;
    public event System.Action<int> OnCollectedCountChanged;

    private readonly Dictionary<string, Player> _subscribedPlayers = new();
    private readonly HashSet<GameObject> _collectedObjects = new();

    private void Start() {
        PlayerManager.Instance.OnPlayerRegistered += SubscribeToPlayer;
        PlayerManager.Instance.OnPlayerUnregistered += UnsubscribeFromPlayer;

        foreach (var kv in PlayerManager.Instance.Players)
            SubscribeToPlayer(kv.Key, kv.Value);
    }

    private void SubscribeToPlayer(string id, Player player) {
        player.OnQuestObjectHit += HandleQuestObjectHit;
        _subscribedPlayers[id] = player;
    }

    private void UnsubscribeFromPlayer(string id) {
        if (!_subscribedPlayers.TryGetValue(id, out var player)) return;
        player.OnQuestObjectHit -= HandleQuestObjectHit;
        _subscribedPlayers.Remove(id);
    }

    private void HandleQuestObjectHit(GameObject obj) {
        if (!_collectedObjects.Add(obj)) return;
        obj.SetActive(false);
        OnQuestObjectCollected?.Invoke(obj.name);
        OnCollectedCountChanged?.Invoke(_collectedObjects.Count);
    }
}
