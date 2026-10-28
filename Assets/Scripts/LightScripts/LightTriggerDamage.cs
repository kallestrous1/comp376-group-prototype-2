using UnityEngine;

public class LightTriggerDamage : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float damageInterval = 0.001f;

    float timer;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        timer += Time.fixedDeltaTime;
        if (timer < damageInterval)
            return;

        timer = 0f;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.changeHealth(-damage, gameObject.name);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            timer = 0f;
    }
}
