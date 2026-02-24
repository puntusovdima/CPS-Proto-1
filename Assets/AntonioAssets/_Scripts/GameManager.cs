using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool hasCompanion = false;
    
    [Header("Game Settings")]
    [SerializeField] private GameStateEnum gameState;
    
    public static GameManager Instance;
    private CheckpointManager _checkpointManager;
    
    private bool _hasCompletedPuzzle = false;
    
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
        
        if(_checkpointManager == null) _checkpointManager = GetComponent<CheckpointManager>();
    }
    
    public Transform GetCurrentCheckpointPosition()
    {
        return _checkpointManager.GetCurrentCheckpointPosition();
    }
    
    public void IncreaseCheckpoint()
    {
        _checkpointManager.IncreaseCheckPoint();
    }

    public void ToggleHasCompanion()
    {
        hasCompanion = !hasCompanion;
    }
    
    public void ToggleHasCompletedPuzzle()
    {
        _hasCompletedPuzzle = !_hasCompletedPuzzle;
    }
}
