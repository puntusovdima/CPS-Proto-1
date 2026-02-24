using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

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
        Debug.Log("Puzzle iniciado. Coloca los " + gSlots.Length + " engranajes.");
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
        Debug.Log($"Engranaje colocado. {gearsPlaced}/{gSlots.Length}");

        if (gearsPlaced == gSlots.Length)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        Debug.Log("¡Puzzle completado! Los engranajes comienzan a girar...");
        
        if (motorGear != null)
        {
            motorGear.isMotor = true;
            motorGear.motorSpeed = motorSpeed;
            Debug.Log("Motor activado con velocidad: " + motorSpeed);
        }
        
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
