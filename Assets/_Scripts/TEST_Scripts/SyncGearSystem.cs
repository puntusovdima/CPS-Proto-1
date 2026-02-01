using UnityEngine;

public class SyncGearSystem : MonoBehaviour
{
    [Header("Settings")]
    public Transform inputGear;      // The DRIVER gear (not the cylinder!)
    public float gearRatio = 1f;     // 1 if teeth count is same. 0.5 if driver is half size.
    public bool reverseDirection = true; // Standard gears always reverse direction
    
    [Header("Axis Settings")]
    // Which axis is the gear spinning around? (Green arrow = Y, Blue = Z, Red = X)
    public Vector3 spinAxis = Vector3.forward; 

    private Quaternion startRotation;
    private Quaternion inputStartRotation;

    void Start()
    {
        if (inputGear == null) return;

        // Remember where we started
        startRotation = transform.localRotation;
        inputStartRotation = inputGear.localRotation;
    }

    void Update()
    {
        if (inputGear == null) return;

        // 1. Calculate how much the INPUT gear has spun total
        // We use the Dot product to see if it spun positive or negative relative to the axis
        float inputCurrentAngle = GetAxisRotation(inputGear, spinAxis);
        float inputStartAngle = GetAxisRotation(inputGear, spinAxis, true);
        
        float angleDifference = inputCurrentAngle - inputStartAngle;

        // 2. Apply Ratio and Direction
        float myRotationAmount = angleDifference * gearRatio;
        if (reverseDirection) myRotationAmount *= -1;

        // 3. Apply to this gear
        // We rotate AROUND the local axis relative to the start rotation
        transform.localRotation = startRotation * Quaternion.AngleAxis(myRotationAmount, spinAxis);
    }

    // Helper to get rotation angle around a specific local axis
    float GetAxisRotation(Transform t, Vector3 axis, bool useStart = false)
    {
        // This is a simplified way to get the angle for single-axis rotation
        // If your axis is Z (0,0,1), it reads eulerAngles.z
        Vector3 euler = useStart ? inputStartRotation.eulerAngles : t.localEulerAngles;
        
        if (axis == Vector3.forward) return euler.z;
        if (axis == Vector3.up) return euler.y;
        if (axis == Vector3.right) return euler.x;
        
        return 0; // Fallback
    }
}