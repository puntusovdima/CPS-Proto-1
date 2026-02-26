using UnityEngine;

public class ObjMoveButton : MonoBehaviour, IInteractable
{

    [Header("Directed movement")]
    [SerializeField] private GameObject movingObject;//Object that falls
    [SerializeField] private Transform startMarker;//Starting point of the object
    [SerializeField] private Transform endMarker;//Final point of the object
    [SerializeField] public float speed; //Speed of the lerp

    private void Start()
    {
        movingObject.transform.position = startMarker.position;
    }

    private void MoveObject()
    {
        movingObject.transform.position = 
            Vector3.MoveTowards(movingObject.transform.position, endMarker.position, 
                speed * Time.deltaTime);
    }

    public void Interact()
    {
        Debug.Log("Interaction with " + this.name);
        
        MoveObject();
    }
}
