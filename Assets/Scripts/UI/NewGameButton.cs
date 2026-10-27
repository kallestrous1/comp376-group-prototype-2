using System.Collections;
using UnityEngine;

public class NewGameButton : BasicButton
{
    public AudioClip startGameSound;
    private bool clicked = false;

    public void StartNewGame()
    {
        if (clicked) return;
        clicked = true;

        if (startGameSound)
        {
            AudioManager.Instance.Play(startGameSound);
        }
        StartCoroutine(LoadAfterSound());
    }

    private IEnumerator LoadAfterSound()
    {
        // Give the click sound a moment to start before the scene load hitches the frame.
        yield return new WaitForSeconds(0.05f);
        GameManager.manager.NewGame();
    }
}
