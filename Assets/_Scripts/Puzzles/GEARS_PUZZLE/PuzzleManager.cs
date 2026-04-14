using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    [SerializeField] private Friendly_Robot friendlyRobotInstance;

    [Header("GEAR SLOTS")]
    [SerializeField] private GearSlotPuzzle[] gSlots;
    private int gearsPlaced = 0;
    [Header("MOTOR SPEED")]
    [SerializeField] private float motorSpeed = 50f;
    private float velocityDiff = 0f;

    [Header("PUZZLE GOAL")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float tolerance = 0.5f;

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float testDuration = 0.5f;

    [Header("MASS PUZZLE SETTINGS")]
    [SerializeField] private AtwoodManager atwoodManager;
    [SerializeField] private float requiredLeftMass = -1f;
    [SerializeField] private float requiredRightMass = -1f;
    [SerializeField] private bool requireBalance = false;

    [SerializeField] private bool isPuzzleSolved = false;
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
            Debug.Log("[PuzzleManager] Target slot detected. Testing chain connection...");
            StartCoroutine(PerformTestSync(gear.GetChainGear()));
        }
    }

    private IEnumerator PerformTestSync(ChainGear placedGear)
    {
        if (motorGear == null) { Debug.LogError("[PuzzleManager] Motor Gear is not assigned!"); yield break; }  

        // Temporarily turn ON the motor to rotate the chain
        bool wasMotorOn = motorGear.isMotor;
        float originalSpeed = motorGear.motorSpeed;

        motorGear.isMotor = true;

        // Wait for some frames to let the gears start moving
        yield return new WaitForSeconds(testDuration);

        CheckPuzzleCompletion(placedGear);

        // If the puzzle is not solved, we turn the motor OFF again
        if (!isPuzzleSolved)
        {
            motorGear.isMotor = wasMotorOn;
            motorGear.motorSpeed = originalSpeed;
            Debug.Log("[PuzzleManager] Test pulse finished - Puzzle NOT solved.");
        }
        else
        {
            // If it is solved, we keep it spinning!
            motorGear.isMotor = true;
            Debug.Log("[PuzzleManager] Test pulse finished - Puzzle SOLVED!");
        }
    }

    private void CheckPuzzleCompletion(ChainGear placedGear)
    {
        if (finalGear == null) { Debug.LogError("[PuzzleManager] Final Gear is not assigned!"); return; }       
        if (placedGear == null) { Debug.LogError("[PuzzleManager] Placed Gear component is null!"); return; }   
        if (isPuzzleSolved) return;

        float placedTeethVelocity = placedGear.currentSpeed * placedGear.teeth;
        float finalTeethVelocity = finalGear.currentSpeed * finalGear.teeth;

        Debug.Log($"[PuzzleManager] SYNC CHECK:\n" +
                  $" - Placed Gear: Speed={placedGear.currentSpeed:F2}, Teeth={placedGear.teeth}, Velocity={placedTeethVelocity:F2}\n" +
                  $" - Final Gear: Speed={finalGear.currentSpeed:F2}, Teeth={finalGear.teeth}, Velocity={finalTeethVelocity:F2}");

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
            Debug.LogWarning($"[PuzzleManager] Sync Failed: Velocity mismatch. Diff: {velocityDiff:F2} (Tolerance: {tolerance})");
        }
    }

    public void OnMassPlaced(MassSlotPuzzle slot, MassDragSystem mass)
    {
        Debug.Log($"[PuzzleManager] Mass placed in {slot.name}. IsPossibleFinal: {slot.isPossibleFinalSlot}");

        if (slot.isPossibleFinalSlot && !isPuzzleSolved)
        {
            Debug.Log("[PuzzleManager] Target mass slot detected. Testing mass connection...");
            StartCoroutine(PerformMassTestSync());
        }
    }

    private IEnumerator PerformMassTestSync()
    {
        if (atwoodManager == null) { Debug.LogError("[PuzzleManager] Atwood Manager is not assigned!"); yield break; }

        // Wait a bit to let the system stabilize if needed
        yield return new WaitForSeconds(testDuration);

        CheckMassPuzzleCompletion();
    }

    private void CheckMassPuzzleCompletion()
    {
        if (isPuzzleSolved) return;
        if (atwoodManager == null) return;

        float leftM = atwoodManager.leftMass != null ? atwoodManager.leftMass.mass : 0f;
        float rightM = atwoodManager.rightMass != null ? atwoodManager.rightMass.mass : 0f;

        bool isCorrect = true;

        if (requireBalance)
        {
            if (Mathf.Abs(leftM - rightM) > 0.01f) isCorrect = false;
        }
        else
        {
            if (requiredLeftMass >= 0 && Mathf.Abs(leftM - requiredLeftMass) > 0.01f) isCorrect = false;
            if (requiredRightMass >= 0 && Mathf.Abs(rightM - requiredRightMass) > 0.01f) isCorrect = false;
        }

        if (isCorrect)
        {
            CompletePuzzle();
        }
        else
        {
            Debug.LogWarning($"[PuzzleManager] Mass Sync Failed. Left: {leftM}, Right: {rightM}");
        }
    }

    private void CompletePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("¡Puzzle completado! El engranaje final está sincronizado.");

        if (friendlyRobotInstance != null)
        {
            friendlyRobotInstance.FriendlyModeActivation();
        }

        Invoke("ClosePuzzleAfterDelay", 2.0f);
    }

    private void ClosePuzzleAfterDelay()
    {
        if (puzzleLogic != null)
        {
            puzzleLogic.ClosePuzzle();
        }
    }
}
