using UnityEngine;

// FRYENDLY ROBOT.
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

    [Header("Robot Arm Settings")]
    [SerializeField] private Vector3 armStartPos = new Vector3(0, -0.202f, 0); 
    [SerializeField] private Vector3 armFinalPos = new Vector3(0, 1.378f, 0);
    [SerializeField] private float armSpeed = 1.2f;

    [Header("Follow Mode Settings")]
    [SerializeField] private float followDistance = 3.1f;

    [Header("Puzzle Settings")]
    private bool isTheRobotActivated = false;
    private bool playerOnArm = false;

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
        if (robotArm != null)
        {
            robotArm.localPosition = armStartPos;
        }
        agent.isStopped = true;
    }
    public void FriendlyModeActivation()
    {
        isTheRobotActivated = true;
    }
    protected override void Update()
    {
        base.Update();
        if (!isTheRobotActivated)return;

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
        if(player == null)return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer > followDistance)
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
        if (robotArm == null) return;
        if (playerOnArm)
        {
            robotArm.localPosition = Vector3.MoveTowards(robotArm.localPosition, armFinalPos, armSpeed * Time.deltaTime);
        }
        else
        {
            robotArm.localPosition = Vector3.MoveTowards(robotArm.localPosition, armStartPos, armSpeed * Time.deltaTime);
        }
    }
    public void SetPlayerOnArm(bool v)
    {
        playerOnArm = v;
    }

}
