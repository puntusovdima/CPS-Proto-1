using UnityEngine;

public class AtwoodManager : MonoBehaviour
{
    [Header("System References")] 
    public MassRegulator leftMass;
    public MassRegulator rightMass;
    [Tooltip("Optional: The physical object to move (like a tray/slot). If empty, uses the MassRegulator's transform.")]
    public Transform leftTransform;
    [Tooltip("Optional: The physical object to move (like a tray/slot). If empty, uses the MassRegulator's transform.")]
    public Transform rightTransform;
    public Transform pulley;
    public Rope rope;

    [Header("Physics State")] 
    public float velocity = 0f;
    [SerializeField] private float friction = 0.98f;
    public float systemTravelY = 0f; // Positive = Left goes UP, Right goes DOWN

    // CHANGED: We now store the offset distance from the pulley, not absolute positions
    private Vector3 leftOffsetFromPulley;
    private Vector3 rightOffsetFromPulley;
    
    private Vector3 leftGuideOffset;
    private Vector3 rightGuideOffset;
    
    private float pulleyCircumference;

    private void Awake()
    {
        rope = GetComponentInChildren<Rope>();
    }

    private void Start()
    {
        Transform lTarget = leftTransform != null ? leftTransform : leftMass.transform;
        Transform rTarget = rightTransform != null ? rightTransform : rightMass.transform;

        // Force the rope to attach to our new slots instead of the old masses
        if (rope != null)
        {
            rope.leftEnd = lTarget;
            rope.rightEnd = rTarget;
            
            // Recalculate lengths in case Rope.Awake used the old references
            if (rope.leftGuide != null && rope.rightGuide != null)
            {
                rope.leftLenght = Vector2.Distance(rope.leftEnd.position, rope.leftGuide.position);
                rope.rightLenght = Vector2.Distance(rope.rightEnd.position, rope.rightGuide.position);
                rope.length = rope.leftLenght + rope.rightLenght;
            }
        }

        // Calculate the initial gap between the mass and the pulley in world space
        leftOffsetFromPulley = lTarget.position - pulley.position;
        rightOffsetFromPulley = rTarget.position - pulley.position;

        // Record where the guides are relative to the pulley's center
        if (rope != null && rope.leftGuide != null && rope.rightGuide != null)
        {
            leftGuideOffset = rope.leftGuide.position - pulley.position;
            rightGuideOffset = rope.rightGuide.position - pulley.position;
        }
        pulleyCircumference = Mathf.PI * pulley.localScale.x;
    }

    private void FixedUpdate()
    {
        if (leftMass == null || rightMass == null) return;

        float mLeft = leftMass.mass;
        float mRight = rightMass.mass;
        float totalMass = mLeft + mRight;

        float acceleration = totalMass > 0 ? 9.81f * (mRight - mLeft) / totalMass : 0f;
        float nextVelocity = (velocity + acceleration * Time.fixedDeltaTime) * friction;
        float nextTravel = systemTravelY + (nextVelocity * Time.fixedDeltaTime);

        float maxTravelUp = rope.leftLenght;
        float maxTravelDown = rope.rightLenght;

        if (nextTravel >= maxTravelUp && nextVelocity > 0)
        {
            nextVelocity = 0;
            nextTravel = maxTravelUp;
        }
        else if (nextTravel <= -maxTravelDown && nextVelocity < 0)
        {
            float bounceDamping = 0.6f;
            nextVelocity = -nextVelocity * bounceDamping;
            nextTravel = -maxTravelDown;
        }

        velocity = nextVelocity;
        systemTravelY = nextTravel;

        Transform lTarget = leftTransform != null ? leftTransform : leftMass.transform;
        Transform rTarget = rightTransform != null ? rightTransform : rightMass.transform;

        // CHANGED: Update position based on the Pulley's CURRENT world position
        lTarget.position = pulley.position + leftOffsetFromPulley + new Vector3(0, systemTravelY, 0);
        rTarget.position = pulley.position + rightOffsetFromPulley + new Vector3(0, -systemTravelY, 0);

        if (rope != null && rope.leftGuide && rope.rightGuide)
        {
            rope.leftGuide.position = pulley.position + leftGuideOffset;
            rope.rightGuide.position = pulley.position + rightGuideOffset;
        }
        // Note: For a 2D wheel facing the camera, you usually rotate the Z axis (0, 0, rotationStep)
        // If your pulley is oriented differently in 3D, change this back to (0, rotationStep, 0)
        float rotationStep = (velocity * Time.fixedDeltaTime / pulleyCircumference) * 360f;
        pulley.Rotate(0, rotationStep, 0, Space.Self);
    }
}