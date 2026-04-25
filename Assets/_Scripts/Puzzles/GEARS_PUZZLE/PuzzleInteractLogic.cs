using System;
using System.Reflection;
using UnityEngine;

public class PuzzleInteractLogic : MonoBehaviour
{
    [Header("CAMERA SETTINGS")]
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private Camera mainCamera;

    [Header("PUZZLE MANAGERS")]
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private MonoBehaviour timedPuzzleManager;

    [Header("PUZZLE SETTINGS")]
    [SerializeField] private GameObject playerToHide;
    [SerializeField] private GameObject wallToRemove;
    [SerializeField] private bool isDoorPuzzle = false;

    private bool isPuzzleActive = false;

    private void Start()
    {
        puzzleCamera.enabled = false;

        if (puzzleManager == null)
            puzzleManager = GetComponent<PuzzleManager>();

        PuzzleManager.OnPuzzleComplete += OnPuzzleCompleteHandler;
    }

    private void OnDestroy()
    {
        PuzzleManager.OnPuzzleComplete -= OnPuzzleCompleteHandler;
    }

    private void OnPuzzleCompleteHandler(PuzzleType type)
    {
        switch (type)
        {
            case PuzzleType.Gears when wallToRemove != null:
                if (!isDoorPuzzle && puzzleManager != null)
                    puzzleManager.ActivateFriendlyRobot();
                Destroy(wallToRemove);
                break;
            case PuzzleType.Pulley when wallToRemove != null:
                Destroy(wallToRemove);
                break;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            InputManager.Instance.PickUpPerformed += OpenPuzzle;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            InputManager.Instance.PickUpPerformed -= OpenPuzzle;
        }
    }

    public void OpenPuzzle()
    {
        if (puzzleManager != null && puzzleManager.IsPuzzleSolved())
            return;

        if (TimedPuzzleIsSolved())
            return;

        if (isPuzzleActive) return;
        isPuzzleActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainCamera != null)
            mainCamera.enabled = false;
        else if (Camera.main != null)
            Camera.main.enabled = false;

        if (puzzleCamera != null)
            puzzleCamera.enabled = true;

        InputManager.Instance.DisablePuzzleInputs();
        PlayerController.Instance.setPause(true);

        if (puzzleManager != null)
            puzzleManager.InitializePuzzle();
        else if (timedPuzzleManager != null)
        {
            InvokeTimedPuzzleMethod("InitializePuzzle");
            InvokeTimedPuzzleMethod("StartPuzzle");
        }

        if (playerToHide != null)
            playerToHide.SetActive(false);
    }

    public void ClosePuzzle()
    {
        if (!isPuzzleActive)
            return;

        bool wasSolved = false;

        if (puzzleManager != null)
            wasSolved = puzzleManager.IsPuzzleSolved();

        if (!wasSolved)
            wasSolved = TimedPuzzleIsSolved();

        isPuzzleActive = false;

        if (puzzleCamera != null) puzzleCamera.enabled = false;

        if (mainCamera != null) mainCamera.enabled = true;
        else if (Camera.main != null) Camera.main.enabled = true;

        if (playerToHide != null) playerToHide.SetActive(true);

        if (wasSolved && wallToRemove != null)
            Destroy(wallToRemove);

        PlayerController.Instance.setPause(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputManager.Instance.EnablePuzzleInputs();
    }

    private bool TimedPuzzleIsSolved()
    {
        if (timedPuzzleManager == null)
            return false;

        object result = InvokeTimedPuzzleMethod("IsPuzzleSolved");
        return result is bool solved && solved;
    }

    private object InvokeTimedPuzzleMethod(string methodName)
    {
        if (timedPuzzleManager == null)
            return null;

        MethodInfo method = timedPuzzleManager.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return method != null ? method.Invoke(timedPuzzleManager, null) : null;
    }
}