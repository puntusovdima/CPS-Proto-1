using UnityEngine;
using UnityEngine.Rendering;

public class ObjMoveTrigger : MonoBehaviour
{
    [Header ("Directed movement")]
    [SerializeField] private GameObject movingObject;//Object that falls
    [SerializeField] private Transform startMarker;//Starting point of the object
    [SerializeField] private Transform endMarker;//Final point of the object
    [SerializeField, Range (0,1)]public float speed; //Speed of the lerp
  

    private Rigidbody rb;

    void Start()
    {
        startMarker=movingObject.transform;

        rb = movingObject.GetComponent<Rigidbody>();
    }
    void OnTriggerStay()
    {
        //ChangeGravity();
        LerpObject();       
    }

    private void ChangeGravity()
    {
        rb.useGravity = true;
    }

    private void LerpObject()
    {
        movingObject.transform.position = Vector3.MoveTowards(startMarker.position, endMarker.position, speed);
    }
}
    

