using UnityEngine;
using System.Collections.Generic;

public class Episode1Manager : MonoBehaviour {
    [Header("BGM Key")]
    [SerializeField] private string _bgmKey;

    [Header("Quest")]
    [SerializeField] private Sprite _image;

    [Header("Next")]
    [SerializeField] private Episode2Manager _episode2Manager;

    private readonly Dictionary<string, Player> _subscribedPlayers = new();

    private void Start() {
        UIManager.Instance.SetEpisodeName("에피소드1 - 힘든 하루");
        UIManager.Instance.SetEpisodeDiscription("효과가 확실한 행운의 네잎클로버 10개를 모으세요!");
        UIManager.Instance.SetEpisodeImage(_image);

        QuestManager.Instance.OnCollectedCountChanged += HandleCount;
        GameManager.Instance.OnPlayerFell += HandlePlayerFell;

        PlayerManager.Instance.OnPlayerRegistered += SubscribeToPlayer;
        PlayerManager.Instance.OnPlayerUnregistered += UnsubscribeFromPlayer;

        foreach (var kv in PlayerManager.Instance.Players)
            SubscribeToPlayer(kv.Key, kv.Value);

        DialogueManager.Instance.Show("intro");

        var bgmName = SoundManager.Instance.soundDatabase.GetName(_bgmKey);
        if (bgmName != null)
            UIManager.Instance.SetBGMName(bgmName);
        SoundManager.Instance.PlayBGM(_bgmKey);
    }

    private void OnDisable() {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnCollectedCountChanged -= HandleCount;

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerFell -= HandlePlayerFell;

        if (PlayerManager.Instance != null) {
            PlayerManager.Instance.OnPlayerRegistered -= SubscribeToPlayer;
            PlayerManager.Instance.OnPlayerUnregistered -= UnsubscribeFromPlayer;
        }

        foreach (var kv in _subscribedPlayers)
            kv.Value.OnObstacleHit -= HandleObstacleHit;

        _subscribedPlayers.Clear();
    }

    private void SubscribeToPlayer(string id, Player player) {
        player.OnObstacleHit += HandleObstacleHit;
        _subscribedPlayers[id] = player;
    }

    private void UnsubscribeFromPlayer(string id) {
        if (!_subscribedPlayers.TryGetValue(id, out var player)) return;
        player.OnObstacleHit -= HandleObstacleHit;
        _subscribedPlayers.Remove(id);
    }

    private void HandleCount(int count) {
        SoundManager.Instance.PlaySFX("Get");
        if (count == 10)
            GameOver(true);
    }

    private void HandleObstacleHit(GameObject obj) {
        GameOver(false);
    }

    private void HandlePlayerFell() {
        GameOver(false);
    }

    public void GameOver(bool clear) {
        SoundManager.Instance.StopBGM();
        if (clear) {
            gameObject.SetActive(false);
            if (_episode2Manager != null)
                _episode2Manager.gameObject.SetActive(true);
        } else {
            Time.timeScale = 0f;
            UIManager.Instance.ShowResultPanel(false);
        }
    }
}
