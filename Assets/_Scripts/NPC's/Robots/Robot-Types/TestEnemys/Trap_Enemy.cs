using UnityEngine;

// TRAP ENEMY -> Doesn’t react to player,
public class Trap_Enemy : Base_AI
{
    protected override void Start()
    {
        base.Start();
        ChangeState(new PatrollingState(this));
    }

    protected override void Update()
    {
        base.Update();
    }

    // Ontrigger -> Enemt touch the Player.
    private void OnTriggerEnter(Collider other)
    {
        // If the player has PC ->
        var playerRef = other.GetComponent<PlayerController>();
        
        if (playerRef != null)
        {
            playerRef.RespawnCoroutine();
        }
    }
}