using UnityEngine;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour
{   
    public enum PlayerCurrentState
    {
        Idle,
        Walk,
        Run,
        CrouchIdle,
        CrouchWalk,
        JumpA,
        JumpB,
        JumpC,
        StepUp
    }
    
    [Header("CURRENT STATE TRACKER")] 
    public PlayerCurrentState cState = PlayerCurrentState.Idle;
    public static PlayerController Instance { get; private set; }

    [Header("SPEED SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float normalMovementSpeed = 5f;
    [Range(0f, 10f)] [SerializeField] private float runSpeed = 20f;

    [Header("CROUCH SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float crouchSpeed = 10f;
    [Header("JUMP SETTINGS")]
    [Range(0f, 20f)][SerializeField] private float normalJumpForce = 0.0f;
    [Range(0f, 20f)][SerializeField] private float runJumpForce = 14f;
    
    [Header("STEP UP SETTINGS")]
    [Range(0f, 5f)] [SerializeField] private float stepUpSpeed = 2f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float stepDetectionDistance = 0.5f;
    
    [Header("ROTATION SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float rotationSpeed = 5f;

    [Header("RESPAWN SETTINGS")]
    [SerializeField] private Transform respawnPoint;

    [Header("INTERACTION SETTINGS")]
    [SerializeField] private LayerMask puzzleG;
    [SerializeField] private LayerMask interactable;
    [SerializeField] private float playerInteractionRadius = 2f;

    [Header("ANIMATION SETTINGS")]
    [SerializeField] Animator animator;
    [Range(0.5f, 2f)] [SerializeField] private float idleASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float walkASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float runASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float crouchIdleASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float crouchWalkASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float jumpAASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float jumpBASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float jumpCASpeed = 1f;
    [Range(0.5f, 2f)] [SerializeField] private float stepUpASpeed = 1f;

    [Header("SOUND SETTINGS")]
    [SerializeField] private AudioClip footstepSFX;
    [SerializeField] private AudioClip jumpSFX;
    private GameObject nearestInteractable;
    private PuzzleInteractLogic nearestPuzzle;
    private Vector2 _currentInput;
    private Vector3 _velocity;
    private float _verticalVelocity;
    private CharacterController _characterController;
    private Camera _camera;
    private float _currentSpeed;
    private float gravity = 9.81f;
    public bool screenPaused;
    public bool _isRunning = false;
    public bool _isCrouching = false;
    private bool _playerIsGrounded = true;
    private bool _isOnStairs = false;
    private float normalHeight;
    private float crouchHeight = 0.6f;
    private Coroutine _footstepCoroutine;
    private const float WalkStepInterval = 0.5f;
    private const float RunStepInterval = 0.25f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;
        
        normalHeight = _characterController.height;
        
        InputManager.Instance.RunPerformed += RunOnPerformed;
        InputManager.Instance.RunCanceled += RunOnCanceled;
        InputManager.Instance.CrouchPerformed += CrouchOnPerformed;
        InputManager.Instance.CrouchOnCanceled += CrouchOnCanceled;
        InputManager.Instance.JumpPerformed += JumpOnPerformed;
        InputManager.Instance.PickUpPerformed += OnInteractPerformed;
        _currentSpeed = normalMovementSpeed;
        if (GameManager.Instance){
            respawnPoint = GameManager.Instance.GetCurrentCheckpointPosition();
        }
        SwitchState(PlayerCurrentState.Idle);
    }

    private void Update()
    {
        if (cState == PlayerCurrentState.JumpA || cState == PlayerCurrentState.JumpB ||cState == PlayerCurrentState.JumpC)
        {
            ChangeToJumpState();
        }
        else if (cState == PlayerCurrentState.CrouchIdle || cState == PlayerCurrentState.CrouchWalk)
        {
            ChangeToCrouchState();
        }
        else if (cState == PlayerCurrentState.Run)
        {
            ChangeToRunState();
        }
        else if (cState == PlayerCurrentState.Walk)
        {
            ChangeToWalkState();
        }
        else if (cState == PlayerCurrentState.StepUp)
        {
            ChangeToStepUpState();
        }
        else if (cState == PlayerCurrentState.Idle)
        {
            ChangeToIdleState();
        }

        DetectNearestPuzzle();
        UpdateVerticalVelocity();
        CheckForStairs();
        ApplyTotalVelocity();
        if (GameManager.Instance){
            respawnPoint = GameManager.Instance.GetCurrentCheckpointPosition();
        }
    }

    private void ChangeToIdleState()
    {
        if (cState != PlayerCurrentState.Idle)return;

        UpdateMovementAndRotation();

        if (_currentInput.magnitude > 0.1f)
        {
            SwitchState(PlayerCurrentState.Walk);
        }
        else if (_isCrouching)
        {
            SwitchState(PlayerCurrentState.CrouchIdle);
        }
    }
    private void ChangeToWalkState()
    {
        if (cState != PlayerCurrentState.Walk) return;

        UpdateMovementAndRotation();

        if (_currentInput.magnitude <= 0.1f)
        {
            SwitchState(PlayerCurrentState.Idle);
        }
        else if (_isRunning)
        {
            SwitchState(PlayerCurrentState.Run);
        }
        else if (_isCrouching)
        {
            SwitchState(PlayerCurrentState.CrouchWalk);
        }
    }
    private void ChangeToRunState()
    {
        if (cState != PlayerCurrentState.Run) return;

        UpdateMovementAndRotation();

        if (!_isRunning || _currentInput.magnitude <= 0.1f)
        {
            SwitchState(PlayerCurrentState.Walk);
        }
    }
    private void ChangeToCrouchState()
    {
        if (cState != PlayerCurrentState.CrouchIdle && cState != PlayerCurrentState.CrouchWalk) return;

        UpdateMovementAndRotation();

        if (_currentInput.magnitude > 0.1f)
        {
            if (cState == PlayerCurrentState.CrouchIdle)
                SwitchState(PlayerCurrentState.CrouchWalk);
        }
        else
        {
            if (cState == PlayerCurrentState.CrouchWalk)
                SwitchState(PlayerCurrentState.CrouchIdle);
        }
        if (!_isCrouching)
        {
            SwitchState(PlayerCurrentState.Idle);
        }
    }
    private void ChangeToJumpState()
    {
        if (cState != PlayerCurrentState.JumpA && cState != PlayerCurrentState.JumpB && cState != PlayerCurrentState.JumpC) return;

        UpdateMovementAndRotation();

        _playerIsGrounded = _characterController.isGrounded;

        if (cState == PlayerCurrentState.JumpA && _verticalVelocity < 0)
        {
            SwitchState(PlayerCurrentState.JumpB);
        }
        else if (cState == PlayerCurrentState.JumpB && _playerIsGrounded && _verticalVelocity <= 0)
        {
            SwitchState(PlayerCurrentState.JumpC);
        }
        else if (cState == PlayerCurrentState.JumpC)
        {
            if (_currentInput.magnitude > 0.1f)
            {
                SwitchState(_isRunning 
                ? PlayerCurrentState.Run : 
                PlayerCurrentState.Walk);
            }
            else
            {
                SwitchState(PlayerCurrentState.Idle);
            }
        }
    }
    private void ChangeToStepUpState()
    {
        if (cState != PlayerCurrentState.StepUp) return;
        if (!_isOnStairs || _currentInput.magnitude <= 0.1f)
        {
            SwitchState(PlayerCurrentState.Walk);
        }
    }
    private void CheckForStairs()
    {
        if (_playerIsGrounded && _currentInput.magnitude > 0.1f)
        {
            Vector3 forward = transform.forward;
            Vector3 stepPos = transform.position + forward * stepDetectionDistance;
            stepPos.y += stepHeight;
            if (Physics.Raycast(stepPos, Vector3.down, out RaycastHit hit, stepHeight * 2))
            {
                if (hit.collider.gameObject.CompareTag("Stairs"))
                {
                    _isOnStairs = true;
                    return;
                }
            }
        }
        _isOnStairs = false;
    }
    private void UpdateMovementAndRotation()
    {
        if (screenPaused) return;
        if (InputManager.Instance != null)
            _currentInput = InputManager.Instance.GetMovementInput();
        else return;
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        Vector3 desiredMove = right * _currentInput.x + forward * _currentInput.y;
        desiredMove.y = 0f;
        if (desiredMove.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMove);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        _velocity = desiredMove * _currentSpeed;
        _playerIsGrounded = _characterController.isGrounded;
        
        if (_playerIsGrounded && _verticalVelocity <= 0)
        {
            _verticalVelocity = -2f;
        }
        bool isMovingOnGround = _currentInput.magnitude > 0.1f && _playerIsGrounded;
        if (isMovingOnGround && _footstepCoroutine == null)
        {
            _footstepCoroutine = StartCoroutine(FootstepLoop());
        }
        else if (!isMovingOnGround && _footstepCoroutine != null)
        {
            StopCoroutine(_footstepCoroutine);
            _footstepCoroutine = null;
        }
    }
    private IEnumerator FootstepLoop()
    {
        while (true)
        {
            float interval = _isRunning ? RunStepInterval : WalkStepInterval;
            yield return new WaitForSeconds(interval);
            if (_currentInput.magnitude > 0.1f && _playerIsGrounded && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySoundWithRandomPitch(footstepSFX);
            }
        }
    }
    public void setPause(bool p) => screenPaused = p;
    private void ApplyTotalVelocity()
    {
        Vector3 totalVelocity = new Vector3(_velocity.x, _verticalVelocity, _velocity.z);
        _characterController.Move(totalVelocity * Time.deltaTime);
    }
    private void SwitchState(PlayerCurrentState newState)
    {
        if (cState == newState) return;
        cState = newState;
        if (animator != null)
        {
            switch (newState)
            {
                case PlayerCurrentState.Idle:
                    animator.CrossFade("Idle", 0.2f);
                    animator.SetFloat("Speed", 0f);
                    break;
                case PlayerCurrentState.Walk:
                    animator.CrossFade("Walk", 0.2f);
                    animator.SetFloat("Speed", walkASpeed);
                    break;
                case PlayerCurrentState.Run:
                    animator.CrossFade("Run", 0.2f);
                    animator.SetFloat("Speed", runASpeed);
                    break;
                case PlayerCurrentState.CrouchIdle:
                    animator.CrossFade("CrouchIdle", 0.2f);
                    animator.SetFloat("Speed", crouchIdleASpeed);
                    break;
                case PlayerCurrentState.CrouchWalk:
                    animator.CrossFade("CrouchWalk", 0.2f);
                    animator.SetFloat("Speed", crouchWalkASpeed);
                    break;
                case PlayerCurrentState.JumpA:
                    animator.CrossFade("JumpA", 0.2f);
                    animator.SetFloat("Speed", jumpAASpeed);
                    break;
                case PlayerCurrentState.JumpB:
                    animator.CrossFade("JumpB", 0.2f);
                    animator.SetFloat("Speed", jumpBASpeed);
                    break;
                case PlayerCurrentState.JumpC:
                    animator.CrossFade("JumpC", 0.2f);
                    animator.SetFloat("Speed", jumpCASpeed);
                    break;
                case PlayerCurrentState.StepUp:
                    animator.CrossFade("StepUp", 0.2f);
                    animator.SetFloat("Speed", stepUpASpeed);
                    break;
            }
        }
        switch (newState)
        {
            case PlayerCurrentState.Idle:
            case PlayerCurrentState.Walk:
                _currentSpeed = normalMovementSpeed;
                break;
            case PlayerCurrentState.Run:
                _currentSpeed = runSpeed;
                break;
            case PlayerCurrentState.CrouchIdle:
            case PlayerCurrentState.CrouchWalk:
                _currentSpeed = crouchSpeed;
                break;
            case PlayerCurrentState.StepUp:
                _currentSpeed = stepUpSpeed;
                break;
        }
    }
    private void RunOnPerformed()
    {
        if (!_playerIsGrounded || _isCrouching) return;
        _isRunning = true;
        _currentSpeed = runSpeed;
        if (cState == PlayerCurrentState.Walk) SwitchState(PlayerCurrentState.Run);
    }
    private void RunOnCanceled()
    {
        _isRunning = false;
        _currentSpeed = normalMovementSpeed;
        if (cState == PlayerCurrentState.Run) SwitchState(PlayerCurrentState.Walk);
    }
    private void JumpOnPerformed()
    {
        if (_playerIsGrounded && cState != PlayerCurrentState.JumpA &&
            cState != PlayerCurrentState.JumpB && cState != PlayerCurrentState.JumpC)
        {
            float jumpForceA = _isRunning ? runJumpForce : normalJumpForce;
            _verticalVelocity = jumpForceA;
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(jumpSFX);
            SwitchState(PlayerCurrentState.JumpA);
        }
    }
    private void CrouchOnPerformed()
    {
        if (_isCrouching)return;
        _isCrouching = true;
        _isRunning = false;
        _characterController.height = crouchHeight;
        _characterController.center = new Vector3(0, crouchHeight / 3f, 0);
        SwitchState(PlayerCurrentState.CrouchIdle);
    }
    private void CrouchOnCanceled()
    {
        if (!_isCrouching) return;
        _isCrouching = false;
        _characterController.height = normalHeight;
        _characterController.center = new Vector3(0, normalHeight / 3f, 0);
        SwitchState(PlayerCurrentState.Idle);
    }
    private void UpdateVerticalVelocity()
    {
        if (!_playerIsGrounded)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }
        _velocity.y = _verticalVelocity;
    }
    private void DetectNearestPuzzle()
    {
        LayerMask combined = puzzleG | interactable;
        Collider[] c = Physics.OverlapSphere(transform.position, playerInteractionRadius, combined);
        nearestInteractable = null;
        nearestPuzzle = null;
        float nearestDistance = playerInteractionRadius;
        for (int i = 0; i < c.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, c[i].transform.position);
            if (distance < nearestDistance)
            {
                nearestInteractable = c[i].gameObject;
                nearestPuzzle = nearestInteractable.GetComponent<PuzzleInteractLogic>();
                nearestDistance = distance;
            }
        }
    }
    private void OnInteractPerformed()
    {
        if (nearestPuzzle)
        {
            nearestPuzzle.OpenPuzzle();
        }
        else if (nearestInteractable != null)
        {
            var interactable = nearestInteractable.GetComponent<IInteractable>();
            if (interactable != null)
                interactable.Interact();
        }
    }
    public Transform GetTransform(out bool playerOnSight)
    {
        playerOnSight = true;
        return transform;
    }
    public void RespawnCoroutine()
    {
        StartCoroutine(RespawnSystem());
    }
    private IEnumerator RespawnSystem()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 0f;
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        _characterController.enabled = false;
        _velocity = Vector3.zero;
        _verticalVelocity = 0f;
        _characterController.enabled = true;
        Friendly_Robot[] friendly_Robots = FindObjectsByType<Friendly_Robot>(FindObjectsSortMode.None);
        for (int k = 0; k < friendly_Robots.Length; k++)
        {
            friendly_Robots[k].ResetRobot();
        }
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
    }
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PickUpPerformed -= OnInteractPerformed;
        }
    }
}