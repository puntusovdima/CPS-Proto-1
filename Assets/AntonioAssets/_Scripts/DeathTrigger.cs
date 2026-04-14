using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        _playerController.RespawnCoroutine();
    }
}
