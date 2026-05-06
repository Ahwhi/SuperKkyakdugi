using UnityEngine;
using System;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Game/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    public DialogueSequence[] sequences;

    public DialogueSequence GetSequence(string sequenceId)
    {
        return Array.Find(sequences, s => s.sequenceId == sequenceId);
    }
}
