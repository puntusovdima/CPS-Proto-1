using UnityEngine;

public class MassSlotPuzzle : MonoBehaviour
{
    [Header("MASS SLOTS SETTINGS")]
    [SerializeField] private float snapDistance = 0.5f;
    [SerializeField] private float snapSpeed = 5f;
    [SerializeField] public bool isPossibleFinalSlot = false;
    
    [SerializeField] private Transform slotCenter;
    
    public MassRegulator myMassRegulator; 
    private MassDragSystem placedMass;
    private bool isOccupied = false;

    private void Awake()
    {
        // If you forget to assign it in the inspector, try to find the first child
        if (slotCenter == null && transform.childCount > 0)
            slotCenter = transform.GetChild(0);
        
        if (myMassRegulator == null)
            myMassRegulator = GetComponent<MassRegulator>();

        // Auto-register this slot to its parent MassRegulator (e.g., the Pulley) 
        // so mass changes bubble up automatically for chained Atwood managers.
        if (myMassRegulator != null && transform.parent != null)
        {
            MassRegulator parentReg = transform.parent.GetComponentInParent<MassRegulator>();
            if (parentReg != null && parentReg != myMassRegulator)
            {
                var pList = new System.Collections.Generic.List<MassRegulator>(parentReg.children ?? new MassRegulator[0]);
                if (!pList.Contains(myMassRegulator))
                {
                    pList.Add(myMassRegulator);
                    parentReg.children = pList.ToArray();
                    Debug.Log($"Auto-registered slot {gameObject.name} to parent MassRegulator {parentReg.gameObject.name}");
                }
            }
        }
    }

    private void Update()
    {
        // Periodic check to catch the mass release if OnTriggerStay fails to trigger at the exact moment.
        if (!isOccupied)
        {
            CheckForNearbyMasses();
        }
    }

    private void CheckForNearbyMasses()
    {
        // Find all MassDragSystems in the scene (small number of masses usually)
        MassDragSystem[] masses = FindObjectsByType<MassDragSystem>(FindObjectsSortMode.None);
        foreach (var mass in masses)
        {
            if (!mass.IsDragging() && Vector3.Distance(mass.transform.position, slotCenter != null ? slotCenter.position : transform.position) < snapDistance)
            {
                Debug.Log($"Update: Snap condition met for {mass.name}. Placing...");
                PlaceMass(mass);
                break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Debug.Log($"OnTriggerStay: Entering {other.name}");
        
        MassDragSystem massDrag = other.GetComponent<MassDragSystem>();
        if (massDrag == null) massDrag = other.GetComponentInParent<MassDragSystem>();
        if (massDrag == null) massDrag = other.GetComponentInChildren<MassDragSystem>();
        
        if (massDrag != null)
        {
            bool dragging = massDrag.IsDragging();
            // Debug.Log($"OnTriggerStay: massDrag found on {massDrag.name}. IsDragging: {dragging}, isOccupied: {isOccupied}");
            
            if (!isOccupied && !dragging)
            {
                Debug.Log($"OnTriggerStay: ALL CONDITIONS MET for {massDrag.name}. Placing...");
                PlaceMass(massDrag);
            }
        }
        else
        {
            Debug.Log($"OnTriggerStay: No MassDragSystem found on {other.name} or its hierarchy.");
        }
    }

    private void PlaceMass(MassDragSystem massDrag)
    {
        placedMass = massDrag;
        isOccupied = true;
        
        // Parent the mass to this slot so it moves up and down with the Atwood tray
        massDrag.transform.SetParent(slotCenter != null ? slotCenter : transform);
        
        massDrag.transform.localPosition = Vector3.zero;
        // massDrag.transform.localRotation = Quaternion.identity;
        
        massDrag.SetCurrentSlot(this);

        MassRegulator draggedMassRegulator = massDrag.GetMassRegulator();

        if (myMassRegulator != null && draggedMassRegulator != null)
        {
            // Logic to connect the dragged mass with this slot's mass regulator
            var childrenList = new System.Collections.Generic.List<MassRegulator>(myMassRegulator.children ?? new MassRegulator[0]);
            if (!childrenList.Contains(draggedMassRegulator))
            {
                childrenList.Add(draggedMassRegulator);
                myMassRegulator.children = childrenList.ToArray();
            }
        }

        // Debug.Log($"Mass {massDrag.name} colocado correctamente en slot {gameObject.name}");
        
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.OnMassPlaced(this, massDrag);
        }
    }

    public bool IsOccupied() => isOccupied;

    public void RemoveMass()
    {
        if (placedMass != null)
        {
            // Unparent from the slot
            placedMass.transform.SetParent(null);

            MassRegulator draggedMassRegulator = placedMass.GetMassRegulator();
            if (myMassRegulator != null && draggedMassRegulator != null && myMassRegulator.children != null)
            {
                var childrenList = new System.Collections.Generic.List<MassRegulator>(myMassRegulator.children);
                if (childrenList.Contains(draggedMassRegulator))
                {
                    childrenList.Remove(draggedMassRegulator);
                    myMassRegulator.children = childrenList.ToArray();
                }
                
                // If it was the last mass, ensure mass is 0 (CalculateMass will handle it if array is empty, 
                // but we need to ensure the slot doesn't keep the previous sum)
                if (myMassRegulator.children.Length == 0)
                {
                    myMassRegulator.mass = 0;
                }
            }
            
            // Clear the slot reference
            placedMass.SetCurrentSlot(null);
        }
        
        placedMass = null;
        isOccupied = false;
        Debug.Log($"Slot {gameObject.name} cleared and ready for new mass.");
    }
}