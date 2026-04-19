using UnityEngine;

public class MassRegulator : MonoBehaviour
{
    public float mass = 1f;
    public MassRegulator[] children;

    private void Start()
    {
        // Periodic check as a fallback
        InvokeRepeating(nameof(CalculateMass), 0.5f, 0.5f);
    }

    public void CalculateMass()
    {
        // Only calculate if we have a children array and it's not empty.
        if (children == null || children.Length == 0)
        {
            // If we have no children, we don't overwrite the base mass
            return;
        }

        float totalMass = 0f;
        bool hasValidChild = false;

        // Loop through all children
        foreach (MassRegulator child in children)
        {
            if (child != null)
            {
                totalMass += child.mass;
                hasValidChild = true;
            }
        }

        if (hasValidChild)
        {
            mass = totalMass;
        }
    }

    public void RecalculateAllUpwards()
    {
        CalculateMass();
        
        // Find parent if it exists and tell it to recalculate
        if (transform.parent != null)
        {
            MassRegulator parentReg = transform.parent.GetComponentInParent<MassRegulator>();
            if (parentReg != null && parentReg != this)
            {
                parentReg.RecalculateAllUpwards();
            }
        }
    }
}