using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    private CanvasGroup _group;
    private Coroutine _active;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        _group.blocksRaycasts = true;
        Run(0f, 1f, duration, () =>
        {
            _group.interactable = true;
            onComplete?.Invoke();
        });
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        _group.interactable = false;
        _group.blocksRaycasts = false;
        Run(1f, 0f, duration, onComplete);
    }

    public void SetVisibleImmediate(bool visible)
    {
        if (_active != null) { StopCoroutine(_active); _active = null; }
        _group.alpha = visible ? 1f : 0f;
        _group.interactable = visible;
        _group.blocksRaycasts = visible;
    }

    private void Run(float from, float to, float duration, Action onComplete)
    {
        if (_active != null) StopCoroutine(_active);
        _active = StartCoroutine(Fade(from, to, duration, onComplete));
    }

    private IEnumerator Fade(float from, float to, float duration, Action onComplete)
    {
        float elapsed = 0f;
        _group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        _group.alpha = to;
        _active = null;
        onComplete?.Invoke();
    }
}
