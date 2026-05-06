using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [SerializeField] private DialogueDatabase _dialogueDatabase;

    private DialogueSequence _currentSequence;
    private int _currentIndex;

    public event Action<DialogueLine> OnLineChanged;
    public event Action OnSequenceEnd;

    public bool IsPlaying => _currentSequence != null;

    public void Show(string sequenceId)
    {
        var sequence = _dialogueDatabase.GetSequence(sequenceId);
        if (sequence == null)
        {
            Debug.LogWarning($"[DialogueManager] Sequence not found: {sequenceId}");
            return;
        }
        if (sequence.lines == null || sequence.lines.Length == 0)
        {
            Debug.LogWarning($"[DialogueManager] Sequence has no lines: {sequenceId}");
            return;
        }

        _currentSequence = sequence;
        _currentIndex    = 0;
        PlayerManager.Instance?.SetInputEnabled(false);
        OnLineChanged?.Invoke(_currentSequence.lines[_currentIndex]);
    }

    public void Skip()
    {
        if (_currentSequence == null) return;
        _currentSequence = null;
        PlayerManager.Instance?.SetInputEnabled(true);
        OnSequenceEnd?.Invoke();
    }

    public void Next()
    {
        if (_currentSequence == null) return;

        _currentIndex++;
        if (_currentIndex >= _currentSequence.lines.Length)
        {
            _currentSequence = null;
            PlayerManager.Instance?.SetInputEnabled(true);
            OnSequenceEnd?.Invoke();
            return;
        }
        OnLineChanged?.Invoke(_currentSequence.lines[_currentIndex]);
    }
}
