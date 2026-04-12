using System;
using Unity.VisualScripting;
using UnityEngine;

// AREA GUARD ENEMY.
public class AreaGuardEnemy : Base_AI {

    [Header("AREA SETTINGS")]
    [SerializeField] private Collider ChaseArea;
    [SerializeField] private bool showChaseArea = true;

    protected override void Start()
    {
        base.Start();
        if(ChaseArea == null) return;
        ChangeState(new PatrollingState(this));
    }

    protected override void Update()
    {
        base.Update();
        
        if(currentState is PatrollingState && CanDetectPlayer() && PlayerDetected()){
            ChangeState(new ChasingState(this));
            
        }
        
        if(currentState is ChasingState && !PlayerDetected()){
             ChangeState(new PatrollingState(this));
        }
    }
    private bool PlayerDetected(){
        if(ChaseArea == null){
            return true;
        }
        if(PlayerController.Instance == null){
            return false;
        }
        Vector3 playerPositionB = PlayerController.Instance.transform.position;
        bool inside = ChaseArea.bounds.Contains(playerPositionB);
        return inside;
    }

    // Ontrigger -> Enemy touch the Player -> Player Die.
    private void OnTriggerEnter(Collider other)
    {
        // If the player has PC ->
        var playerRef = other.GetComponent<PlayerController>();
        
        if (playerRef != null)
        {
            playerRef.RespawnCoroutine();
        }
    }

    // FOR VISUALIZE THE BOX AREA IN THE EDITOR.
    private void OnDrawGizmosSelected()
    {
        if (!showChaseArea || ChaseArea == null) {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(ChaseArea.bounds.center, ChaseArea.bounds.size);
    }
}