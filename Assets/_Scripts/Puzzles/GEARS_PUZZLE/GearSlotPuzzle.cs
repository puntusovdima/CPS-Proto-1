using UnityEngine;

public class GearSlotPuzzle : MonoBehaviour
{
    [Header("GEARS SLOTS SETTINGS")]
    [SerializeField] private float snapDistance = 0.5f;
    [SerializeField] private float snapSpeed = 5f;
    [SerializeField] private ChainGear inputGearForThisSlot;
    [SerializeField] private int requiredTeeth = 10;
    private GearDragSystem placedGear;
    private bool isOccupied = false;

    private void OnTriggerStay(Collider other)
    {
        GearDragSystem gear = other.GetComponent<GearDragSystem>();
        
        if (gear == null)
            gear = other.GetComponentInParent<GearDragSystem>();
        
        if (gear != null && !gear.IsDragging() && !isOccupied)
        {
            // Until snap -> verified.
            ChainGear chainGear = gear.GetChainGear();
            if (chainGear == null || chainGear.teeth != requiredTeeth){
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
        ChainGear chainGear = gear.GetChainGear();
        
        if (chainGear == null)
        {
            return;
        }
        
        if (chainGear.teeth != requiredTeeth)
        {
            Debug.Log("Ese gear no tieene los dientes correspondientes");
            return;
        }

        placedGear = gear;
        isOccupied = true;
        gear.transform.position = transform.position;
        gear.SetCurrentSlot(this);

        if (inputGearForThisSlot != null)
        {
            chainGear.inputGear = inputGearForThisSlot;
            inputGearForThisSlot.RegisterFollower(chainGear);
        }

        Debug.Log("Gear colocado correctamente en slot");
        
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
                chainGear.inputGear = null;
            }
            placedGear.SetCurrentSlot(null);
        }
        placedGear = null;
        isOccupied = false;
    }
}
