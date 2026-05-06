using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private CanvasFader _pauseFader;
    [SerializeField] private CanvasFader _resultFader;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _episodeNameText;
    [SerializeField] private TextMeshProUGUI _episodeDiscriptionText;
    [SerializeField] private TextMeshProUGUI _bgmText;
    [SerializeField] private Image _episodeImage;
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Result Panel")]
    [SerializeField] private TextMeshProUGUI _resultMessage;
    [SerializeField] private Image _resultImage;
    [SerializeField] private Sprite _successSprite;
    [SerializeField] private Sprite _failSprite;

    private bool _isPaused;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() {
        GameDataManager.Instance.OnScoreChanged += UpdateScoreText;
        UpdateScoreText(GameDataManager.Instance.Score);

        _pauseFader?.SetVisibleImmediate(false);
        _resultFader?.SetVisibleImmediate(false);
        ShowTimer(false);
    }

    private void Update() {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    private void OnDestroy() {
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.OnScoreChanged -= UpdateScoreText;

        if (Instance == this) Instance = null;
    }

    // ──────────────────────────────────────────────
    // Pause
    // ──────────────────────────────────────────────
    public void TogglePause() {
        _isPaused = !_isPaused;
        if (_isPaused) _pauseFader?.FadeIn(0.2f);
        else _pauseFader?.FadeOut(0.2f);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void Resume() {
        if (_isPaused) TogglePause();
    }

    // ──────────────────────────────────────────────
    // HUD
    // ──────────────────────────────────────────────
    private void UpdateScoreText(int score) {
        if (_scoreText == null) return;
        _scoreText.text = $"x {score}";
    }

    public void SetEpisodeName(string name) {
        if (_episodeNameText == null) return;
        _episodeNameText.text = name;
    }

    public void SetEpisodeDiscription(string name) {
        if (_episodeDiscriptionText == null) return;
        _episodeDiscriptionText.text = name;
    }

    public void SetEpisodeImage(Sprite sprite) {
        if (_episodeImage == null) return;
        _episodeImage.sprite = sprite;
    }

    public void SetBGMName(string name) {
        if (_bgmText == null) return;
        _bgmText.text = $"♪ {name}";
    }

    public void SetTimer(float seconds) {
        if (_timerText == null) return;
        int m = Mathf.Max(0, Mathf.FloorToInt(seconds / 60));
        int s = Mathf.Max(0, Mathf.FloorToInt(seconds % 60));
        _timerText.text = $"{m:00}:{s:00}";
    }

    public void ShowTimer(bool show) {
        if (_timerText == null) return;
        _timerText.gameObject.SetActive(show);
    }

    public void SetRescueCount(int current, int total) {
        if (_scoreText == null) return;
        _scoreText.text = $"찾은 친구: {current}/{total}";
    }

    public void ShowRescueCount(bool show) {
        if (!show) UpdateScoreText(GameDataManager.Instance != null ? GameDataManager.Instance.Score : 0);
    }

    // ──────────────────────────────────────────────
    // Result
    // ──────────────────────────────────────────────
    public void ShowResultPanel(bool clear, string message = null) {
        _resultMessage.text = message ?? (clear ? "겜잘알 두기" : "겜알못 두기");
        _resultImage.sprite = clear ? _successSprite : _failSprite;
        _resultFader?.FadeIn(0.3f);
    }

    // ──────────────────────────────────────────────
    // Scene Transitions
    // ──────────────────────────────────────────────
    public void RestartGame() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void GoToTitle() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    // ──────────────────────────────────────────────
    // Misc
    // ──────────────────────────────────────────────
    public void QuitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
