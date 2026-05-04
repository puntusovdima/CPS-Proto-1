using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        var deathUI = FindFirstObjectByType<DeathScreenUI>();
        if (deathUI != null)
        {
            deathUI.PlayDeathSequence();
            PlayerController.Instance.SetPause(true);
        }
    }
}
