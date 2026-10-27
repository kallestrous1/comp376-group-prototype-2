using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicPlayer : MonoBehaviour
{

    public AudioClip sceneMusic;

    void Start()
    {
        PlaySceneMusic();
    }

    public void PlaySceneMusic()
    {
        if (sceneMusic == null || AudioManager.Instance == null)
            return;

        AudioSource source = AudioManager.Instance.MusicSource;
        if (source != null && source.isPlaying && source.clip == sceneMusic)
            return;

        AudioManager.Instance.PlayMusic(sceneMusic);
    }

    public static void PlayForScene(Scene scene)
    {
        if (!scene.IsValid())
            return;

        var players = FindObjectsByType<SceneMusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.gameObject.scene == scene)
                player.PlaySceneMusic();
        }
    }

    public static void PlayAll()
    {
        var players = FindObjectsByType<SceneMusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var player in players)
            player.PlaySceneMusic();
    }


}
