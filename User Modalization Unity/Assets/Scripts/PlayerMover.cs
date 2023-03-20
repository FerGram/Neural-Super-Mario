using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour
{
    public float horizontalSpeed = 20;
    public float jumpForce = 300;

    public UnityAction OnJumpStarted;
    public UnityAction OnJumpEnded;

    private Rigidbody2D _rb;

    private bool _isGrounded = false;
    private GroundDetector _groundDetector;

    private float _movementInput = 0;
    private bool _jump = false;

    private Vector2 velocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _groundDetector = GetComponentInChildren<GroundDetector>();

        if (_groundDetector != null) _groundDetector.OnGroundDetected += () => 
        {
            _isGrounded = true;
            OnJumpEnded.Invoke();
        };
    }

    private void Update()
    {
        _movementInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded) _jump = true;
    }

    private void FixedUpdate()
    {
        //Horizontal movement
        Vector2 velocity = new Vector2(_movementInput * horizontalSpeed, _rb.velocity.y);
        _rb.velocity = velocity;

        //Jump
        if (_jump) HandleJump();
    }

    private void HandleJump()
    {
        _rb.AddForce(Vector2.up * jumpForce);
        _isGrounded = false;

        OnJumpStarted.Invoke();

        _jump = false;
    }

    private void OnDestroy()
    {
        if (_groundDetector != null) _groundDetector.OnGroundDetected -= () => _isGrounded = true;
    }
}
