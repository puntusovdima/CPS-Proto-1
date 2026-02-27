using UnityEngine;

public class ObjMoveButton : MonoBehaviour, IInteractable
{
    [Header("Directed movement")]
    [SerializeField] private GameObject movingObject;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform endMarker;
    [SerializeField] public float speed;

    private bool _isMoving = false;

    private void Start()
    {
        movingObject.transform.position = startMarker.position;
    }

    private void Update()
    {
        if (!_isMoving) return;

        movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, 
            endMarker.position, speed * Time.deltaTime);

        if (movingObject.transform.position == endMarker.position)
        {
            _isMoving = false;
        }
    }

    public void Interact()
    {
        Debug.Log("Interaction with " + this.name);

        if (movingObject.transform.position != endMarker.position)
        {
            _isMoving = true;
        }
    }
}