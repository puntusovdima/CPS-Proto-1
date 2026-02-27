using System.Collections;
using UnityEngine;

public class ObjMoveTrigger : MonoBehaviour
{
    [Header ("Directed movement")]
    [SerializeField] private GameObject fallingPlatform;//Object that falls
    [SerializeField] private float countdownToFall;
  
    private Rigidbody _rb;

    private void Start()
    {
        _rb = fallingPlatform.GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.isKinematic = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(countdownToFall);
        DropObject();
    }

    private void DropObject()
    {
        _rb.useGravity = true;
        _rb.isKinematic = false;
        
        this.gameObject.SetActive(false);
    }
}
    

