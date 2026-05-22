using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    void Start()
    {
        // Auto-destroy after 10 seconds to prevent memory leaks from missed collisions
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Notify enemy so it can play death effects before being destroyed
        EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
        if (enemy != null)
            enemy.Die();

        Destroy(gameObject);
    }
}
