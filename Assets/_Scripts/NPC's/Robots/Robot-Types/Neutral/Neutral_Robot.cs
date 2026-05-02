using UnityEngine;

// NEUTRAL ROBOT.
public class Neutral_Robot : Base_AI
{
    private enum NeutralState
    {
        Waiting,
        Ally,
        Enemy
    }

    [SerializeField] private NeutralState state = NeutralState.Waiting;
    [SerializeField] private float chaseSpeedWhenEnemy = 6.5f;
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated) return;
        if (state != NeutralState.Enemy || !other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.RespawnCoroutine();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (state == NeutralState.Enemy && CanDetectPlayer())
        {
            agent.speed = chaseSpeedWhenEnemy;
            if (player != null)
            {
                agent.SetDestination(player.position);
            }
        }
        else if (state == NeutralState.Ally && CanDetectPlayer())
        {
            agent.speed = speedForChasing;
            if (player != null)
            {
                agent.SetDestination(player.position);
            }
        }
        else
        {
            agent.speed = patrolSpeed;
        }
    }

    public void KillPlayer()
    {
        state = NeutralState.Enemy;
        isActivated = true;
        FindFirstObjectByType<DeathScreenUI>().PlayDeathSequence();
        Debug.Log("UI ACTIVA");
    }

    public void StartFollowing()
    {
        state = NeutralState.Ally;
        isActivated = true;
    }
}