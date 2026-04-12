using UnityEngine;

using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    [SerializeField] private Friendly_Robot friendlyRobotInstance;

    [Header("PUZZLE GOAL")]
    [SerializeField] private ChainGear finalGear;
    [SerializeField] private float tolerance = 0.5f;

    [Header("GEARS SETTINGS")]
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float testDuration = 0.5f;

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
        gearsPlaced = 0;
    }

    public bool IsPuzzleSolved() => isPuzzleSolved;

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        gearsPlaced++;
        if (gearsPlaced == gSlots.Length)
        {
            CompletePuzzle();
        }
        else
        {
            Debug.LogWarning($"[PuzzleManager] Sync Failed: Velocity mismatch. Diff: {velocityDiff:F2} (Tolerance: {tolerance})");
        }
    }

    private void CompletePuzzle()
    {
        if (motorGear != null)
        {
            motorGear.isMotor = true;
            motorGear.motorSpeed = motorSpeed;
        }
        
        Invoke("ClosePuzzleAfterDelay", 2.0f);
        friendlyRobotInstance.FriendlyModeActivation();
    }

    private void ClosePuzzleAfterDelay()
    {
        if (puzzleLogic != null)
        {
            puzzleLogic.ClosePuzzle();
        }
    }
}
