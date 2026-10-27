using UnityEngine;
using UnityEngine.Audio;
using System;
using Random = System.Random;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	public AudioSource[] EffectsSources = new AudioSource[5];
	public AudioSource[] LoopSources = new AudioSource[1];
	public AudioSource MusicSource;

	// Random pitch adjustment range.
	public float LowPitchRange = .95f;
	public float HighPitchRange = 1.05f;

	// Singleton instance.
	public static AudioManager Instance = null;

	float musicVolume = 1f;
	Coroutine musicFade;

	// Initialize the singleton instance.
	private void Awake()
	{
		// If there is not already an instance of SoundManager, set it to this.
		if (Instance == null)
		{
			Instance = this;
			if (MusicSource != null)
				musicVolume = MusicSource.volume;
		}
		//If an instance already exists, destroy whatever this object is to enforce the singleton.
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	// Play a single clip through the sound effects source.
	public void Play(AudioClip clip)
	{
		foreach(AudioSource source in EffectsSources)
        {
            if (!source.isPlaying)
            {
				source.clip = clip;
				source.Play();
				return;
			}
        }
		EffectsSources[0].clip = clip;
		EffectsSources[0].Play();
	}

	public AudioSource PlayLoop(AudioClip clip)
    {
		foreach (AudioSource source in LoopSources)
		{
			if (!source.isPlaying)
			{
				source.clip = clip;
				source.Play();
				return source;
			}
		}
		Debug.Log("replacing last loopsource");
		LoopSources[LoopSources.Length].clip = clip;
		LoopSources[LoopSources.Length].Play();
		return LoopSources[LoopSources.Length];
		
	}

	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		CancelMusicFade();
		musicFade = StartCoroutine(PlayMusicWhenLoaded(clip));
	}

	// WebGL hangs if a clip is played before the browser has finished decoding it.
	IEnumerator PlayMusicWhenLoaded(AudioClip clip)
	{
		if (clip.loadState == AudioDataLoadState.Unloaded)
			clip.LoadAudioData();
		while (clip.loadState == AudioDataLoadState.Loading)
			yield return null;

		musicFade = null;
		MusicSource.clip = clip;
		MusicSource.volume = musicVolume;
		MusicSource.Play();
	}

	public void ResetMusic()
	{
		CancelMusicFade();
		if (MusicSource != null)
		{
			MusicSource.Stop();
			MusicSource.volume = musicVolume;
		}
		SceneMusicPlayer.PlayAll();
	}

	void CancelMusicFade()
	{
		if (musicFade != null)
		{
			StopCoroutine(musicFade);
			musicFade = null;
		}
		if (MusicSource != null)
			MusicSource.volume = musicVolume;
	}

	// Play a random clip from an array, and randomize the pitch slightly.
	public void RandomSoundEffect(params AudioClip[] clips)
	{
		//int randomIndex = Random.Range(0, clips.Length);
		//float randomPitch = Random.Range(LowPitchRange, HighPitchRange);

		//EffectsSource.pitch = randomPitch;
		//EffectsSource.clip = clips[randomIndex];
		//EffectsSource.Play();
	}

	public void fadeMusic(float duration, AudioClip newMusic)
    {
		CancelMusicFade();
		musicFade = StartCoroutine(FadeOutMusic(duration, newMusic));
    }

	IEnumerator FadeOutMusic(float duration, AudioClip newMusic)
    {
		float currentTime = 0;
		while(currentTime < duration)
        {
			currentTime += Time.deltaTime;
			MusicSource.volume = Mathf.Lerp(musicVolume, 0, currentTime / duration);
			yield return null;
		}

		musicFade = null;
        if (newMusic)
			PlayMusic(newMusic);
		else if (MusicSource != null)
        {
			MusicSource.Stop();
			MusicSource.volume = musicVolume;
        }
	}
}
