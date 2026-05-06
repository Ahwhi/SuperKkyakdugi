using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour {
    [SerializeField] private Image _portraitImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TypeWriter _typeWriter;
    [SerializeField] private SpeakerPortraitDatabase _portraitDatabase;
    [SerializeField] private CanvasFader _canvasFader;

    private bool _isVisible;

    private void Awake() {
        gameObject.SetActive(false);

        // Awake() 실행 순서가 보장되지 않으므로 Instance 없으면 씬에서 직접 탐색
        var dm = DialogueManager.Instance != null
            ? DialogueManager.Instance
            : FindAnyObjectByType<DialogueManager>();

        if (dm == null) {
            Debug.LogError("[ChatWindow] DialogueManager not found.");
            return;
        }

        dm.OnLineChanged += Show;
        dm.OnSequenceEnd += Hide;

        if (_canvasFader == null)
            _canvasFader = GetComponent<CanvasFader>();
    }

    private void OnDestroy() {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnLineChanged -= Show;
        DialogueManager.Instance.OnSequenceEnd -= Hide;
    }

    private void Show(DialogueLine line) {
        if (!_isVisible) {
            gameObject.SetActive(true);
            _canvasFader.FadeIn(0.1f);
            _isVisible = true;
        }

        _nameText.text = line.speaker;
        _typeWriter.Play(line.text);

        Sprite portrait = _portraitDatabase != null ? _portraitDatabase.GetSprite(line.portraitKey) : null;
        _portraitImage.sprite = portrait;
        _portraitImage.gameObject.SetActive(portrait != null);
    }

    public void Hide() {
        _isVisible = false;
        _canvasFader.FadeOut(0.3f);
    }

    public void OnNextButton() {
        if (_typeWriter.IsTyping) {
            _typeWriter.Complete();
            return;
        }
        SoundManager.Instance.PlaySFX("Click");
        DialogueManager.Instance.Next();
    }

    public void OnSkipButton() {
        SoundManager.Instance.PlaySFX("Click");
        DialogueManager.Instance.Skip();
    }
}
