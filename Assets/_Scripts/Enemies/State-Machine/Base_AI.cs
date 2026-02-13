using UnityEngine;
using UnityEngine.AI;

public class Base_AI : MonoBehaviour
{

    [Header("Patrol Settings / WayPoints Settings")]
    [SerializeField] protected float patrolSpeed = 2.0f;
    [SerializeField] public bool useWaypoints = false;
    [SerializeField] public Transform[] wayPoints; // Array for all the waypoint in the scene for the enemy.
    [SerializeField] public float waypointsRadius = 15.0f;
    [SerializeField] public float waypointStopDistance = 2f;

    [Header("Chasing Setting")]
    [SerializeField] protected float speedForChasing = 5f;  

    [Header("Detection Setting")]
    [SerializeField] protected float detectPlayerRadius = 10f;

    // When the player is croaching -> smaller detection radius     
    [SerializeField] protected float crouchDetectionRadius = 5f;
    [SerializeField] protected float detectionOffset = 1f;

    [Header("Animator")]
    [SerializeField] protected Animator animator;

    [Header("Transform")]
    [SerializeField] protected Transform player;

    protected NavMeshAgent agent;

  /*  // ANIMATIONS.
    public void IdleAnimationSet()
    {
        animator.SetBool("isPlayerSpotted", false);
    }
    public void ChaselAnimationSet()
    {
        animator.SetBool("isPlayerSpotted", true);
    }
*/
    // SIMPLE-VARIABLES
    // protected string playerTag = "Player"; // If gamemanager fails to find the player tag.
    //protected bool _isActive = true;
    protected AI_State currentState;
    protected AI_State prevState;
    protected float stateTimer;

    // UNITY METHODS.
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        GameObject playerTag = GameObject.FindWithTag("Player");
        if(playerTag == null) {
            return;
        }
        player = playerTag.transform;
        
        if(agent == null) {
            return;
        }
        agent.speed = patrolSpeed;  
    }

    protected virtual void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(AI_State newState)
    {
        if (currentState != null){
            currentState.Exit();
        }
        prevState = currentState;
        currentState = newState;
        currentState.Enter();
    }

    // OTHER METHODS
    public Transform GetPlayerT()
    {
        return player;
    }

    protected bool playerInRange(){

        if(player == null){
            return false;
        }
        // Is in range.
        float dist = Vector3.Distance(transform.position, player.position);
        return dist <= detectPlayerRadius;
    }

    // The player differentiate between the back and the front

    // Player in front? ->
    protected bool playerInFront()
    {
        if(player == null){
            return false;
        }

        // Direction ->
        Vector3 directionToThePlayer = (player.position - transform.position).normalized;
        
        // Angle -> 
        float angleP = Vector3.Angle(transform.forward,directionToThePlayer);
        return angleP <= 90f;

    }
    
    public void GoToThePosition(Vector3 posRef)
    {
        if(agent == null){
            return;
        }
        agent.SetDestination(posRef);
    }

    public void SetPatrolSpeed()
    {
        if(agent == null){
            return;
        }
        agent.speed = patrolSpeed;
    }

    protected void SetChaseSpeed()
    {
        if (agent == null) {
            return;
        }
        agent.speed =speedForChasing;

    }
    
    public Vector3 GoToTheFirstWayPoint(Vector3 current, float radius)
    {
    
    // Generate a random point near.
    Vector3 randomWaypointToReturn = current + Random.insideUnitSphere * radius;

    if (NavMesh.SamplePosition(randomWaypointToReturn, out NavMeshHit hit,2f,NavMesh.AllAreas))
        return hit.position;

    return current;
    }


    
    // Depends of the state of the player ->
    public float SetDetectionRadius(){

    if(IsPlayerCrouching())
    {
        return crouchDetectionRadius;
    }
    
    return detectPlayerRadius;
}
    public bool IsPlayerCrouching(){
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if(playerController._isCrouching) {
            return true;
        }
    return false;
    }

    public bool IsPlayerBehindObject()
    {
        if (player == null)
        {
            
         return false;
        }
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer))
        {
            if (hit.collider.gameObject != player.gameObject)
            {
                return true;
            }
        }
        
        return false;
    }

    public bool CanDetectPlayer()
    {
        if (player == null) {
            return false;
        }
        float currentDetectionRadius = SetDetectionRadius();
        bool inRange = Vector3.Distance(transform.position, player.position) <= currentDetectionRadius;
        bool inFront = playerInFront();
        bool notHidden = !IsPlayerBehindObject();
        
        return inRange && inFront && notHidden;
    }
}

