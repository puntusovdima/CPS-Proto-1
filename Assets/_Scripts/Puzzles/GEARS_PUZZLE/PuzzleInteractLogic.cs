using UnityEngine;

public class PuzzleInteractLogic : MonoBehaviour
{
    [Header("CAMERA SETTINGS")]
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private Camera mainCamera;

    [Header("PUZZLE SETTINGS")]
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private GameObject playerToHide;
    [SerializeField] private GameObject wallToRemove;
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
        if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleSolved())
            return;

        if (isPuzzleActive)return;
        isPuzzleActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainCamera != null)
            mainCamera.enabled = false;
        else if (Camera.main != null)
            Camera.main.enabled = false;

        if (puzzleCamera != null)
        {
            puzzleCamera.enabled = true;
        }
        
        InputManager.Instance.DisablePuzzleInputs();
        PlayerController.Instance.setPause(true);
        
        if (puzzleManager != null)
            puzzleManager.InitializePuzzle();

        if (playerToHide != null)
            playerToHide.SetActive(false);
    }

    public void ClosePuzzle()
    {
        if (!isPuzzleActive)
            return;

        bool wasSolved = PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleSolved();
        isPuzzleActive = false;

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;
        else if (Camera.main != null)
            Camera.main.enabled = true;

        if (playerToHide != null)
            playerToHide.SetActive(true);

        if (wasSolved && wallToRemove != null)
            Destroy(wallToRemove);

        PlayerController.Instance.setPause(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputManager.Instance.EnablePuzzleInputs();
    }
}