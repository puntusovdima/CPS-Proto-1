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
    
    private void Awake()
    {
        // Si olvidas asignarlo en el inspector, intenta buscar el primer hijo
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
                // Optionally log this if needed for debugging
                // Debug.Log($"[GearSlotPuzzle] Teeth mismatch: {chainGear.teeth} != {requiredTeeth}");
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
            Debug.Log("Ese gear no tieene los dientes correspondientes");
            return;
        }

        placedGear = gear;
        isOccupied = true;
        gear.transform.position = slotCenter.transform.position;
        gear.SetCurrentSlot(this);

        // Chain connection logic
        ChainGear inputToUse = null;
        if (inputGearForThisSlot != null)
        {
            inputToUse = inputGearForThisSlot;
        }
        else if (inputSlotForThisSlot != null)
        {
            inputToUse = inputSlotForThisSlot.myChainGear;
        }

        if (inputToUse != null && inputToUse != myChainGear) // Safety check
        {
            myChainGear.inputGear = inputToUse;
            inputToUse.RegisterFollower(myChainGear);
        }

        Debug.Log($"Gear colocado correctamente en slot {gameObject.name}");
        
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnGearPlaced(this, gear);
        }
    }

    public bool IsOccupied() => isOccupied;

    public void RemoveGear()
    {
        if (placedGear != null)
        {
            ChainGear chainGear = placedGear.GetChainGear();
            if (chainGear != null)
            {
                // IMPORTANT: Unregister from the input gear list before nulling
                if (chainGear.inputGear != null)
                {
                    chainGear.inputGear.UnregisterFollower(chainGear);
                    chainGear.inputGear = null;
                }
            }
            placedGear.SetCurrentSlot(null);
        }
        placedGear = null;
        isOccupied = false;
        myChainGear = null; // Clear local reference too
    }
}
