using UnityEngine;

using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("PUZZLE GOAL")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float tolerance = 0.5f;

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;

    private bool isPuzzleSolved = false;
    private PuzzleInteractLogic puzzleLogic;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        puzzleLogic = GetComponent<PuzzleInteractLogic>();
    }

    public void InitializePuzzle()
    {
        isPuzzleSolved = false;
        Debug.Log("Puzzle iniciado. Conecta el engranaje final.");
    }

    public bool IsPuzzleSolved() => isPuzzleSolved;

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        Debug.Log($"[PuzzleManager] Gear placed in {slot.name}. IsPossibleFinal: {slot.isPossibleFinalSlot}");

        if (slot.isPossibleFinalSlot && !isPuzzleSolved)
        {
            Debug.Log("[PuzzleManager] Target slot detected. Starting sync check...");
            StartCoroutine(CheckSyncAfterFrame(gear.GetChainGear()));
        }
    }

    private IEnumerator CheckSyncAfterFrame(ChainGear placedGear)
    {
        yield return null; 
        CheckPuzzleCompletion(placedGear);
    }

    private void CheckPuzzleCompletion(ChainGear placedGear)
    {
        if (finalGear == null) { Debug.LogError("[PuzzleManager] Final Gear is not assigned!"); return; }
        if (placedGear == null) { Debug.LogError("[PuzzleManager] Placed Gear component is null!"); return; }
        if (isPuzzleSolved) return;

        float placedTeethVelocity = placedGear.currentSpeed * placedGear.teeth;
        float finalTeethVelocity = finalGear.currentSpeed * finalGear.teeth;

        Debug.Log($"[PuzzleManager] SYNC CHECK:\n" +
                  $" - Placed Gear: Speed={placedGear.currentSpeed}, Teeth={placedGear.teeth}, Velocity={placedTeethVelocity}\n" +
                  $" - Final Gear: Speed={finalGear.currentSpeed}, Teeth={finalGear.teeth}, Velocity={finalTeethVelocity}");

        if (Mathf.Abs(placedGear.currentSpeed) < 0.1f)
        {
            Debug.LogWarning("[PuzzleManager] Placed gear is NOT rotating. Check your chain connection!");
            return;
        }

        bool oppositeDirection = (placedTeethVelocity * finalTeethVelocity) < 0;
        float velocityDiff = Mathf.Abs(Mathf.Abs(placedTeethVelocity) - Mathf.Abs(finalTeethVelocity));

        if (!oppositeDirection)
        {
            Debug.LogWarning($"[PuzzleManager] Sync Failed: Wrong direction. Both are rotating same way.");
            return;
        }

        if (velocityDiff < tolerance)
        {
            CompletePuzzle();
        }
        else
        {
            Debug.LogWarning($"[PuzzleManager] Sync Failed: Velocity mismatch. Diff: {velocityDiff} (Tolerance: {tolerance})");
        }
    }

    private void CompletePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("¡Puzzle completado! El engranaje final está sincronizado.");
        
        Invoke("ClosePuzzleAfterDelay", 3f);
    }

    private void ClosePuzzleAfterDelay()
    {
        if (puzzleLogic != null)
        {
            puzzleLogic.ClosePuzzle();
        }
    }
}
