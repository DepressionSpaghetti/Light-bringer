using System.Linq;
using Unity.VisualScripting;

//using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManagerScript : MonoBehaviour
{
    //References to components
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Rigidbody2D _projectile;
    [SerializeField] private Collider2D _projectileCollider;
    [SerializeField] private Animator _animator;

    //movement variables
    private Vector2 _movementInput;
    private Vector2 velocity = Vector2.zero;
    private Vector2 movementDirection;
    private bool _facingRight = true;

    //set movement speeds
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _maxRunSpeed;
    [SerializeField] private float _jumpForce;
    public float runSpeed = 1;

    //airborne check
    private bool isAirborne = false;

    //projectile variables
    [SerializeField] private float _projectileSpeed;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if(ControlManager.Instance == null)
        {
            Debug.LogError("ControlManager instance not found.");
            return;
        }

        ControlManager.Instance.Move += OnMove;
        ControlManager.Instance.Jump += OnJump;
        ControlManager.Instance.Run += OnRun;
        ControlManager.Instance.Shoot += OnShoot;

    }

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
        //calculate movement direction based on player input and current rotation
        movementDirection = Vector2.right * _movementInput;
        //velocity
        velocity = movementDirection.normalized * (_walkSpeed * runSpeed);
        float maxHorizontalSpeed = _walkSpeed * runSpeed;

        //move the player
        if (velocity != Vector2.zero)
        {
            Vector2 horizontalForce = new Vector2(velocity.x, 0f);
            _rigidBody.AddForce(horizontalForce, ForceMode2D.Impulse);

            Vector2 v = _rigidBody.linearVelocity;
            v.x = Mathf.Clamp(v.x, -maxHorizontalSpeed, maxHorizontalSpeed);
            _rigidBody.linearVelocity = v;

            //old velocity code
            //_rigidBody.linearVelocity = Mathf.Clamp(_rigidBody.linearVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
            //_rigidBody.AddForce((velocity + _rigidBody.linearVelocity), ForceMode2D.Impulse);
            //_rigidBody.linearVelocity = Vector2.ClampMagnitude(_rigidBody.linearVelocity, _walkSpeed * runSpeed);
        }
        if (velocity == Vector2.zero)
        {
            switch (isAirborne)
            {
                case true:

                    break;
                case false:
                    Vector2 v = _rigidBody.linearVelocity;
                    v.x = Mathf.Lerp(v.x, 0, 0.1f);
                    _rigidBody.linearVelocity = v;
                    //old airborne velocity code
                    //_rigidBody.linearVelocity = Vector2.Lerp(_rigidBody.linearVelocity, new Vector2(0, _rigidBody.linearVelocity.y), 0.1f);
                    break;
            }
        }
    }

    void OnMove(Vector2 value)
    {
        _movementInput = value;

        //flip facing based on horizontal input
        if (value.x > 0f && !_facingRight)
            FaceRight();
        else if (value.x < 0f && _facingRight)
            FaceLeft();

        _animator.SetBool("Moving", value != Vector2.zero ? true : false);
    }

    void OnJump()
    {
        if (!isAirborne)
        {
            Vector2 v = _rigidBody.linearVelocity;
            v.y = _jumpForce;
            _rigidBody.linearVelocity = v;

            //old jump velocity code
            //_rigidBody.AddForceY(_jumpForce + _rigidBody.linearVelocityX, ForceMode2D.Impulse);
            //_rigidBody.linearVelocityY = _jumpForce;
            //_rigidBody.linearVelocity = Vector2.ClampMagnitude(_rigidBody.linearVelocity, _walkSpeed * runSpeed);
        }

        _animator.SetTrigger("Jump");

    }

    void OnRun(bool value)
    {
        switch (value)
        {
            case true:
                runSpeed = _maxRunSpeed;
                break;
            case false:
                runSpeed = 1;
                break;
        }

        _animator.SetBool("Running", value);
    }

    void OnShoot()
    {
        //spawn projectile slightly in front of the player based on facing direction
        Vector2 spawnOffset = _facingRight ? Vector2.right * 0.3f : Vector2.left * 0.3f;
        Vector2 spawnPos = (Vector2)transform.position + spawnOffset;
        
        Rigidbody2D p = Instantiate(_projectile, spawnPos, Quaternion.identity);

        //ignore collision between player and projectile
        //Collider2D projCol = p.GetComponent<Collider2D>();
        if(_projectileCollider != null && _collider != null)
        {
            Physics2D.IgnoreCollision(_collider, _projectileCollider);
        }

        //set projectile direction based on player facing direction
        Vector2 projDirection = _facingRight ? Vector2.right : Vector2.left;

        //set projectile rotation and velocity
        p.transform.rotation = _facingRight ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        p.linearVelocityX = projDirection.x * _projectileSpeed;

        //old
        //p.linearVelocity = transform.right * _projectileSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            isAirborne = false;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            isAirborne = true;
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