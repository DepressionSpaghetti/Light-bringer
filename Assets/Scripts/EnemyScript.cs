using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyScript : MonoBehaviour
{
    // --- Patrol ---
    [Header("Patrol")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _rayDistance = 0.6f;

    // --- Hit state ---
    [Header("Hit State")]
    [SerializeField] private Sprite _hitSprite;          // sprite shown after being shot
    [SerializeField] private float _hitSpeed = 1f; // slower when confused
    [SerializeField] private float _moveDuration = 1f; // seconds to move each burst
    [SerializeField] private float _minIdleTime = 3f; // min pause between bursts
    [SerializeField] private float _maxIdleTime = 6f; // max pause between bursts

    // --- Components ---
    private Rigidbody2D _rb;
    private Collider2D _col;
    private SpriteRenderer _sr;
    [SerializeField] private Collider2D damagerComponent;

    // --- State ---
    private bool _movingRight = true;
    private bool _isHit = false;

    // Coroutine writes this; FixedUpdate reads it to keep physics on the physics thread
    private float _hitMoveTimer = 0f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _rb.freezeRotation = true;
        
    }

    private void FixedUpdate()
    {
        if (_isHit)
        {
            // Hit state — velocity driven by coroutine timer, not raycasts
            if (_hitMoveTimer > 0f)
            {
                float dir = _movingRight ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dir * _hitSpeed, _rb.linearVelocity.y);
                _hitMoveTimer -= Time.fixedDeltaTime;
            }
            else
            {
                // Idle between bursts — stop horizontal movement
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            }
            return;
        }

        // Patrol state — wall + ledge detection as before
        if (HitsWallAhead() || ReachesLedge())
            Flip();

        float patrolDir = _movingRight ? 1f : -1f;
        _rb.linearVelocity = new Vector2(patrolDir * _speed, _rb.linearVelocity.y);
    }

    // -------------------------------------------------------------------------
    // Called by ProjectileScript on hit — enters hit state permanently

    public void OnHit()
    {
        if (_isHit) return; // already hit, ignore further projectiles

        _isHit = true;

        // Swap sprite
        if (_hitSprite != null)
            _sr.sprite = _hitSprite;

        // Stop immediately so the state change is visually clear
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        StartCoroutine(HitMovementLoop());
    }

    // Cycle: idle (3-6 s) → move in random direction (1 s) → repeat
    private IEnumerator HitMovementLoop()
    {
        while (true)
        {
            // --- Idle phase ---
            _hitMoveTimer = 0f;
            yield return new WaitForSeconds(Random.Range(_minIdleTime, _maxIdleTime));

            // --- Move phase: pick random direction ---
            bool goRight = Random.value > 0.5f;
            if (goRight != _movingRight)
                Flip();

            // Setting the timer tells FixedUpdate to apply velocity for this many seconds
            _hitMoveTimer = _moveDuration;
            yield return new WaitForSeconds(_moveDuration);
        }
    }

    // -------------------------------------------------------------------------
    // Raycasts — only used in Patrol state

    private bool HitsWallAhead()
    {
        bool hitRight = CastWallRays(Vector2.right);
        bool hitLeft = CastWallRays(Vector2.left);
        return _movingRight ? hitRight : hitLeft;
    }

    private bool CastWallRays(Vector2 dir)
    {
        float edgeX = dir.x > 0f ? _col.bounds.max.x : _col.bounds.min.x;
        float top = _col.bounds.max.y - 0.05f;
        float mid = transform.position.y;
        float bottom = _col.bounds.min.y + 0.05f;

        float[] heights = { top, mid, bottom };
        foreach (float h in heights)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(edgeX, h), dir, _rayDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == _col) continue;
                if (hit.collider.CompareTag("Player")) continue;
                if (hit.collider.CompareTag("EnemyDamager")) continue;
                return true;
            }
        }
        return false;
    }

    private bool ReachesLedge()
    {
        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        float feetY = _col.bounds.min.y;
        float frontX = dir.x > 0f ? _col.bounds.max.x : _col.bounds.min.x;

        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(frontX, feetY), Vector2.down, 0.3f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Ground")) return false;
        }
        return true;
    }

    // -------------------------------------------------------------------------

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

        // In hit state the enemy is harmless — cannot kill the player
        if (_isHit) return;

        PlayerManagerScript player = collision.gameObject.GetComponent<PlayerManagerScript>();
        if (player != null) player.Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (_col == null) _col = GetComponent<Collider2D>();
        if (_col == null) return;

        float top = _col.bounds.max.y - 0.05f;
        float mid = transform.position.y;
        float bottom = _col.bounds.min.y + 0.05f;

        // Right wall rays (red)
        Gizmos.color = Color.red;
        float rightEdge = _col.bounds.max.x;
        foreach (float h in new[] { top, mid, bottom })
            Gizmos.DrawRay(new Vector2(rightEdge, h), Vector2.right * _rayDistance);

        // Left wall rays (cyan)
        Gizmos.color = Color.cyan;
        float leftEdge = _col.bounds.min.x;
        foreach (float h in new[] { top, mid, bottom })
            Gizmos.DrawRay(new Vector2(leftEdge, h), Vector2.left * _rayDistance);

        // Ledge ray (yellow)
        Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
        float feetY = _col.bounds.min.y;
        float frontX = dir.x > 0f ? _col.bounds.max.x : _col.bounds.min.x;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(new Vector2(frontX, feetY), Vector2.down * 0.3f);
    }
}
