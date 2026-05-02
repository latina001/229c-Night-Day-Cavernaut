using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    public AudioSource musicSource; // เพลงประกอบ

    void Start()
    {
        // 🔄 โหลดค่าที่เคยเซฟ
        float musicVol = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("sfxVolume", 1f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        musicSource.volume = musicVol;

        // 🎚️ ฟังตอนเลื่อน
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("sfxVolume", value);
    }
}