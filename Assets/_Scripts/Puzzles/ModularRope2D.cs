using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ModularRope2D : MonoBehaviour
{
    [Header("Rope Settings")]
    public float targetLength = 10f;  
    public float springForce = 2000f; 
    public float damper = 50f;       
    public float ropeWidth = 0.1f;

    [Header("Rope Path (Drag Transforms here in order)")]
    public List<Transform> nodes = new List<Transform>();

    private LineRenderer lr;
    private float currentLength;
    private float previousLength;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.startWidth = ropeWidth;
        lr.endWidth = ropeWidth;
        
        // Ensure LineRenderer uses World Space to draw correctly between objects
        lr.useWorldSpace = true;

        // Auto-calculate initial length so it doesn't snap at start
        if (nodes.Count > 1)
        {
            currentLength = CalculateTotalLength();
            previousLength = currentLength;
            targetLength = currentLength; 
        }
    }

    void FixedUpdate()
    {
        if (nodes.Count < 2) return;
        SimulateRopePhysics();
    }

    void Update()
    {
        RenderRope();
    }

    float CalculateTotalLength()
    {
        float total = 0f;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] != null && nodes[i+1] != null)
                // We use Vector2.Distance to ignore Z depth in physics calculation
                total += Vector2.Distance(nodes[i].position, nodes[i+1].position);
        }
        return total;
    }

    void SimulateRopePhysics()
    {
        currentLength = CalculateTotalLength();

        // 1. Calculate Tension
        float stretch = currentLength - targetLength;
        if (stretch <= 0) return; // Slack rope = no force

        // Damping calculation
        float stretchVelocity = (currentLength - previousLength) / Time.fixedDeltaTime;
        previousLength = currentLength;

        float tension = (stretch * springForce) + (stretchVelocity * damper);
        tension = Mathf.Max(0, tension);

        // 2. Apply Forces to nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            Transform currentNode = nodes[i];
            if (currentNode == null) continue;

            // Look for Rigidbody2D instead of Rigidbody
            Rigidbody2D rb = currentNode.GetComponentInParent<Rigidbody2D>();
            
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 forceVector = Vector2.zero;
                Vector2 currentPos = currentNode.position;

                // Pull towards PREVIOUS node
                if (i > 0)
                {
                    Vector2 prevPos = nodes[i-1].position;
                    forceVector += (prevPos - currentPos).normalized;
                }

                // Pull towards NEXT node
                if (i < nodes.Count - 1)
                {
                    Vector2 nextPos = nodes[i+1].position;
                    forceVector += (nextPos - currentPos).normalized;
                }

                // Apply Force (Vector2)
                rb.AddForceAtPosition(forceVector * tension, currentPos);
            }
        }
    }

    void RenderRope()
    {
        if (nodes.Count < 2) return;
        
        lr.positionCount = nodes.Count;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null)
            {
                // We use the full 3D position for rendering so it looks correct in URP
                lr.SetPosition(i, nodes[i].position);
            }
        }
    }
}