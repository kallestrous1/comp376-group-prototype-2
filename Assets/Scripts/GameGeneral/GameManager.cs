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

    private void Awake()
    {
        if (manager != null && manager != this)
        {
            Destroy(gameObject);
            return;
        }

        manager = this;
        DontDestroyOnLoad(gameObject);
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
    public void MoveToScene(string newScene, string previousScene, bool upBoost)
    {
        StartCoroutine(SceneTransition(newScene, previousScene, upBoost, null));
    }

    /// <summary>Called when the player dies: back to the start of the game.</summary>
    public void RespawnPlayer()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        StartCoroutine(SceneTransition(startScene, activeScene, false, startPosition));
    }

    public void ReturnToMenu()
    {
        StartCoroutine(ReturnToMenuRoutine());
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

    private IEnumerator SceneTransition(string newScene, string previousScene, bool upBoost, Vector2? placeAt)
    {
        GameStateManager.instance.ChangeState(GameState.movingScene);
        ShowLoading(true);

        Rigidbody2D body = Player.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;

        if (newScene != previousScene)
        {
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

        if (newScene != previousScene)
        {
            yield return Unload(previousScene);
        }

        ShowLoading(false);
        GameStateManager.instance.ChangeState(GameState.Play);
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        Time.timeScale = 1f;
        GameStateManager.instance.ChangeState(GameState.movingScene);
        ShowLoading(true);

        yield return LoadAdditive(menuScene, true);

        // Unload everything except this manager's scene and the menu.
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != gameObject.scene && scene.name != menuScene)
            {
                yield return Unload(scene.name);
            }
        }

        ShowLoading(false);
        GameStateManager.instance.ChangeState(GameState.MainMenu);
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
