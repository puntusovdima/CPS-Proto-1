using UnityEngine;
using UnityEngine.InputSystem;

public class GearDragSystem : MonoBehaviour
{
    [Header("GEARS SETTINGS")]
    [SerializeField] private float distanceToDrag;
    [SerializeField] private float speedToDrag;
    [SerializeField] private Camera pCamera;

    [Header("PUZZLE SETTINGS")]
    [SerializeField] private GearPuzzleWhitTime timedPuzzle;

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
        if (c == null) c = GetComponentInChildren<ChainGear>();
        
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
        if (timedPuzzle != null)
        {
            if (timedPuzzle.IsPuzzleSolved())
                return;
        }
        else if (PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleSolved())
        {
            return;
        }

        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool hitThisGear = false;

        if (Physics.Raycast(rayC, out RaycastHit hit, Mathf.Infinity))
        {
            // Check if the hit object is this object or one of its children
            if (hit.collider.transform.IsChildOf(transform))
            {
                hitThisGear = true;
            }
        }

        if (hitThisGear)
        {
            isDraggingAGear = true;

            if (currentSlot != null)
            {
                // Store reference to clear it, then null it locally
                GearSlotPuzzle slotToClear = currentSlot;
                currentSlot = null; 
                slotToClear.RemoveGear();
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