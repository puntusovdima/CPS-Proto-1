using UnityEngine;

public class MassRegulator : MonoBehaviour
{
    public float mass = 1f;
    public MassRegulator[] children;

    private void Start()
    {
        // InvokeRepeating takes the name of the method as a string.
        // Use nameof() to avoid typos and make it refactor-friendly.
        InvokeRepeating(nameof(CalculateMass), 0f, 0.5f);
    }

    private void CalculateMass()
    {
        // Only calculate if we have a children array and it's not empty.
        if (children == null || children.Length == 0) return;

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

        // Only overwrite mass if we actually found children to sum.
        // This preserves the manually set mass for leaf nodes (like "just a mass").
        if (hasValidChild)
        {
            mass = totalMass;
        }
    }
}