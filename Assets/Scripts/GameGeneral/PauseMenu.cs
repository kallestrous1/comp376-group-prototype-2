using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (GameStateManager.instance.gameState == GameState.Paused)
        {
            Resume();
        }
        else if (GameStateManager.instance.gameState == GameState.Play)
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameStateManager.instance.ChangeState(GameState.Play);
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        GameStateManager.instance.ChangeState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void LoadMenu()
    {
        GameManager.manager.ReturnToMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
