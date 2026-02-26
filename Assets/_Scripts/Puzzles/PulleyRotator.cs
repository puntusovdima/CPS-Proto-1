using System;
using UnityEngine;

public class PulleyRotator : MonoBehaviour
{
    public Transform leftGuide, rightGuide;
    public Vector3 leftGuideInitPos, rightGuideInitPos;
    [SerializeField] private MassRegulator leftMass;
    private float travel;
    private float circumference;
    private float rotationStep;
    private void Awake()
    {
        circumference = Mathf.PI * transform.localScale.x;   //2Pi*R
        leftGuideInitPos = leftGuide.localPosition;
        rightGuideInitPos = rightGuide.localPosition;
    }
    private void FixedUpdate()
    {
        leftGuide.localPosition = leftGuideInitPos;
        rightGuide.localPosition = rightGuideInitPos;
        // we need to calculate the travel based on velocity
        // we take the left mass so positive travelY is clockWise
        // travel = leftMass.velocity * Time.fixedDeltaTime;
        // translate travel to degrees
        rotationStep = (travel / circumference) * 360;
        transform.Rotate(0, rotationStep, 0, Space.Self);    
    }
    
}
