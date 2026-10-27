using UnityEngine;

public class SettingButton : BasicButton
{
    public GameObject SettingsMenus;
    public GameObject MainButtons;

    public override void OnClicked()
    {
        base.OnClicked();
        SettingsMenus.SetActive(true);
        MainButtons.SetActive(false);
    }
}
