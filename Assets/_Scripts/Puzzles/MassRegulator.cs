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
        // Safety check: if there are no children, we don't want to overwrite 
        // the mass or we might reset a leaf-node's mass to zero.
        if (children == null || children.Length == 0) return;

        float totalMass = 0f;

        // Loop through all children instead of just index 0 and 1
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