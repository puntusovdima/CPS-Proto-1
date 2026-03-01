using UnityEngine;
using System.Collections;
using System;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("SPEED SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float normalMovementSpeed = 5f;
    [Range(0f, 10f)] [SerializeField] private float runSpeed = 20f;

    [Header("CROUCH SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float crouchSpeed = 10f;

    [Header("JUMP SETTINGS")]
    [Range(0f, 10f)][SerializeField] private float normalJumpForce = 5f;
    [Range(0f, 10f)][SerializeField] private float runJumpForce = 8f;
    [SerializeField] private bool _playerIsGrounded = true;

    [Header("ROTATION SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float rotationSpeed = 5f;

    [Header("RESPAWN SETTINGS")]
    [SerializeField] private Transform respawnPoint;

    [Header("INTERACTION SETTINGS")]
    [SerializeField] private LayerMask puzzleG;
    [SerializeField] private LayerMask interactable;
    [SerializeField] private float playerInteractionRadius = 2f;

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
    private Animator _animator;
    private float _currentSpeed;
    private float gravity = 9.81f;
    public bool screenPaused;
    private bool _isRunning = false;
    public bool _isCrouching = false;
    private static readonly int Speed = Animator.StringToHash("Speed");
    
    private float normalHeight;
    private float crouchHeight = 0.6f;
    private Vector3 cameraNormalPos;
    private float cameraCrouchOffset = -0.5f;

    private Coroutine _footstepCoroutine;
    private const float WalkStepInterval = 0.5f;
    private const float RunStepInterval = 0.25f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        _animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;
        
        normalHeight = _characterController.height;
        if (_camera != null)
        {
            cameraNormalPos = _camera.transform.localPosition;
        }
        
        InputManager.Instance.RunPerformed += RunOnPerformed;
        InputManager.Instance.RunCanceled += RunOnCanceled;
        InputManager.Instance.CrouchPerformed += CrouchOnPerformed;
        InputManager.Instance.CrouchOnCanceled += CrouchOnCanceled;
        InputManager.Instance.JumpPerformed += JumpOnPerformed;
        InputManager.Instance.PickUpPerformed += OnInteractPerformed;

        _currentSpeed = normalMovementSpeed;

        respawnPoint = GameManager.Instance.GetCurrentCheckpointPosition();
    }

    private void Update()
    {
        UpdateMovement();
        DetectNearestPuzzle();
        UpdateVerticalVelocity();
        ApplyTotalVelocity();
        respawnPoint = GameManager.Instance.GetCurrentCheckpointPosition();
    }

    private void UpdateMovement()
    {
        if (screenPaused)
        {
            return;
        }

        _currentInput = InputManager.Instance.GetMovementInput();

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        Vector3 desiredMove = right * _currentInput.x + forward * _currentInput.y;
        desiredMove.y = 0f;

        if (desiredMove.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMove);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (_isCrouching)
        {
            _currentSpeed = crouchSpeed;
        }
        else if (_isRunning)
        {
            _currentSpeed = runSpeed;
        }
        else
        {
            _currentSpeed = normalMovementSpeed;
        }
        
        _velocity = desiredMove * _currentSpeed;

        if (_animator != null)
        {
            _animator.SetFloat(Speed, _currentInput.magnitude);
        }

        Vector3 rayOriginPoint = transform.position + Vector3.up * 0.2f;
        bool raycastHitToTheGround = Physics.Raycast(rayOriginPoint, Vector3.down, 2.0f);
        _playerIsGrounded = raycastHitToTheGround;
        
        if (_playerIsGrounded && _verticalVelocity <= 0)
        {
            _verticalVelocity = -2f;
        }

        // Footstep sounds
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
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void RunOnPerformed()
    {
        if (!_playerIsGrounded)
        {
            return;
        }

        _isRunning = true;
        _currentSpeed = runSpeed;
    }

    private void RunOnCanceled()
    {
        _isRunning = false;
        _currentSpeed = normalMovementSpeed;
    }

    private void JumpOnPerformed()
    {
        if (_playerIsGrounded)
        {
            float jumpForceA = _isRunning ? runJumpForce : normalJumpForce;
            _verticalVelocity = jumpForceA;
            _playerIsGrounded = false;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(jumpSFX);
        }
    }

    private void CrouchOnPerformed()
    {
        _isCrouching = true;
        _isRunning = false;
            
        _characterController.height = crouchHeight;
            
        if (_camera != null)
        {
            _camera.transform.localPosition = cameraNormalPos + Vector3.up * cameraCrouchOffset;
        }
    }

    private void CrouchOnCanceled()
    {
        _isCrouching = false;
            
        _characterController.height = normalHeight;
            
        if (_camera != null)
        {
            _camera.transform.localPosition = cameraNormalPos;
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

        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
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
            else
                Debug.LogError($"{nearestInteractable.name} has no IInteractable component!", nearestInteractable);
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PickUpPerformed -= OnInteractPerformed;
        }
    }
}