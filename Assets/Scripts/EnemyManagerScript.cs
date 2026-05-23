using UnityEngine;

public class EnemyManagerScript : MonoBehaviour
{
    // References to components
    [SerializeField] private Rigidbody2D _rigidBody;

    // movement variables
    private Vector2 movementDirection;
    private int _movementInput;
    private Vector2 velocity = Vector2.zero;
    [SerializeField] private float _maxSpeed;

    public bool Corrupted { get; private set; } = true;

    bool playerSpotted = false;

    bool _facingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        movementDirection = Vector2.right * _movementInput;
        //velocity
        velocity = movementDirection.normalized * _maxSpeed;
        
        Vector2 horizontalForce = new Vector2(velocity.x, 0f);
        _rigidBody.AddForce(horizontalForce, ForceMode2D.Impulse);

        Vector2 v = _rigidBody.linearVelocity;
        v.x = Mathf.Clamp(v.x, -_maxSpeed, _maxSpeed);
        _rigidBody.linearVelocity = v;


        /////////
        RaycastHit2D hit;

        if(Physics2D.Raycast(transform.position, Vector2.left, Mathf.Infinity, LayerMask.GetMask("Player")))
        {
            playerSpotted = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
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
}
