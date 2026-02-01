using UnityEngine;

public class SimpleGearRotator : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up; // Or Vector3.up, depending on orientation
    public float speed = 50f;

    void Update()
    {
        // Rotate the object around the defined axis at 'speed' degrees per second
        transform.Rotate(rotationAxis * speed * Time.deltaTime);
    }
}