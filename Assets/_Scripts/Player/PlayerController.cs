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

    [Header("ROTATION SETTINGS")]
    [Range(0f, 10f)] [SerializeField] private float rotationSpeed = 5f;

    // RESPAWN -> only if is needed.
    /*
    [Header("RESPAWN SETTINGS")]
    [SerializeField] private Transform RP;
    [SerializeField] Animator fadeAnimation;
    */
    private Vector2 _currentInput;
    private Vector3 _velocity;
    private CharacterController _characterController;
    private Camera _camera;
    private Animator _animator;
    private float _currentSpeed;
    private float gravity = 9.81f;
    public bool screenPaused;
    private bool _isRunning = false;
    private bool _wasRunning = false;
    private bool _wasWalking = false;
    public bool _isCrouching = false;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        
        InputManager.Instance.RunPerformed += RunOnPerformed;
        InputManager.Instance.RunCanceled += RunOnCanceled;
        InputManager.Instance.CrouchPerformed += CrouchOnPerformed;
        InputManager.Instance.CrouchOnCanceled += CrouchOnCanceled;
        _currentSpeed = normalMovementSpeed;
    }

    private void Update()
    {
        UpdateMovement();
        ApplyTotalVelocity();
    }

    private void UpdateMovement()
    {

        // SOUNDS -> WALK AND RUN -> SOUNDMANAGER NEEDED FIRST.

        /*
        // PREFABS.
        bool isMoving = _currentInput.magnitude > 0.1f;
        bool playerIsWalking = !_isRunning && isMoving;
        bool playerIsRunning = _isRunning && isMoving;
        // Walk and Run sounds.
        if (SoundManager.Instance != null && SoundManager.Instance.playerSounds != null)
        {
            if (playerIsRunning && (!_wasRunning || !SoundManager.Instance.playerSounds.isPlaying))
            {
                SoundManager.Instance.playerSounds.Stop();
                SoundManager.Instance.PlayFx(AudioFX.RunSound, SoundManager.Instance.playerSounds);
            }
            else if (playerIsWalking && (!_wasWalking || !SoundManager.Instance.playerSounds.isPlaying))
            {
                SoundManager.Instance.playerSounds.Stop();
                SoundManager.Instance.PlayFx(AudioFX.WalkSound, SoundManager.Instance.playerSounds);
            }
            else if (!playerIsRunning && !playerIsWalking && SoundManager.Instance.playerSounds.isPlaying)
            {
                SoundManager.Instance.playerSounds.Stop();
            }
            _wasRunning = playerIsRunning;
            _wasWalking = playerIsWalking;
        }
        */

        if (screenPaused) {
            return;
        }
       _currentInput = InputManager.Instance.GetMovementInput();

            // Direcction.
            Vector3 forward = new Vector3(1, 0, 1).normalized;
            Vector3 right = new Vector3(1, 0, -1).normalized;

            Vector3 desiredMove = right * _currentInput.x + forward * _currentInput.y;
            desiredMove.y = 0f;

            // Rotation.
            if (desiredMove.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredMove);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // States.
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
            
            // Movement
            _velocity = desiredMove * _currentSpeed;

            if (_animator != null)
            {
                _animator.SetFloat(Speed, _currentInput.magnitude);
            }
        }
        
    public void setPause(bool p) => screenPaused = p;
    private void ApplyTotalVelocity()
    {
        _characterController.SimpleMove(_velocity);
    }
        private void RunOnPerformed()
        {
            _isRunning = true;
            _currentSpeed = runSpeed;
        }

        private void RunOnCanceled()
        {
            _isRunning = false;
            _currentSpeed = normalMovementSpeed;
        }

        private void CrouchOnPerformed()
        {
            _isCrouching = true;
            _isRunning = false;
            
            /*
            if (SoundManager.Instance != null && SoundManager.Instance.playerOneShotSounds != null)
            {
                // PlayOneshot -> ony one time, no loop.
                SoundManager.Instance.playerOneShotSounds.PlayOneShot(SoundManager.Instance.m_fxClips[(int)AudioFX.CrouchSound]);
            }
            if (_animator != null){
                _animator.SetBool(IsCrouching, true);
            }
            */
        }

        private void CrouchOnCanceled()
        {
            _isCrouching = false;
            /*
            if (_animator != null) {
                _animator.SetBool(IsCrouching, false);
            }
            */
        }
        public Transform GetTransform(out bool playerOnSight)
        {
            playerOnSight = true;
            return transform;
        }
}
