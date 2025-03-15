using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    
    [SerializeField]
    private float _movementSpeed;
    
    [SerializeField]
    private float _jumpForce;

    [SerializeField]
    private float _gravityScale;

    [SerializeField]
    private float _fallingGravityScale;
    
    
    private bool _jumpRequested = false;
    

    [Header("Read-only Values")]
    
    [SerializeField]
    private bool _grounded = true;
    
    [SerializeField]
    private Vector2 _movementVector = Vector2.zero;

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
        _rigidbody.linearVelocityX = _movementVector.x * _movementSpeed;

        if(_jumpRequested)
        {
            if(_grounded)
            {
                _rigidbody.linearVelocity += Vector2.up * _jumpForce;
            }
            _jumpRequested = false;
        }

        if(_rigidbody.linearVelocityY >= 0)
        {
            _rigidbody.gravityScale = _gravityScale;
        }
        else
        {
            _rigidbody.gravityScale = _fallingGravityScale;
            
        }

    }

    void OnMove(InputValue value)
    {
        _movementVector = value.Get<Vector2>();
    }

    void OnJump()
    {
        if(!_jumpRequested)
        {
            _jumpRequested = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision: "+collision.gameObject.name);
        if(collision.gameObject.CompareTag("Floor"))
        {
            _grounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("Collision Exit: "+collision.gameObject.name);
        if(collision.gameObject.CompareTag("Floor"))
        {
            _grounded = false;
        }
    }


}
