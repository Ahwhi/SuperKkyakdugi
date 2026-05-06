using UnityEngine;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        QuestManager.Instance.OnQuestObjectCollected += HandleQuestObjectCollected;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestObjectCollected -= HandleQuestObjectCollected;
    }

    private readonly List<string> _collectedObjects = new();

    public int Score { get; private set; }

    public IReadOnlyList<string> CollectedObjects => _collectedObjects;

    public event System.Action<string> OnItemCollected;
    public event System.Action<int>    OnScoreChanged;

    private void HandleQuestObjectCollected(string objectName)
    {
        _collectedObjects.Add(objectName);
        Score++;
        OnItemCollected?.Invoke(objectName);
        OnScoreChanged?.Invoke(Score);
    }
}
