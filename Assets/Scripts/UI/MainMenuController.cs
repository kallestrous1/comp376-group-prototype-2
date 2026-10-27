using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public AudioClip quitSound;

    public void QuitGame()
    {
        if (quitSound)
        {
            AudioManager.Instance.Play(quitSound);
        }
        Application.Quit();
    }
}
