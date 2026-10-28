using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent manager (lives in the SuperScene). Levels are loaded additively on top of the
/// persistent "Base Scene", which holds the player, camera and gameplay UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager manager { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider loadingBar;

    [Header("Scenes")]
    [SerializeField] private string menuScene = "Menu";
    [SerializeField] private string baseScene = "Base Scene";
    [SerializeField] private string startScene = "Level1";

    [Header("Spawn")]
    [SerializeField] private Vector2 startPosition = new Vector2(0, 2);

    private GameObject player;

    public Vector2 StartPosition => startPosition;

    private void Awake()
    {
        if (manager != null && manager != this)
        {
            Destroy(gameObject);
            return;
        }

        manager = this;
    }

    // SuperScene (this manager) is always the first scene; bring the menu up on top of it.
    private IEnumerator Start()
    {
        if (!SceneManager.GetSceneByName(menuScene).isLoaded)
        {
            yield return LoadAdditive(menuScene, true);
        }
        GameStateManager.instance.ChangeState(GameState.MainMenu);
    }

    #region Public API

    /// <summary>Called from the main menu: loads the base scene and the chosen level.</summary>
    public void NewGame(string levelScene)
    {
        if (!string.IsNullOrEmpty(levelScene))
            startScene = levelScene;
        StartCoroutine(NewGameRoutine());
    }

    /// <summary>Called by scene doors: loads newScene, then unloads previousScene.</summary>
    public void MoveToScene(string newScene, string previousScene, bool upBoost, bool unloadBaseScene = false)
    {
        StartCoroutine(SceneTransition(newScene, previousScene, upBoost, null, unloadBaseScene));
    }

    /// <summary>Death reloads this scene and places the player at the start.</summary>
    public void SetRespawnScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            startScene = sceneName;
    }

    /// <summary>Called when the player dies: back to the start of the current level.</summary>
    public void RespawnPlayer()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        StartCoroutine(SceneTransition(startScene, activeScene, false, startPosition, false, true));
    }

    /// <summary>Pause menu: load the menu on top of SuperScene and unload the level.</summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        MoveToScene(menuScene, SceneManager.GetActiveScene().name, false, true);
    }

    #endregion

    #region Scene loading

    private IEnumerator NewGameRoutine()
    {
        GameStateManager.instance.ChangeState(GameState.movingScene);
        ShowLoading(true);

        yield return LoadAdditive(baseScene);
        yield return LoadAdditive(startScene, true);
        yield return null; // let the new scenes initialise before touching the player

        PlacePlayer(startPosition);
        yield return Unload(menuScene);

        ShowLoading(false);
        GameStateManager.instance.ChangeState(GameState.Play);
    }

    private IEnumerator SceneTransition(string newScene, string previousScene, bool upBoost, Vector2? placeAt, bool unloadBaseScene = false, bool reloadIfSame = false)
    {
        GameStateManager.instance.ChangeState(GameState.movingScene);
        ShowLoading(true);

        Rigidbody2D body = Player.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;

        bool sameScene = newScene == previousScene;
        if (!sameScene)
        {
            yield return LoadAdditive(newScene, true);
            yield return null;
        }
        else if (reloadIfSame)
        {
            yield return Unload(newScene);
            yield return LoadAdditive(newScene, true);
            yield return null;
        }

        if (placeAt.HasValue)
        {
            PlacePlayer(placeAt.Value);
            Player.GetComponent<PlayerHealth>().ResetHealth();
        }

        body.bodyType = RigidbodyType2D.Dynamic;
        if (upBoost)
        {
            Player.GetComponent<PlayerController>().Jump();
        }

        if (!sameScene)
        {
            yield return Unload(previousScene);
        }

        if (unloadBaseScene)
        {
            Time.timeScale = 1f;
            yield return Unload(baseScene);
        }

        ShowLoading(false);
        GameStateManager.instance.ChangeState(unloadBaseScene ? GameState.MainMenu : GameState.Play);
    }

    private IEnumerator LoadAdditive(string sceneName, bool makeActive = false)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone)
        {
            if (loadingBar) loadingBar.value = load.progress;
            yield return null;
        }

        if (makeActive)
        {
            Scene loaded = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loaded);
            SceneMusicPlayer.PlayForScene(loaded);
        }
    }

    private IEnumerator Unload(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    #endregion

    #region Helpers

    private GameObject Player
    {
        get
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");
            return player;
        }
    }

    private void PlacePlayer(Vector2 position)
    {
        Player.transform.position = position;

        Rigidbody2D body = Player.GetComponent<Rigidbody2D>();
        if (body.bodyType == RigidbodyType2D.Dynamic)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private void ShowLoading(bool show)
    {
        if (loadingScreenPanel) loadingScreenPanel.SetActive(show);
        if (show && loadingBar) loadingBar.value = 0;
    }

    #endregion
}
