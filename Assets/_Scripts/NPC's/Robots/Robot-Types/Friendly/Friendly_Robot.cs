using System.Collections;
using UnityEngine;

public class Friendly_Robot : Base_AI
{
    private enum FriendlyRobotModes
    {
        FollowPlayer,
        HelpPlayer
    }

    [Header("Friendly Robot Settings")]
    [SerializeField] private FriendlyRobotModes robotMode = FriendlyRobotModes.HelpPlayer;
    [SerializeField] private Transform helpPoint;
    [SerializeField] private Transform robotArm;

    [Header("Friendly Robot Combat Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float chaseEnemyRobotRange = 2f;

    [Header("Robot Arm Settings")]
    [SerializeField] private Vector3 armStartPos = new Vector3(0, -0.202f, 0);
    [SerializeField] private Vector3 armFinalPos = new Vector3(0, 1.378f, 0);
    [SerializeField] private float armSpeed = 1.2f;
    [SerializeField] private float timeToDestroyRobotArm = 1.5f;

    [Header("Follow Mode Settings")]
    [SerializeField] private float followDistance = 3.1f;

    [Header("Puzzle Settings")] 
    [SerializeField] private PuzzleDoorBehaviour puzzleDoor;
    
    private bool isTheRobotActivated = false;
    private bool playerOnArm = false;
    private bool armReachedMax = false;
    private bool isChasingEnemy = false;
    private bool hasReachedEnemy = false;
    private float enemyAttackTimer = 0f;

    protected override void Start()
    {
        base.Start();
        if (robotArm != null)
        {
            robotArm.localPosition = armStartPos;
        }
    }

    public void ResetRobot()
    {
        isTheRobotActivated = false;
        playerOnArm = false;
        armReachedMax = false;
        robotMode = FriendlyRobotModes.HelpPlayer;
        if (robotArm != null)
        {
            robotArm.localPosition = armStartPos;
        }
        agent.isStopped = true;
    }

    public void FriendlyModeActivation()
    {
        isTheRobotActivated = true;
        armReachedMax = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!isTheRobotActivated && PuzzleManager.Instance != null && PuzzleManager.Instance.IsPuzzleSolved())
        {
            isTheRobotActivated = true;
            armReachedMax = false;
        }

        if (!isTheRobotActivated)
        {
            return;
        }

        switch (robotMode)
        {
            case FriendlyRobotModes.FollowPlayer:
                FollowPlayerLogic();
                break;
            case FriendlyRobotModes.HelpPlayer:
                HelpPlayer();
                break;
        }
    }

    private void FollowPlayerLogic()
    {
        if (player == null)
        {
            return;
        }

        GameObject enemyT = GameObject.FindGameObjectWithTag("Enemy");

        if (enemyT != null)
        {
            float distToEnemy = Vector3.Distance(transform.position, enemyT.transform.position);

            if (hasReachedEnemy && enemyAttackTimer > 0f)
            {
                enemyAttackTimer -= Time.deltaTime;
                if (enemyAttackTimer <= 0f)
                {
                    Destroy(enemyT);
                    puzzleDoor.OpenDoor();
                    Destroy(gameObject);
                    hasReachedEnemy = false;
                    enemyAttackTimer = 0f;
                }
                return;
            }

            if (distToEnemy <= 1f)
            {
                if (!hasReachedEnemy)
                {
                    hasReachedEnemy = true;
                    enemyAttackTimer = 0.5f;
                }
                return;
            }

            hasReachedEnemy = false;
            enemyAttackTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(enemyT.transform.position);
            return;
        }

        isChasingEnemy = false;
        enemyAttackTimer = 0f;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void HelpPlayer()
    {
        if (robotArm == null)
        {
            return;
        }

        if (playerOnArm && !armReachedMax)
        {
            Vector3 armPrevPos = robotArm.localPosition;
            robotArm.localPosition = Vector3.MoveTowards(robotArm.localPosition, armFinalPos, armSpeed * Time.deltaTime);
            Vector3 armDeltaLocal = robotArm.localPosition - armPrevPos;
            Vector3 armDeltaWorld = robotArm.parent != null
                ? robotArm.parent.TransformDirection(armDeltaLocal)
                : armDeltaLocal;

            if (PlayerController.Instance != null)
            {
                CharacterController cc = PlayerController.Instance.GetComponent<CharacterController>();
                Vector3 newPos = PlayerController.Instance.transform.position + armDeltaWorld;
                cc.enabled = false;
                PlayerController.Instance.transform.position = newPos;
                cc.enabled = true;
            }

            if (Vector3.Distance(robotArm.localPosition, armFinalPos) < 0.01f)
            {
                StartCoroutine(DestroyRobotArm());
            }
        }
        else if (!armReachedMax)
        {
            robotArm.localPosition = Vector3.MoveTowards(robotArm.localPosition, armStartPos, armSpeed * Time.deltaTime);
        }
    }

    private IEnumerator DestroyRobotArm()
    {
        armReachedMax = true;
        robotMode = FriendlyRobotModes.FollowPlayer;
        playerOnArm = false;
        agent.isStopped = false;
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
        yield return new WaitForSeconds(timeToDestroyRobotArm);
        Destroy(robotArm.gameObject);
    }

    public void SetPlayerOnArm(bool v)
    {
        playerOnArm = v;
    }
}
