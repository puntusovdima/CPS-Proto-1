using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    [SerializeField] private Friendly_Robot friendlyRobotInstance;

    [Header("GEARS SETTINGS")]
    [SerializeField] private GearSlotPuzzle[] gSlots;
    [SerializeField] private ChainGear motorGear;
    [SerializeField] private float motorSpeed = 50f;

    private int gearsPlaced = 0;
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

    private void Update()
    {
        if (gearsPlaced == gSlots.Length && motorGear != null && motorGear.isMotor)
        {
            motorGear.motorSpeed = motorSpeed;
        }
    }

    public void OnGearPlaced(GearSlotPuzzle slot, GearDragSystem gear)
    {
        gearsPlaced++;
        if (gearsPlaced == gSlots.Length)
        {
            CompletePuzzle();
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
