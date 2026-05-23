using System.Collections;
using UnityEngine;

public class EnemyManagerScript2 : MonoBehaviour
{
    // References to components
    [SerializeField] private Rigidbody2D _rigidBody;

    // Movement variables
    [SerializeField] private float _maxSpeed = 2.0f;
    // Ground detection to avoid falling off platforms
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 1.0f;
    [SerializeField] private float _groundCheckAhead = 0.5f;

    // Decision timing and probabilities
    [SerializeField] private float _decisionIntervalMin = 1.0f;
    [SerializeField] private float _decisionIntervalMax = 3.0f;
    [SerializeField, Range(0f, 1f)] private float _idleChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _turnChance = 0.5f;

    // Internal state
    private int _movementInput = 1; // -1 = left, 0 = idle, 1 = right
    private float _nextDecisionTime;
    public bool Corrupted { get; private set; } = true;
    private bool _facingRight = true;

    private void Awake()
    {
        if (_rigidBody == null)
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }

        // Start moving in a random direction
        _movementInput = Random.value > 0.5f ? 1 : -1;
        _facingRight = _movementInput > 0;
        if (_facingRight) FaceRight(); else FaceLeft();

        // Schedule first decision
        _nextDecisionTime = Time.time + Random.Range(_decisionIntervalMin, _decisionIntervalMax);
    }

    private void Update()
    {
        if (Time.time >= _nextDecisionTime)
        {
            DecideNextAction();
        }
    }

    private void FixedUpdate()
    {
        // Check ground ahead only when moving
        if (_movementInput != 0)
        {
            Vector2 origin = (Vector2)transform.position + Vector2.right * (_movementInput * _groundCheckAhead);
            RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, _groundCheckDistance, _groundLayer);
            if (!groundHit)
            {
                // No ground ahead: turn around and give a small cooldown
                TurnAround();
                _nextDecisionTime = Time.time + 0.5f;
            }
        }

        // Apply horizontal movement deterministically
        float targetX = _movementInput * _maxSpeed;
        Vector2 v = _rigidBody.linearVelocity;
        v.x = targetX;
        _rigidBody.linearVelocity = v;
    }

    private void DecideNextAction()
    {
        float r = Random.value;

        if (r < _idleChance)
        {
            // Idle for a while
            _movementInput = 0;
            _nextDecisionTime = Time.time + Random.Range(_decisionIntervalMin * 0.5f, _decisionIntervalMax * 0.5f);
            return;
        }

        // Decide whether to turn or keep going
        float turnRoll = Random.value;
        if (turnRoll < _turnChance)
        {
            TurnAround();
        }
        else
        {
            // Keep the same facing; ensure movement input matches facing
            _movementInput = _facingRight ? 1 : -1;
        }

        // Schedule next decision
        _nextDecisionTime = Time.time + Random.Range(_decisionIntervalMin, _decisionIntervalMax);
    }

    private void TurnAround()
    {
        _movementInput = -_movementInput;
        if (_movementInput == 0)
        {
            // If currently idle, pick a direction opposite of current facing
            _movementInput = _facingRight ? -1 : 1;
        }

        if (_movementInput > 0) FaceRight(); else FaceLeft();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Corrupted = false;
        }
    }

    private void FaceRight()
    {
        _facingRight = true;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void FaceLeft()
    {
        _facingRight = false;
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    // Optional: draw debug rays in the editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Vector2 origin = (Vector2)transform.position + Vector2.right * (_movementInput * _groundCheckAhead);
        Gizmos.DrawLine(origin, origin + Vector2.down * _groundCheckDistance);
    }
}