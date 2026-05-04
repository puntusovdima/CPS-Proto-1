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


    [Header("MOTOR SPEED")]
    [SerializeField] private float motorSpeed = 50f;

    [Header("PUZZLE GOAL (GEARS)")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float gearTolerance = 500f;

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float testDuration = 1.5f; 

    [Header("MASS PUZZLE SETTINGS (PULLEY)")]
    [SerializeField] private AtwoodManager[] atwoodManagers;
    [SerializeField] private float massTolerance = 0.05f;

    [Header("REFERENCES")]
    [SerializeField] private PuzzleInteractLogic puzzleLogicRef;
    [SerializeField] private GameObject door2ToRemove;
    [SerializeField] private Transform puzzleRoot;
    [SerializeField] private bool isOverallSolved = false;
    [SerializeField] private AudioClip completionSfx;
    
    private int _currentGearsSolved = 0;
    private int _currentPulleysSolved = 0;
    private bool _requireBalance = true;
    private PuzzleInteractLogic _puzzleLogic;

    private void Awake()
    {
        Instance = this;
        _requireBalance = true;
    }

    private void Start()
    {
        if (puzzleLogicRef != null)
            _puzzleLogic = puzzleLogicRef;
        else
        {
            _puzzleLogic = GetComponent<PuzzleInteractLogic>();
            if (_puzzleLogic == null)
                _puzzleLogic = GetComponentInParent<PuzzleInteractLogic>();
        }

        if (atwoodManagers == null || atwoodManagers.Length == 0)
        {
            atwoodManagers = FindObjectsOfType<AtwoodManager>();
        }

        ChainGear[] allGears;
        if (puzzleRoot != null)
            allGears = puzzleRoot.GetComponentsInChildren<ChainGear>();
        else
            allGears = FindObjectsOfType<ChainGear>();

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
        _currentGearsSolved = 0;
        _currentPulleysSolved = 0;
        isOverallSolved = false;
    }

    public bool IsPuzzleSolved() => isOverallSolved;

    private void Update()
    {
        // Continuously check Pulley balance if it's not solved yet
        if (atwoodManagers != null && atwoodManagers.Length > 0 && _currentPulleysSolved < requiredPulleysSolved)
        {
            CheckPulleyPuzzleCompletion();
        }
    }

    private void CheckPulleyPuzzleCompletion()
    {
        if (atwoodManagers == null || atwoodManagers.Length == 0 || _currentPulleysSolved >= requiredPulleysSolved) return;

        var balancedCount = 0;
        var totalPulleys = atwoodManagers.Length;

        foreach (var am in atwoodManagers)
        {
            if (am == null) continue;

            var leftM = am.leftMass != null ? am.leftMass.mass : 0f;
            var rightM = am.rightMass != null ? am.rightMass.mass : 0f;
            var diff = Mathf.Abs(leftM - rightM);

            // Debug logs removed

            if (diff < massTolerance && leftM > 0.01f)
            {
                balancedCount++;

            }
        }

        if (balancedCount == totalPulleys && totalPulleys > 0)
        {
            CompletePulleyPuzzle();
        }
    }

    #region GEARS LOGIC

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        if (_currentGearsSolved >= requiredGearsSolved) return;

        // Check if any possible final slot is occupied to trigger the test
        GearSlotPuzzle[] allSlots = FindObjectsOfType<GearSlotPuzzle>();
        GearSlotPuzzle occupiedFinalSlot = null;
        foreach (var s in allSlots)
        {
            if (s.isPossibleFinalSlot && s.IsOccupied())
            {
                occupiedFinalSlot = s;
                break;
            }
        }

        if (occupiedFinalSlot != null)
        {
            StartCoroutine(PerformGearTestSync(occupiedFinalSlot.myChainGear));
        }
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
        if (_currentGearsSolved < requiredGearsSolved && motorGear != null)
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
        _currentGearsSolved++;
        if (finalGear != null && lastGearInChain != null)
        {
            finalGear.inputGear = lastGearInChain;
            lastGearInChain.RegisterFollower(finalGear);
            finalGear.isMotor = false;
        }

        if (motorGear != null) motorGear.isMotor = true;

        Debug.Log($"[CompleteGearsPuzzle] RESUELTO! puzzleLogic={(_puzzleLogic != null ? "OK" : "NULL")}");

        isOverallSolved = true;
        _puzzleLogic?.ClosePuzzle();
    }

    #endregion

    #region PULLEY / MASS LOGIC

    public void OnMassPlaced(MassSlotPuzzle slot, MassDragSystem mass)
    {
        if (_currentPulleysSolved >= requiredPulleysSolved) return;

        // Check balance regardless of which slot was used
        StartCoroutine(PerformPulleyTestSync());
    }

    public void OnMassRemoved(MassSlotPuzzle slot, MassDragSystem mass)
    {
        if (_currentPulleysSolved >= requiredPulleysSolved) return;

        // Recalculate balance when mass is removed as well
        StartCoroutine(PerformPulleyTestSync());
    }

    private IEnumerator PerformPulleyTestSync()
    {
        if (atwoodManagers == null || atwoodManagers.Length == 0) yield break;
        
        // Wait a small bit for physics/mass regulators to settle
        yield return new WaitForSeconds(0.1f);
        
        CheckPulleyPuzzleCompletion();
    }

    private void CompletePulleyPuzzle()
    {
        _currentPulleysSolved++;
        Debug.Log($"[Pulley Puzzle RESUELTO] currentPulleysSolved: {_currentPulleysSolved}/{requiredPulleysSolved} - puzzleLogic={(_puzzleLogic != null ? "OK" : "NULL")}");

        if (door2ToRemove != null)
            Destroy(door2ToRemove);

        TriggerSinglePuzzleCompletion(PuzzleType.Pulley);
    }

    #endregion

    public void PlayCompletionSound()
    {
        SoundManager.Instance.PlaySound(completionSfx);
    }
    
    private void TriggerPuzzleCompletion(PuzzleType type)
    {
        OnPuzzleComplete?.Invoke(type);
        CheckOverallCompletion();
    }

    private void TriggerSinglePuzzleCompletion(PuzzleType type)
    {
        OnPuzzleComplete?.Invoke(type);
        
        if (_puzzleLogic != null)
            _puzzleLogic.ClosePuzzle();
    }

    private void CheckOverallCompletion()
    {
        if (_currentGearsSolved >= requiredGearsSolved && _currentPulleysSolved >= requiredPulleysSolved)
        {
            isOverallSolved = true;
            OnAllPuzzlesComplete?.Invoke();

            Invoke("ClosePuzzleAfterDelay", 2.0f);
        }
    }

    private void CheckOverallCompletion(bool skipLogic) // Overload to handle potential legacy calls if any
    {
        CheckOverallCompletion();
    }

    private void ClosePuzzleAfterDelay()
    {
        if (_puzzleLogic != null) _puzzleLogic.ClosePuzzle();
    }
    
    public void ActivateFriendlyRobot()
    {
        if (friendlyRobotInstance != null) friendlyRobotInstance.FriendlyModeActivation();
    }
}
