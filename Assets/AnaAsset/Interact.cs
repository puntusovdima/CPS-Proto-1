using System.Net;
using TMPro;
using UnityEngine;

public class Interact : MonoBehaviour
{
    
    /*[Header("CANVAS SETTINGS")]
    public Canvas canva;*/
 

    private void Start()
    {
        //canva.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //canva.enabled = true;
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKey("e"))
        {
            DoEvent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //canva.enabled = true;
        
    }

    public virtual void DoEvent() { }














}

