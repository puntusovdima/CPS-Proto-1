using System;
using UnityEngine;

public class MassRegulator : MonoBehaviour
{
    [SerializeField] private Transform pulley;
    private float ropeLength;    // Same length for all (for now)
    public Transform pulleyGuide;
    public MassRegulator neighbor;
    public Rigidbody2D rb;
    public Transform pulley_bottom;
    public Transform transform;
    public float tension;
    public float velocity = 0;
    public float acceleration = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
    }
    private void Start()
    {
        if (neighbor == null)
            return;
        var pulleyHalfRadius = pulley.localScale.x * (float)Math.PI / 2f;   // 2*PI*R / 2 -- bc scale = radius
        ropeLength = Vector3.Distance(pulleyGuide.position, transform.position + transform.localScale / 2) * 2;// + pulleyHalfRadius;
        tension = neighbor.rb.mass * 9.81f; // tension = -1 * neighbor's weight
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // F = ma -> Sum_F = P + T: P + T = ma -> (P + T) / m = a
        float weight = rb.mass * -9.81f;
        acceleration =((weight + tension) / rb.mass);
        // if (weight > tension)
        // {
        //     acceleration = -Math.Abs(acceleration);
        // } else if (weight < tension)
        // {
        //     acceleration = Math.Abs(acceleration);
        // }
        if (transform.position.y >= pulley_bottom.position.y || neighbor.transform.position.y >= pulley_bottom.position.y)
        {
            velocity = 0;
            return;
        }
        velocity += acceleration * Time.fixedDeltaTime;
        float travelY =  velocity * Time.fixedDeltaTime;
        
        Debug.Log(this.name + "'s acceleration: " + acceleration);
        Debug.Log(this.name + "'s travelY : " + travelY);
        transform.position = new Vector3(transform.position.x, transform.position.y + travelY, transform.position.z );
        
        Debug.Log(this.name + "'s velocity magnitude: " + rb.linearVelocity.magnitude);
    }
    
}
