using UnityEngine;

public class PuzzleInteractLogic : MonoBehaviour
{
    [Header("CAMERA SETTINGS")]
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private Camera mainCamera;

    [Header("PUZZLE SETTINGS")]
    [SerializeField] private PuzzleManager puzzleManager;
    private bool isPuzzleActive = false;

    private void Start()
    {
        puzzleCamera.enabled = false;
        if (puzzleManager == null)
            puzzleManager = GetComponent<PuzzleManager>();
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
        if (isPuzzleActive)
            return;
        
        isPuzzleActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (puzzleCamera != null)
            puzzleCamera.enabled = true;
        
        InputManager.Instance.DisablePuzzleInputs();
        
        if (puzzleManager != null)
            puzzleManager.InitializePuzzle();
    }

    public void ClosePuzzle()
    {
        if (!isPuzzleActive)
            return;
            
        isPuzzleActive = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;
        if (mainCamera != null)
            mainCamera.enabled = true;
        
        InputManager.Instance.EnablePuzzleInputs();
    }
}