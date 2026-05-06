using System.Collections;
using UnityEngine;
using TMPro;

public class TypeWriter : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float delay = 0.05f;

    public bool IsTyping { get; private set; }
    private Coroutine _typingCoroutine;

    public void Play(string fullText)
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    public void Complete()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        textUI.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
    }

    private IEnumerator TypeText(string fullText)
    {
        IsTyping = true;
        textUI.text = fullText;
        textUI.maxVisibleCharacters = 0;
        textUI.ForceMeshUpdate();

        int total = textUI.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            textUI.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
        IsTyping = false;
    }
}
