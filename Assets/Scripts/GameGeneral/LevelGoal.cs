using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public string currentLevel;
    public string nextLevelName;
    public bool returnToMenuOnCompletion;

    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        TryAdvance(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryAdvance(collision.gameObject);
    }

    void TryAdvance(GameObject other)
    {
        if (used || !other.CompareTag("Player")) return;
        if (string.IsNullOrEmpty(nextLevelName)) return;

        used = true;
        if (!returnToMenuOnCompletion)
            other.transform.position = GameManager.manager.StartPosition;
        GameManager.manager.MoveToScene(nextLevelName, currentLevel, false, returnToMenuOnCompletion);
    }
}
