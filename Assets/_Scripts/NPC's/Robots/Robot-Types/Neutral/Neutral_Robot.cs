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

    public void SetAsAlly()
    {
        state = NeutralState.Ally;
        isA = true;
        FriendlyModeActivation(); // same as friendly robot.
    }

    public void SetAsEnemy()
    {
        state = NeutralState.Enemy;
        isA = true;
        // ANIMATION OF THE DEAD OR SOMETRHING ELSE HERE.

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isA)return;
        if (state == NeutralState.Enemy && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RespawnCoroutine();
            }
        }
    }
}