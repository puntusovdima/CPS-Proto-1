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
        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(rayC, out RaycastHit hit, 1000f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isDraggingAGear = true;
                off = transform.position - hit.point;
                
                if (currentSlot != null)
                {
                    currentSlot.RemoveGear();
                    currentSlot = null;
                }
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
        Vector3 targetPos = rayC.origin + rayC.direction * distanceToDrag;
        
        targetPos.z = FirstPosition.z;
        
        transform.position = Vector3.Lerp(transform.position, targetPos + off, Time.deltaTime * speedToDrag);
    }

    public bool IsDragging() => isDraggingAGear;
    public ChainGear GetChainGear() => c;
    
    public void SetCurrentSlot(GearSlotPuzzle slot)
    {
        currentSlot = slot;
    }
}

