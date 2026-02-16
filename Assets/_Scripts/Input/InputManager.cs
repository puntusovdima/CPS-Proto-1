using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public Action PausePerformed, UnPausePerformed,JumpPerformed, PickUpPerformed,RunPerformed,RunCanceled;
    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
            _inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        if(_inputActions == null){
            return;
        }
        _inputActions.Player.Crouch.performed += OnCrouchPerformed;
        _inputActions.Player.Crouch.canceled += OnCrouchCanceled;
        _inputActions.Player.Sprint.performed += OnRunPerformed;
        _inputActions.Player.Sprint.canceled += OnRunCanceled;
        _inputActions.Player.Jump.performed += OnJumpPerformed;
        _inputActions.Player.Interact.performed += OnInteractPerformed;
        _inputActions.UI.Cancel.performed += OnUnPausePerformed;
        _inputActions.UI.Cancel.canceled += OnPausePerformed;
        
        EnablePlayerInputs();
    }

    private void OnDisable()
    {
        if(_inputActions == null){
            return;
        }

        DisableAllInputs();
        _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
        _inputActions.Player.Crouch.canceled -= OnCrouchCanceled;
        _inputActions.Player.Sprint.performed -= OnRunPerformed;
        _inputActions.Player.Sprint.canceled -= OnRunCanceled;
        _inputActions.Player.Jump.performed -= OnJumpPerformed;
        _inputActions.Player.Interact.performed -= OnInteractPerformed;
        _inputActions.UI.Cancel.performed -= OnUnPausePerformed;
        _inputActions.UI.Cancel.canceled -= OnPausePerformed;
    }
    private void OnRunPerformed(InputAction.CallbackContext ctx)
    {
        RunPerformed?.Invoke();
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        RunCanceled?.Invoke();
    }
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        PausePerformed?.Invoke();
        EnablePlayerInputs();
    }
    private void OnUnPausePerformed(InputAction.CallbackContext ctx)
    {
        UnPausePerformed?.Invoke();
        EnableUIInputs();
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx){
        JumpPerformed?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx){
        Debug.Log("[INPUT] E presionado - PickUpPerformed");

        PickUpPerformed?.Invoke();
    }

    public void EnablePlayerInputs()
    {
        DisableAllInputs();
        _inputActions.Player.Enable();
    }
    public void EnableUIInputs()
    {
        DisableAllInputs();
        _inputActions.UI.Enable();
    }
    public void DisableAllInputs()
    {
        _inputActions.Disable();
    }

    public void DisablePuzzleInputs()
    {
        _inputActions.Player.Move.Disable();
        _inputActions.Player.Jump.Disable();
        _inputActions.Player.Sprint.Disable();
        _inputActions.Player.Crouch.Disable();
    }
    
    public void EnablePuzzleInputs()
    {
        _inputActions.Player.Enable();
    }

    // Movement.
    public Vector2 GetMovementInput()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>();
    }

    public void EnablePlayerInputActions()
    {
        if (_inputActions == null)
            return;

        DisableAllInputActions();
        _inputActions.Player.Enable();
    }

    public void EnableUiInputActions()
    {
        if (_inputActions == null)
            return;

        DisableAllInputActions();
        _inputActions.UI.Enable();
    }
    
    private void DisableAllInputActions()
    {
        if (_inputActions == null)
            return;

        _inputActions.Player.Disable();
        _inputActions.UI.Disable();
    }
    public Action CrouchPerformed, CrouchOnCanceled;
    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        CrouchPerformed?.Invoke();
    }
    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        CrouchOnCanceled?.Invoke();
    }
}