using UnityEngine;

public enum GameState
{
    MainMenu,
    Play,
    Paused,
    movingScene,
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance { get; private set; }

    public GameState gameState = GameState.MainMenu;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        gameState = newState;
    }
}
