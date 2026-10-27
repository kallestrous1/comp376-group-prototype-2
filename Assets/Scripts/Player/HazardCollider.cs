using UnityEngine;

public class HazardCollider : MonoBehaviour
{
    public int damage;
    public AudioClip hitSound;
    public bool oneOff = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.gameObject.GetComponent<KnockBack>().isBeingKnockedBack)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                KnockBack knockBack = collision.gameObject.GetComponent<KnockBack>();
                Vector2 knockBackDirection = (collision.transform.position - transform.position).normalized;

                playerHealth.changeHealth(-damage, this.gameObject.name);
                knockBack.StartPlayerKnockback(knockBackDirection);

                if (hitSound)
                {
                    AudioManager.Instance.Play(hitSound);
                }
            }         
        }
        if(oneOff)
        {
            Destroy(this.transform.gameObject);
        }

    }

    private void OnParticleCollision(GameObject collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!collision.gameObject.GetComponent<KnockBack>().isBeingKnockedBack)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                KnockBack knockBack = collision.gameObject.GetComponent<KnockBack>();

                Vector2 knockBackDirection = (collision.transform.position - transform.position).normalized;

                playerHealth.changeHealth(-damage, this.gameObject.name);
                knockBack.StartPlayerKnockback(knockBackDirection);


                if (hitSound)
                {
                    AudioManager.Instance.Play(hitSound);
                }
            }
        }
        if (oneOff)
        {
            Destroy(this.transform.gameObject);
        }
    }
}
