using UnityEngine;
using UnityEngine.InputSystem;

public class GearDragSystem : MonoBehaviour
{
    [Header("GEARS SETTINGS")]
    [SerializeField] private float distanceToDrag;
    [SerializeField] private float speedToDrag;
    [SerializeField] private Camera pCamera;

    private bool isDraggingAGear = false;
    private Vector3 off;
    private ChainGear c;
    private Vector3 FirstPosition;
    private GearSlotPuzzle currentSlot; 


    private void Start()
    {
        if (pCamera == null)
        {
            pCamera = Camera.main;
        }
        c = GetComponent<ChainGear>();
        FirstPosition = transform.position;
    }


    private void Update()
    {
        // PLAYER INPUTS -> TRY TO GRAB...
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryToGrabGear();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            UndoG();
        }
        if (isDraggingAGear)
        {
            DragTheGear();
        }
    }

    // DRAG LOGIC.
    private void TryToGrabGear()
    {

    // Can't grab the gear if the gear is in the right slot.
    if (currentSlot != null && currentSlot.IsOccupied()){
        return;
    }
        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool hitThisGear = false;

        if (Physics.Raycast(rayC, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitThisGear = true;
            }
        }

        if (hitThisGear)
        {
            isDraggingAGear = true;
            
            if (currentSlot != null)
            {
                currentSlot.RemoveGear();
                currentSlot = null;
            }
        }
    }

    private void UndoG()
    {
        isDraggingAGear = false;
        off = Vector3.zero;
    }

    private void DragTheGear()
    {
        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane dragPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
        
        if (dragPlane.Raycast(rayC, out float hitDist))
        {
            Vector3 targetPos = rayC.origin + rayC.direction * hitDist;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speedToDrag);
        }
    }

    public bool IsDragging() => isDraggingAGear;
    public ChainGear GetChainGear() => c;
    
    public void SetCurrentSlot(GearSlotPuzzle slot)
    {
        currentSlot = slot;
    }
}

