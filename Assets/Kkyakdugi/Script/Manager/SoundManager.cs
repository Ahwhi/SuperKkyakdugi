using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundDatabase _soundDatabase;
    [SerializeField] private AudioSource   _bgmSource;
    [SerializeField] private AudioSource   _sfxSource;

    public SoundDatabase soundDatabase => _soundDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        RefreshVolumes();
    }

    public void RefreshVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _bgmSource.volume = master * PlayerPrefs.GetFloat("BGMVolume", 1f);
        _sfxSource.volume = master * PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayBGM(string key)     => PlayBGM(_soundDatabase.GetClip(key));
    public void PlaySFX(string key)     => PlaySFX(_soundDatabase.GetClip(key));

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void StopBGM()   => _bgmSource.Stop();
    public void PauseBGM()  => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) _sfxSource.PlayOneShot(clip);
    }

    public void SetBGMVolume(float volume) => _bgmSource.volume = Mathf.Clamp01(volume);
    public void SetSFXVolume(float volume) => _sfxSource.volume = Mathf.Clamp01(volume);
}
