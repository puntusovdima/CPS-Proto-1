using UnityEngine;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    [SerializeField] private Friendly_Robot friendlyRobotInstance;

    [Header("GEAR SLOTS")]
    // [SerializeField] private GearSlotPuzzle[] gSlots;
    private int gearsPlaced = 0;
    [Header("MOTOR SPEED")]
    [SerializeField] private float motorSpeed = 50f;
    private float velocityDiff = 0f;

    [Header("PUZZLE GOAL")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float tolerance = 500f; // Increased significantly to handle frame-rate jitter

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float testDuration = 1.5f; // Increased for stability

    [Header("MASS PUZZLE SETTINGS")]
    [SerializeField] private AtwoodManager atwoodManager;
    [SerializeField] private float requiredLeftMass = -1f;
    [SerializeField] private float requiredRightMass = -1f;
    [SerializeField] private bool requireBalance = false;

    private bool isPuzzleSolved = false;
    private PuzzleInteractLogic puzzleLogic;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        puzzleLogic = GetComponent<PuzzleInteractLogic>();

        // Fallback: Auto-assign motor gears if missing
        ChainGear[] allGears = FindObjectsOfType<ChainGear>();
        System.Collections.Generic.List<ChainGear> motors = new System.Collections.Generic.List<ChainGear>();
        
        foreach (var gear in allGears)
        {
            if (gear != null && gear.isMotor) motors.Add(gear);
        }

        if (motorGear == null && motors.Count > 0)
        {
            motorGear = motors[0];
            Debug.Log($"[PuzzleManager] Motor Gear auto-assigned: {motorGear.name}");
        }

        if (finalGear == null)
        {
            // If we have a second motor, it's likely the final gear
            if (motors.Count > 1)
            {
                finalGear = (motors[0] == motorGear) ? motors[1] : motors[0];
                Debug.Log($"[PuzzleManager] Final Gear auto-assigned from motors: {finalGear.name}");
            }
            // Otherwise, look for any gear named "Final" or similar, or just any gear that isn't the motor
            else
            {
                foreach (var gear in allGears)
                {
                    if (gear != null && gear != motorGear && (gear.name.ToLower().Contains("final") || gear.name.ToLower().Contains("goal")))
                    {
                        finalGear = gear;
                        Debug.Log($"[PuzzleManager] Final Gear auto-assigned by name: {finalGear.name}");
                        break;
                    }
                }
            }
        }
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

        ChainGear placedChainGear = gear.GetChainGear();

        if (slot.isPossibleFinalSlot && !isPuzzleSolved)
        {
            // NOTE: We no longer connect the finalGear here to avoid "Double Drive" fighting.
            // We just check if the speeds match. Connection happens in CompletePuzzle.
            Debug.Log("[PuzzleManager] Target slot detected. Testing chain connection...");
            StartCoroutine(PerformTestSync(placedChainGear));
        }
    }

    public void OnGearRemoved(GearSlotPuzzle slot, GearDragSystem gear)
    {
        if (slot.isPossibleFinalSlot && finalGear != null)
        {
            ChainGear removedChainGear = gear.GetChainGear();
            if (removedChainGear != null)
            {
                removedChainGear.UnregisterFollower(finalGear);
                if (finalGear.inputGear == removedChainGear)
                {
                    finalGear.inputGear = null;
                }
                Debug.Log($"[PuzzleManager] Disconnected {finalGear.name} from removed gear {removedChainGear.name}");
            }
        }
    }

    private IEnumerator PerformTestSync(ChainGear placedGear)
    {
        bool wasMotorOn = false;
        float originalSpeed = 0f;

        if (motorGear != null)
        {
            // Temporarily turn ON the motor to rotate the chain
            wasMotorOn = motorGear.isMotor;
            originalSpeed = motorGear.motorSpeed;
            motorGear.isMotor = true;
        }

        // Wait for some frames to let the gears start moving
        yield return new WaitForSeconds(testDuration);

        CheckGearPuzzleCompletion(placedGear);

        // If the puzzle is not solved, we turn the motor OFF again
        if (!isPuzzleSolved && motorGear != null)
        {
            motorGear.isMotor = wasMotorOn;
            motorGear.motorSpeed = originalSpeed;
            Debug.Log("[PuzzleManager] Test pulse finished - Puzzle NOT solved.");
        }
    }

    private void CheckGearPuzzleCompletion(ChainGear placedGear)
    {
        if (finalGear == null) { Debug.LogError("[PuzzleManager] Final Gear is not assigned!"); return; }
        if (placedGear == null) { Debug.LogError("[PuzzleManager] Placed Gear component is null!"); return; }
        if (isPuzzleSolved) return;

        float placedTeethVelocity = placedGear.currentSpeed * placedGear.teeth;
        float finalTeethVelocity = finalGear.currentSpeed * finalGear.teeth;

        Debug.Log($"[PuzzleManager] SYNC CHECK:\n" +
                  $" - Placed Gear: Speed={placedGear.currentSpeed:F2}, Velocity={placedTeethVelocity:F2}\n" +
                  $" - Final Gear Target: Speed={finalGear.currentSpeed:F2}, Velocity={finalTeethVelocity:F2}\n" +
                  $" - Diff: {Mathf.Abs(Mathf.Abs(placedTeethVelocity) - Mathf.Abs(finalTeethVelocity)):F2} (Tolerance: {tolerance})");

        if (Mathf.Abs(placedGear.currentSpeed) < 0.1f)
        {
            Debug.LogWarning("[PuzzleManager] Placed gear is NOT rotating.");
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
            CompletePuzzle(placedGear);
        }
        else
        {
            Debug.LogWarning($"[PuzzleManager] Sync Failed: Velocity mismatch.");
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
            CompletePuzzle(null);
        }
        else
        {
            Debug.LogWarning($"[PuzzleManager] Mass Sync Failed. Left: {leftM}, Right: {rightM}");
        }
    }

    private void CompletePuzzle(ChainGear lastGearInChain)
    {
        isPuzzleSolved = true;
        Debug.Log("¡Puzzle completado! El engranaje final está sincronizado.");

        // Connect the chain permanently
        if (finalGear != null && lastGearInChain != null)
        {
            finalGear.inputGear = lastGearInChain;
            lastGearInChain.RegisterFollower(finalGear);
            
            // Turn off the goal's independent motor so the chain drives it
            finalGear.isMotor = false;
        }

        if (motorGear != null) motorGear.isMotor = true;

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
