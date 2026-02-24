using UnityEngine;
using System.Collections.Generic;

public class ChainGear : MonoBehaviour
{
    [Header("Physical Properties")]
    public int teeth = 10;
    public Vector3 axis = Vector3.up; // Check your gizmo line!

    [Header("Connections")]
    public ChainGear inputGear; // Drag the driver here
    
    [Header("Motor Settings")]
    public bool isMotor = false;
    public float motorSpeed = 50f;

    // Private state
    private List<ChainGear> connectedFollowers = new List<ChainGear>();
    private Quaternion initialRotation;
    private float currentTotalAngle = 0f;

    void Start()
    {
        // 1. Capture the exact rotation you set in the editor so teeth stay aligned
        initialRotation = transform.localRotation;

        // 2. If I have an input gear, tell it "I am your child"
        if (inputGear != null)
        {
            inputGear.RegisterFollower(this);
        }
    }

    // Called by child gears during Start() to link the chain
    public void RegisterFollower(ChainGear follower)
    {
        if (!connectedFollowers.Contains(follower))
        {
            connectedFollowers.Add(follower);
        }
    }

    void Update()
    {
        // ONLY the Motor is allowed to initiate movement
        if (isMotor)
        {
            float moveAmount = motorSpeed * Time.deltaTime;
            Drive(moveAmount);
        }
    }

    // This function is called recursively down the chain
    public void Drive(float angleDelta)
    {
        // 1. Rotate Myself
        currentTotalAngle += angleDelta;
        
        // We use absolute rotation (Start + Offset) to prevent floating point drift over time
        transform.localRotation = initialRotation * Quaternion.AngleAxis(currentTotalAngle, axis);

        // 2. Push the rotation to all followers instantly
        foreach (var follower in connectedFollowers)
        {
            // Calculate ratio: Input Teeth / Output Teeth
            float ratio = (float)teeth / (float)follower.teeth;
            
            // Flip direction (-1)
            float outputAngle = angleDelta * ratio * -1f;
            
            follower.Drive(outputAngle);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(axis) * 1.5f);
    }
}