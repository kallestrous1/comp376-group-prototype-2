using UnityEngine;

public class SceneDoorScript : MonoBehaviour
{
    public Vector2 newPlayerPosition;
    public bool upBoost = false;
    public string nextScene;
    public string previousScene;

    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;

        other.transform.position = newPlayerPosition;
        GameManager.manager.MoveToScene(nextScene, previousScene, upBoost);
    }
}
