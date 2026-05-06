using UnityEngine;
using UnityEngine.InputSystem;

// Collider2D is abstract - attach CapsuleCollider2D or BoxCollider2D manually in the Inspector.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour {
    // ──────────────────────────────────────────────
    // Inspector
    // ──────────────────────────────────────────────
    [Header("Identity")]
    [SerializeField] private string _playerId = "Player1";

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 7f;

    [Header("Jump Feel")]
    [SerializeField] private float _riseMultiplier = 2f;
    [SerializeField] private float _fallMultiplier = 4f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    // ──────────────────────────────────────────────
    // Animator Parameter Keys
    // ──────────────────────────────────────────────
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimJump = Animator.StringToHash("Jump");

    // ──────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────
    public enum PlayerState { Idle, Moving, Jumping, Falling }

    private PlayerState _state = PlayerState.Idle;

    public PlayerState State {
        get => _state;
        private set {
            if (_state == value) return;
            _state = value;
            OnStateChanged?.Invoke(_state);
        }
    }

    // ──────────────────────────────────────────────
    // Events
    // ──────────────────────────────────────────────
    public event System.Action<PlayerState>       OnStateChanged;
    public event System.Action<GameObject>        OnQuestObjectHit;
    public event System.Action<GameObject>        OnObstacleHit;
    public event System.Action<string, GameObject> OnTaggedTrigger;

    // ──────────────────────────────────────────────
    // Public Read-only Properties
    // ──────────────────────────────────────────────
    public bool IsGrounded { get; private set; }
    public Vector2 Velocity => _rb.linearVelocity;
    public string PlayerId { get; private set; } = "Player";

    // ──────────────────────────────────────────────
    // Public Writable Properties
    // ──────────────────────────────────────────────
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }

    // ──────────────────────────────────────────────
    // Internal
    // ──────────────────────────────────────────────
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    private bool _jumpRequested;
    private bool _isFacingRight = true;
    private float _moveIdleTimer;
    private float _baseGravityScale;

    private const float MoveIdleGrace = 0.08f;

    // ──────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────
    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _baseGravityScale = _rb.gravityScale;

        if (GetComponent<Collider2D>() == null)
            Debug.LogError("[Player] Collider2D 없음. CapsuleCollider2D 또는 BoxCollider2D를 추가하세요.");
    }

    private void Start() {
        PlayerManager.Instance.RegisterPlayer(_playerId, this);
    }

    private void OnDestroy() {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.UnregisterPlayer(_playerId);
    }

    private void Update() {
        CheckGround();
        TickMoveIdleTimer();
        UpdateState();
        UpdateAnimator();
    }

    private void FixedUpdate() {
        ApplyMovement();
        ApplyJump();
        ApplyGravityModifier();
    }

    // ──────────────────────────────────────────────
    // Input Callbacks (PlayerInput - Send Messages)
    // ──────────────────────────────────────────────
    private void OnMove(InputValue value) {
        _moveInput = value.Get<Vector2>();
        UpdateFacingDirection(_moveInput.x);
    }

    private void OnJump(InputValue value) {
        if (value.isPressed && IsGrounded) {
            SoundManager.Instance.PlaySFX("Jump");
            _jumpRequested = true;
        }

    }

    // ──────────────────────────────────────────────
    // Physics
    // ──────────────────────────────────────────────
    private void ApplyMovement() {
        _rb.linearVelocity = new Vector2(_moveInput.x * _moveSpeed, _rb.linearVelocity.y);
    }

    private void ApplyJump() {
        if (!_jumpRequested) return;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        _animator.SetTrigger(AnimJump);
        _jumpRequested = false;
    }

    private void ApplyGravityModifier() {
        if (!IsGrounded)
            _rb.gravityScale = _rb.linearVelocity.y < 0f
                ? _baseGravityScale * _fallMultiplier
                : _baseGravityScale * _riseMultiplier;
        else
            _rb.gravityScale = _baseGravityScale;
    }

    private void TickMoveIdleTimer() {
        if (Mathf.Abs(_moveInput.x) > 0.01f)
            _moveIdleTimer = MoveIdleGrace;
        else if (_moveIdleTimer > 0f)
            _moveIdleTimer -= Time.deltaTime;
    }

    private void CheckGround() {
        if (_groundCheck == null) return;
        IsGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    // ──────────────────────────────────────────────
    // Flip
    // ──────────────────────────────────────────────
    private void UpdateFacingDirection(float directionX) {
        if (directionX > 0f && !_isFacingRight)
            Flip();
        else if (directionX < 0f && _isFacingRight)
            Flip();
    }

    private void Flip() {
        _isFacingRight = !_isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    // ──────────────────────────────────────────────
    // State & Animator
    // ──────────────────────────────────────────────
    private void UpdateState() {
        if (!IsGrounded)
            State = _rb.linearVelocity.y > 0.01f ? PlayerState.Jumping : PlayerState.Falling;
        else if (Mathf.Abs(_moveInput.x) > 0.01f || _moveIdleTimer > 0f)
            State = PlayerState.Moving;
        else
            State = PlayerState.Idle;
    }

    private void UpdateAnimator() {
        bool hasIntent = Mathf.Abs(_moveInput.x) > 0.01f || _moveIdleTimer > 0f;
        float speed = hasIntent ? Mathf.Max(Mathf.Abs(_moveInput.x), 0.1f) : 0f;
        _animator.SetFloat(AnimSpeed, speed);
        _animator.SetBool(AnimIsGrounded, IsGrounded);
    }

    // ──────────────────────────────────────────────
    // Collision
    // ──────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Quest"))
            OnQuestObjectHit?.Invoke(other.gameObject);

        OnTaggedTrigger?.Invoke(other.tag, other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Obstacle")) {
            OnObstacleHit?.Invoke(collision.gameObject);
        }
    }

    // ──────────────────────────────────────────────
    // Public Control API
    // ──────────────────────────────────────────────
    public void Initialize(string id) {
        PlayerId = id;
    }

    public void SetPosition(Vector2 position) {
        _rb.position = position;
    }

    public void SetVelocity(Vector2 velocity) {
        _rb.linearVelocity = velocity;
    }

    public void ForceJump() {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
        _animator.SetTrigger(AnimJump);
    }

    public void SetInputEnabled(bool enabled) {
        _playerInput.enabled = enabled;
        if (!enabled) {
            _moveInput = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    public void SetGravityScale(float scale) {
        _baseGravityScale = scale;
        if (IsGrounded)
            _rb.gravityScale = scale;
    }

    // ──────────────────────────────────────────────
    // Debug
    // ──────────────────────────────────────────────
    private void OnDrawGizmosSelected() {
        if (_groundCheck == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}
