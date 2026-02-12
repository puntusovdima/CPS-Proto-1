using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoint References  ")] 
    [SerializeField] private GameObject[] checkpoints;

    public static CheckpointManager Instance;
    
    private int _currCheckpointIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
