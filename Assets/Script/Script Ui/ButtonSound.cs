using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayClick()
    {
        float sfxVol = PlayerPrefs.GetFloat("sfxVolume", 1f);

        audioSource.volume = sfxVol;
        audioSource.Play();
    }
}