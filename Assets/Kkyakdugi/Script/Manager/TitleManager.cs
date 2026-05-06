using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {
    public static TitleManager Instance { get; private set; }

    [SerializeField] private CanvasFader _settingFader;

    private bool _isSetting;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() {
        SoundManager.Instance.PlayBGM("Title");
        _settingFader?.SetVisibleImmediate(false);
    }

    public void OnStartGame() {
        SceneManager.LoadScene("GameScene");
    }

    public void OnSetting() {
        _isSetting = !_isSetting;
        if (_isSetting) _settingFader?.FadeIn(0.2f);
        else _settingFader?.FadeOut(0.2f);
    }

    public void OnQuitGame() {
        Application.Quit();
    }
}
