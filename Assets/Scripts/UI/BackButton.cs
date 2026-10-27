using UnityEngine;

public class BackButton : BasicButton
{
    public GameObject[] displaysToClose;
    public GameObject[] displaysToOpen;

    public override void OnClicked()
    {
        base.OnClicked();
        foreach (GameObject display in displaysToClose)
        {
            display.SetActive(false);
        }
        foreach (GameObject display in displaysToOpen)
        {
            display.SetActive(true);
        }
    }

}
