using UnityEngine;

public class ObjMoveButton : Interact
{

    [Header("Directed movement")]
    [SerializeField] private GameObject movingObject;//Object that falls
    [SerializeField] private Transform startMarker;//Starting point of the object
    [SerializeField] private Transform endMarker;//Final point of the object
    [SerializeField, Range(0, 1)] public float speed; //Speed of the lerp
    private bool isMoving;

    private void Update()
    {
        if (isMoving)
        {
            movingObject.transform.position = Vector3.MoveTowards(startMarker.position, endMarker.position, speed * Time.deltaTime);
        }

    }

    public override void DoEvent()
    {
        //gameObject.transform.position = Vector3.MoveTowards(startMarker.position, endMarker.position, speed*Time.deltaTime);
        isMoving = true;
    }
}
