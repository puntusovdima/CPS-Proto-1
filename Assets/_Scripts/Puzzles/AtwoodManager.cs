using UnityEngine;

public class AtwoodManager : MonoBehaviour
{
    [Header("System References")] 
    public MassRegulator leftMass;
    public MassRegulator rightMass;
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
        // Calculate the initial gap between the mass and the pulley in world space
        leftOffsetFromPulley = leftMass.transform.position - pulley.position;
        rightOffsetFromPulley = rightMass.transform.position - pulley.position;

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
        if (this.name.Contains("_2")){
            Debug.Log(this.name + "'s: left mass: " + mLeft + "; right mass: " + mRight);
            Debug.Log(this.name + "'s: total mass: " + totalMass);
            Debug.Log(this.name + "'s acceleration: " + acceleration);
            Debug.Log(this.name + "'s nextVelocity: " + nextVelocity);
        }
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

        // CHANGED: Update position based on the Pulley's CURRENT world position
        leftMass.transform.position = pulley.position + leftOffsetFromPulley + new Vector3(0, systemTravelY, 0);
        rightMass.transform.position = pulley.position + rightOffsetFromPulley + new Vector3(0, -systemTravelY, 0);

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