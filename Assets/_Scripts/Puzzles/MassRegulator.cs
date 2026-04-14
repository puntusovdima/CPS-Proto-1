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
        // Only calculate if we have children to sum.
        // If children is null or empty, this is a leaf node/base mass, so keep current mass.
        if (children == null || children.Length == 0) return;

        float totalMass = 0f;

        // Loop through all children
        foreach (MassRegulator child in children)
        {
            if (child != null)
            {
                totalMass += child.mass;
            }
        }

        mass = totalMass;
    }
}