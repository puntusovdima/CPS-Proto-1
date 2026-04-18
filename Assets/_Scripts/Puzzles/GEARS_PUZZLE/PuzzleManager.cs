using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum PuzzleType
{
    Gears,
    Pulley
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    // Event for individual puzzles: PuzzleManager.OnPuzzleComplete += MyMethod;
    public static event Action<PuzzleType> OnPuzzleComplete;
    
    // Event for overall completion: PuzzleManager.OnAllPuzzlesComplete += MyVictoryMethod;
    public static event Action OnAllPuzzlesComplete;

    [SerializeField] private Friendly_Robot friendlyRobotInstance;
    public Friendly_Robot FriendlyRobotInstance => friendlyRobotInstance;

    [Header("PUZZLE GOALS (SCALING)")]
    [Tooltip("How many gear chains need to be completed?")]
    [SerializeField] private int requiredGearsSolved = 1;
    [Tooltip("How many pulley systems need to be balanced?")]
    [SerializeField] private int requiredPulleysSolved = 1;

    private int currentGearsSolved = 0;
    private int currentPulleysSolved = 0;

    [Header("MOTOR SPEED")]
    [SerializeField] private float motorSpeed = 50f;

    [Header("PUZZLE GOAL (GEARS)")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float gearTolerance = 500f;
    [SerializeField] private bool doorPuzzle = false;
    public bool DoorPuzzle => doorPuzzle;

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float testDuration = 1.5f; 

    [Header("MASS PUZZLE SETTINGS (PULLEY)")]
    [SerializeField] private AtwoodManager atwoodManager;
    [SerializeField] private bool requireBalance = true;
    [SerializeField] private float massTolerance = 0.05f;

    [SerializeField] private bool isOverallSolved = false;
    private PuzzleInteractLogic puzzleLogic;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        puzzleLogic = GetComponent<PuzzleInteractLogic>();
        if (puzzleLogic == null)
            puzzleLogic = GetComponentInChildren<PuzzleInteractLogic>();
        if (puzzleLogic == null)
            puzzleLogic = FindObjectOfType<PuzzleInteractLogic>();

        ChainGear[] allGears = FindObjectsOfType<ChainGear>();
        List<ChainGear> motors = new List<ChainGear>();

        foreach (var gear in allGears)
        {
            if (gear != null && gear.isMotor) motors.Add(gear);
        }

        if (motorGear == null && motors.Count > 0)
            motorGear = motors[0];

        if (finalGear == null)
        {
            if (motors.Count > 1)
                finalGear = (motors[0] == motorGear) ? motors[1] : motors[0];
            else
            {
                foreach (var gear in allGears)
                {
                    if (gear != null && gear != motorGear && (gear.name.ToLower().Contains("final") || gear.name.ToLower().Contains("goal")))
                    {
                        finalGear = gear;
                        break;
                    }
                }
            }
        }
    }

    public void InitializePuzzle()
    {
        currentGearsSolved = 0;
        currentPulleysSolved = 0;
        isOverallSolved = false;
    }

    public bool IsPuzzleSolved() => isOverallSolved;

    private void Update()
    {
        // Continuously check Pulley balance if it's not solved yet
        if (atwoodManager != null && currentPulleysSolved < requiredPulleysSolved)
        {
            CheckPulleyPuzzleCompletion();
        }
    }

    private void CheckPulleyPuzzleCompletion()
    {
        if (atwoodManager == null || currentPulleysSolved >= requiredPulleysSolved) return;

        float leftM = atwoodManager.leftMass != null ? atwoodManager.leftMass.mass : 0f;
        float rightM = atwoodManager.rightMass != null ? atwoodManager.rightMass.mass : 0f;

        // Only log if the masses are non-zero to avoid spamming at the start of the scene
        if (leftM > 0.01f || rightM > 0.01f)
        {
            // We use a periodic log or only log on significant changes if needed, 
            // but for now let's just ensure we have the check logic.
        }

        if (requireBalance && Mathf.Abs(leftM - rightM) < massTolerance && leftM > 0.01f)
            CompletePulleyPuzzle();
    }

    #region GEARS LOGIC

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        if (currentGearsSolved >= requiredGearsSolved) return;

        ChainGear placedChainGear = gear.GetChainGear();

        if (slot.isPossibleFinalSlot)
            StartCoroutine(PerformGearTestSync(placedChainGear));
    }

    public void OnGearRemoved(GearSlotPuzzle slot, GearDragSystem gear)
    {
        // Safety: If the puzzle is already solved, we usually don't care, 
        // but if we have multiple gear puzzles, we might want to handle disconnection
        if (slot.isPossibleFinalSlot && finalGear != null)
        {
            ChainGear removedChainGear = gear.GetChainGear();
            if (removedChainGear != null)
            {
                removedChainGear.UnregisterFollower(finalGear);
                if (finalGear.inputGear == removedChainGear)
                    finalGear.inputGear = null;
            }
        }
    }

    private IEnumerator PerformGearTestSync(ChainGear placedGear)
    {
        bool wasMotorOn = false;
        float originalSpeed = 0f;

        if (motorGear != null)
        {
            wasMotorOn = motorGear.isMotor;
            originalSpeed = motorGear.motorSpeed;
            motorGear.isMotor = true;
        }

        yield return new WaitForSeconds(testDuration);

        CheckGearPuzzleCompletion(placedGear);

        // If this specific check didn't result in a solve, reset motor
        if (currentGearsSolved < requiredGearsSolved && motorGear != null)
        {
            motorGear.isMotor = wasMotorOn;
            motorGear.motorSpeed = originalSpeed;
        }
    }

    private void CheckGearPuzzleCompletion(ChainGear placedGear)
    {
        if (finalGear == null || placedGear == null) return;

        float placedTeethVelocity = placedGear.currentSpeed * placedGear.teeth;
        float finalTeethVelocity = finalGear.currentSpeed * finalGear.teeth;

        float diff = Mathf.Abs(Mathf.Abs(placedTeethVelocity) - Mathf.Abs(finalTeethVelocity));
        bool oppositeDirection = (placedTeethVelocity * finalTeethVelocity) < 0;

        if (Mathf.Abs(placedGear.currentSpeed) < 0.1f) return;

        if (diff < gearTolerance && oppositeDirection)
            CompleteGearsPuzzle(placedGear);
    }

    private void CompleteGearsPuzzle(ChainGear lastGearInChain)
    {
        currentGearsSolved++;
        if (finalGear != null && lastGearInChain != null)
        {
            finalGear.inputGear = lastGearInChain;
            lastGearInChain.RegisterFollower(finalGear);
            finalGear.isMotor = false;
        }

        if (motorGear != null) motorGear.isMotor = true;

        Debug.Log("[CompleteGearsPuzzle] friendlyRobotInstance=" + (friendlyRobotInstance != null ? "OK" : "NULL"));
        if (friendlyRobotInstance != null && !doorPuzzle)
            friendlyRobotInstance.FriendlyModeActivation();

        isOverallSolved = true;
        puzzleLogic?.ClosePuzzle();
    }

    #endregion

    #region PULLEY / MASS LOGIC

    public void OnMassPlaced(MassSlotPuzzle slot, MassDragSystem mass)
    {
        if (currentPulleysSolved >= requiredPulleysSolved) return;

        if (slot.isPossibleFinalSlot)
        {
            StartCoroutine(PerformPulleyTestSync());
        }
    }

    private IEnumerator PerformPulleyTestSync()
    {
        if (atwoodManager == null) yield break;
        yield return new WaitForSeconds(testDuration);
        CheckPulleyPuzzleCompletion();
    }

    private void CompletePulleyPuzzle()
    {
        currentPulleysSolved++;
        TriggerSinglePuzzleCompletion(PuzzleType.Pulley);
    }

    #endregion

    private void TriggerPuzzleCompletion(PuzzleType type)
    {
        OnPuzzleComplete?.Invoke(type);
        CheckOverallCompletion();
    }

    private void TriggerSinglePuzzleCompletion(PuzzleType type)
    {
        OnPuzzleComplete?.Invoke(type);
        
        if (puzzleLogic != null)
            puzzleLogic.ClosePuzzle();
    }

    private void CheckOverallCompletion()
    {
        if (currentGearsSolved >= requiredGearsSolved && currentPulleysSolved >= requiredPulleysSolved)
        {
            isOverallSolved = true;
            OnAllPuzzlesComplete?.Invoke();

            if (friendlyRobotInstance != null && !doorPuzzle)
                friendlyRobotInstance.FriendlyModeActivation();

            Invoke("ClosePuzzleAfterDelay", 2.0f);
        }
    }

    private void CheckOverallCompletion(bool skipLogic) // Overload to handle potential legacy calls if any
    {
        CheckOverallCompletion();
    }

    private void ClosePuzzleAfterDelay()
    {
        if (puzzleLogic != null) puzzleLogic.ClosePuzzle();
    }
}
