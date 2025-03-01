using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer m_myMixer;
    [SerializeField] private Slider m_musicSlider;
    [SerializeField] private Slider m_vfxSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadMusicAndVFX();
        }
        else
        {
            SetMusic();
            SetVFX();
        }
    }

    public void SetMusic()
    {
        float volume = m_musicSlider.value;
        m_myMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        if (volume == 0)
        {
            m_myMixer.SetFloat("Music", -80);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetVFX()
    {
        float volume = m_vfxSlider.value;
        m_myMixer.SetFloat("VFX", Mathf.Log10(volume) * 20);
        if (volume == 0)
        {
            m_myMixer.SetFloat("VFX", -80);
        }
        PlayerPrefs.SetFloat("VFXVolume", volume);
    }


    private void LoadMusicAndVFX()
    {
        m_musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        m_vfxSlider.value = PlayerPrefs.GetFloat("VFXVolume");

        SetMusic();
        SetVFX();
    }

}
