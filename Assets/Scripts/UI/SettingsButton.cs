using UnityEngine;

public class SettingsButton : BasicButton
{

    public GameObject[] mainButtons;

    public GameObject settingsMenu;


    public override void OnClicked()
    {
        base.OnClicked();
        foreach(GameObject obj in mainButtons){
            obj.SetActive(false);
        }
        settingsMenu.SetActive(true);
    }
}
