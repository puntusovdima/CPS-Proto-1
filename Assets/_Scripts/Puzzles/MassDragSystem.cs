using UnityEngine;
using UnityEngine.InputSystem;

public class MassDragSystem : MonoBehaviour
{
    [Header("MASS SETTINGS")] 
    [SerializeField] private float speedToDrag = 10f;
    [SerializeField] private Camera pCamera;

    private bool isDraggingAMass = false;
    private MassRegulator m;
    private MassSlotPuzzle currentSlot;

    private void Start()
    {
        if (pCamera == null)
        {
            pCamera = Camera.main;
        }

        m = GetComponent<MassRegulator>();
        if (m == null)
        {
            m = GetComponentInChildren<MassRegulator>();
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryToGrabMass();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            UndoM();
        }

        if (isDraggingAMass)
        {
            DragTheMass();
        }
    }

    private void TryToGrabMass()
    {
        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool hitThisMass = false;

        if (Physics.Raycast(rayC, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform)
            {
                hitThisMass = true;
            }
        }

        if (hitThisMass)
        {
            isDraggingAMass = true;
            transform.SetParent(null);

            if (currentSlot != null)
            {
                MassSlotPuzzle slotToClear = currentSlot;
                currentSlot = null;
                slotToClear.RemoveMass();
            }
        }
    }

    private void UndoM()
    {
        isDraggingAMass = false;
    }

    private void DragTheMass()
    {
        Ray rayC = pCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane dragPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));

        if (dragPlane.Raycast(rayC, out float hitDist))
        {
            Vector3 targetPos = rayC.origin + rayC.direction * hitDist;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speedToDrag);
        }
    }

    public bool IsDragging() => isDraggingAMass;
    public MassRegulator GetMassRegulator() => m;

    public void SetCurrentSlot(MassSlotPuzzle slot)
    {
        currentSlot = slot;
    }
}