using UnityEngine;

public class ControlsButton : BasicButton
{

    public GameObject volumeMenu;
    public GameObject controlsMenu;
    public GameObject MainButtons;

    public GameObject settingsButtons;

    public override void OnClicked()
    {
        base.OnClicked();
        if (!controlsMenu.activeSelf)
        {
            MainButtons.SetActive(false);
            volumeMenu.SetActive(false);
            controlsMenu.SetActive(true);
            settingsButtons.SetActive(false);
        }
        else
        {       
            controlsMenu.SetActive(false);
        }

    }
}

