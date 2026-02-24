using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoint References")] 
    [SerializeField] private GameObject[] checkpoints;
    
    private int _currCheckpointIndex = 0;

    /// <summary>
    /// Deactivates all checkpointes except the first one at the start of the game,
    /// so the player can respawn at the first checkpoint if they die before reaching any other checkpoint.
    /// </summary>
    private void Start()
    {
        _currCheckpointIndex = 0;
        
        for (var i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].SetActive(i == _currCheckpointIndex);
        }
    }

    /// <summary>
    /// Activates the next checkpoint and deactivates the current one,
    /// this function is called when the player reaches a checkpoint.
    /// </summary>
    public void IncreaseCheckPoint()
    {
        checkpoints[_currCheckpointIndex].SetActive(false);
        _currCheckpointIndex++;
        checkpoints[_currCheckpointIndex].SetActive(true);
    }
    
    /// <summary>
    /// This funtion returns the current checkpoint location so the player can respawn there after dying.
    /// </summary>
    /// <returns>Checkpoint Position</returns>
    public Transform GetCurrentCheckpointPosition()
    {
        return checkpoints[_currCheckpointIndex].transform;
    }
}
