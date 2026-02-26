using UnityEngine;
using UnityEngine.Rendering;

public class ObjMoveTrigger : MonoBehaviour, IInteractable
{
    [Header ("Directed movement")]
    [SerializeField] private GameObject movingObject;//Object that falls
    [SerializeField] private Transform startMarker;//Starting point of the object
    [SerializeField] private Transform endMarker;//Final point of the object
    [SerializeField, Range (0,1)]public float speed; //Speed of the lerp
  

    private Rigidbody _rb;

    void Start()
    {
        startMarker=movingObject.transform;

        _rb = movingObject.GetComponent<Rigidbody>();
    }
    void OnTriggerStay(Collider other)
    {
        //ChangeGravity();
        LerpObject();       
    }

    private void ChangeGravity()
    {
        _rb.useGravity = true;
    }

    private void LerpObject()
    {
        movingObject.transform.position = Vector3.MoveTowards(startMarker.position, endMarker.position, speed);
    }

    public void Interact()
    {
        Debug.Log("Interaction with " + this.name);
        
        LerpObject();
    }
}
    

