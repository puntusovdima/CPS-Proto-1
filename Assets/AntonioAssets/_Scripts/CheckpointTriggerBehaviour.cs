using System;
using UnityEngine;

public class CheckpointTriggerBehaviour : MonoBehaviour
{
    private bool _hasPassedCheckpoint = false;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.name);

        if (!other.CompareTag("Player") || _hasPassedCheckpoint) return;
        
        ToggleHasPassedCheckpoint();// must be changed when the player finishes the room
        GameManager.Instance.IncreaseCheckpoint();
    }
    
    public void ToggleHasPassedCheckpoint()
    {
        _hasPassedCheckpoint = !_hasPassedCheckpoint;
    }
}
