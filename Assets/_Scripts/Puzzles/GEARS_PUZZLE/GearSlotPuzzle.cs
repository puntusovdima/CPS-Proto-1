using UnityEngine;

public class GearSlotPuzzle : MonoBehaviour
{
    [Header("GEARS SLOTS SETTINGS")]
    [SerializeField] private float snapDistance = 0.5f;
    [SerializeField] private float snapSpeed = 5f;
    [SerializeField] public bool isPossibleFinalSlot = false;
    [SerializeField] private ChainGear inputGearForThisSlot;
    [SerializeField] private GearSlotPuzzle inputSlotForThisSlot;
    public ChainGear myChainGear;
    [SerializeField] private int requiredTeeth = 10;
    private GearDragSystem placedGear;
    private bool isOccupied = false;
    [SerializeField] private Transform slotCenter;

    [Header("TIMED PUZZLE")]
    [SerializeField] private bool useTimedPuzzle = false;
    [SerializeField] private GearPuzzleWhitTime timedPuzzle;

    private void Awake()
    {
        if (slotCenter == null && transform.childCount > 0)
            slotCenter = transform.GetChild(0);
    }

    private void OnTriggerStay(Collider other)
    {
        GearDragSystem gear = other.GetComponent<GearDragSystem>();

        if (gear == null)
            gear = other.GetComponentInParent<GearDragSystem>();

        if (gear != null && !gear.IsDragging() && !isOccupied)
        {
            ChainGear chainGear = gear.GetChainGear();
            if (chainGear == null)
            {
                Debug.LogWarning($"[GearSlotPuzzle] ChainGear component not found on {gear.name}!");
                return;
            }

            if (chainGear.teeth != requiredTeeth)
            {
                return;
            }

            gear.transform.position = Vector3.Lerp(gear.transform.position, transform.position, Time.deltaTime * snapSpeed);
            if (Vector3.Distance(gear.transform.position, transform.position) < snapDistance)
            {
                PlaceGear(gear);
            }
        }
    }

    private void PlaceGear(GearDragSystem gear)
    {
        myChainGear = gear.GetChainGear();

        if (myChainGear == null)
        {
            return;
        }

        if (myChainGear.teeth != requiredTeeth)
        {
            Debug.Log("Ese gear no tiene los dientes correspondientes");
            return;
        }

        placedGear = gear;
        isOccupied = true;
        gear.transform.position = slotCenter.transform.position;
        gear.SetCurrentSlot(this);

        RefreshAllSlots();

        Debug.Log($"Gear colocado correctamente en slot {gameObject.name}");

        if (useTimedPuzzle && timedPuzzle != null)
        {
            Debug.Log($"[GearSlotPuzzle] Llamando a timedPuzzle.OnGearPlaced!");
            timedPuzzle.OnGearPlaced(this, gear);
        }
        else if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnGearPlaced(this, gear);
        }
    }

    public bool IsOccupied() => isOccupied;

    public void RemoveGear()
    {
        if (placedGear != null)
        {
            if (useTimedPuzzle && timedPuzzle != null)
            {
                timedPuzzle.OnGearRemoved(this, placedGear);
            }
            else if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.OnGearRemoved(this, placedGear);
            }

            placedGear.SetCurrentSlot(null);
        }

        placedGear = null;
        myChainGear = null;
        isOccupied = false;

        RefreshAllSlots();
        Debug.Log($"Slot {gameObject.name} cleared and ready for new gear.");
    }

    private void RefreshAllSlots()
    {
        GearSlotPuzzle[] allSlots = FindObjectsOfType<GearSlotPuzzle>();
        foreach (var slot in allSlots)
        {
            slot.UpdateChainConnection();
        }
    }

    public void UpdateChainConnection()
    {
        if (!isOccupied || myChainGear == null) return;

        ChainGear desiredInput = null;
        if (inputGearForThisSlot != null)
        {
            desiredInput = inputGearForThisSlot;
        }
        else if (inputSlotForThisSlot != null && inputSlotForThisSlot.IsOccupied())
        {
            desiredInput = inputSlotForThisSlot.myChainGear;
        }

        if (myChainGear.inputGear != null && myChainGear.inputGear != desiredInput)
        {
            myChainGear.inputGear.UnregisterFollower(myChainGear);
            myChainGear.inputGear = null;
        }

        if (desiredInput != null && myChainGear.inputGear == null)
        {
            myChainGear.inputGear = desiredInput;
            desiredInput.RegisterFollower(myChainGear);
        }
    }
}
