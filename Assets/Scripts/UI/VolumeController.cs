using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioMixer musicMixer;
    public AudioMixer effectsMixer;

    public Slider musicSlider;
    public Slider effectsSlider;

    public string musicVolume;
    public string effectsVolume;

    public void SetMusicVolume(float sliderValue)
    {
        musicMixer.SetFloat(musicVolume, Mathf.Log10(sliderValue) * 20);
    }

    public void SetEffectsVolume(float sliderValue)
    {
        effectsMixer.SetFloat(effectsVolume, Mathf.Log10(sliderValue) * 20);
    }
}
