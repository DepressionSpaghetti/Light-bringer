using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _rayDistance = 0.6f;

    private Rigidbody2D _rb;
    private bool _movingRight = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (HitsWallAhead() || ReachesLedge())
            Flip();

        float dir = _movingRight ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dir * _speed, _rb.linearVelocity.y);
    }

    private bool HitsWallAhead()
    {
        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _rayDistance, _wallLayer);
        return hit.collider != null;
    }

    private bool ReachesLedge()
    {
        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        Vector2 ledgeCheck = (Vector2)transform.position + dir * 0.5f + Vector2.down * 0.1f;
        RaycastHit2D floor = Physics2D.Raycast(ledgeCheck, Vector2.down, 1f, _wallLayer);
        return floor.collider == null;
    }

    private void Flip()
    {
        _movingRight = !_movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
            player.Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, dir * _rayDistance);

        Vector2 ledgeOrigin = (Vector2)transform.position + dir * 0.5f + Vector2.down * 0.1f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ledgeOrigin, Vector2.down);
    }
}
