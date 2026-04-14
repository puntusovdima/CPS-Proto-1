using UnityEngine;

// ENEMY ROBOT.
public class Enemy_Robot : Base_AI
{
    [Header("Enemy Robot Settings")]
    [SerializeField] private float chaseSpeed = 6.5f;

    protected override void Update()
    {
        base.Update();
        if (CanDetectPlayer())
        {
            agent.speed = chaseSpeed;
            if (player != null){
                agent.SetDestination(player.position);
            }
        }
        else
        {
            agent.speed = patrolSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                FindFirstObjectByType<DeathScreenUI>().PlayDeathSequence();
            }
        }
    }
}