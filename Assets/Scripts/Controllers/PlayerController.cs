using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controllers
{

    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private FamiliarController _familiarControllerReference;

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

        [SerializeField]
        public bool ActiveCharacter;

        private bool _jumpRequested = false;
        private bool switchCharacters = false;

        private UnityEvent<int> SwitchCamerasEvent = new();


        [Header("Read-only Values")]

        [SerializeField]
        private bool _grounded = true;

        [SerializeField]
        private Vector2 _movementVector = Vector2.zero;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var cinemachineController = FindFirstObjectByType<CinemachineController>();
            if(cinemachineController)
            {
                SwitchCamerasEvent.AddListener((int x) => cinemachineController.CinemachineSwapCameras(x));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (switchCharacters && ActiveCharacter)
            {
                SwapCharacters();
            }
        }

        void FixedUpdate()
        {
            if (ActiveCharacter)
            {
                _rigidbody.linearVelocityX = _movementVector.x * _movementSpeed;

                if (_jumpRequested)
                {
                    if (_grounded)
                    {
                        _rigidbody.linearVelocity += Vector2.up * _jumpForce;
                    }
                    _jumpRequested = false;
                }

                if (_rigidbody.linearVelocityY >= 0)
                {
                    _rigidbody.gravityScale = _gravityScale;
                }
                else
                {
                    _rigidbody.gravityScale = _fallingGravityScale;
                }
            }
        }

        void OnMove(InputValue value)
        {
            if (!ActiveCharacter)
                return;

            _movementVector = value.Get<Vector2>();
        }

        void OnJump()
        {
            if (!ActiveCharacter)
                return;

            if (!_jumpRequested)
            {
                _jumpRequested = true;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision: " + collision.gameObject.layer);
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _grounded = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            Debug.Log("Collision Exit: " + collision.gameObject.layer);
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _grounded = false;
            }
        }

        void OnSwap(InputValue value)
        {
            if (value.isPressed && !switchCharacters)
            {
                if (!ActiveCharacter)
                    return;

                Debug.Log("Swapping to player...");
                switchCharacters = true;
            }
        }

        private void SwapCharacters()
        {
            if (_familiarControllerReference)
            {
                ActiveCharacter = false;
                switchCharacters = false;
                _familiarControllerReference.ActiveCharacter = true;
                _rigidbody.linearVelocity = Vector2.zero;
                SwitchCamerasEvent.Invoke(2);
            }
        }

    }
}