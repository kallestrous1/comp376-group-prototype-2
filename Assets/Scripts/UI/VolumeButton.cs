using UnityEngine;

public class VolumeButton : BasicButton
{

    public GameObject volumeMenu;
    public GameObject controlsMenu;
    public GameObject MainButtons;

    public GameObject settingsButtons;

    public override void OnClicked()
    {
        base.OnClicked();
        if (!volumeMenu.activeSelf)
        {
            MainButtons.SetActive(false);
            controlsMenu.SetActive(false);
            volumeMenu.SetActive(true);
            settingsButtons.SetActive(false);
        }
        else
        {
            volumeMenu.SetActive(false);
        }

    }
}
