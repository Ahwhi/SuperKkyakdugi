using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : MonoBehaviour {
    public Slider slider;

    public enum VolumeType { Master, BGM, SFX }
    public VolumeType type;

    void Start() {
        slider = GetComponent<Slider>();

        string key = GetKey();

        slider.value = PlayerPrefs.GetFloat(key, 1f);
        slider.onValueChanged.AddListener(OnChangedValue);
    }

    public void OnChangedValue(float value) {
        PlayerPrefs.SetFloat(GetKey(), value);
        PlayerPrefs.Save();
        SoundManager.Instance.RefreshVolumes();
    }

    string GetKey() {
        return type.ToString() + "Volume";
    }
}