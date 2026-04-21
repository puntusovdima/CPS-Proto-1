using UnityEngine;

// NEUTRAL ROBOT.
public class Neutral_Robot : Friendly_Robot
{
  private enum NeutralState
    {
        Waiting,
        Ally,
        Enemy
    }

    [SerializeField] private NeutralState state = NeutralState.Waiting;
    private bool isA = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isA)return;
        if (state != NeutralState.Enemy || !other.CompareTag("Player")) return;
        
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.RespawnCoroutine();
        }
    }
}