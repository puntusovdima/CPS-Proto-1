using UnityEngine;

public class PatrollingState : AI_State
{
    public PatrollingState(Base_AI ai) : base(ai) { }
    private int waypointsRemaining = 0;

    public override void Enter()
    {
        Ai.SetPatrolSpeed();
       // Ai.IdleAnimationSet();
    }

    public override void Update()
    {
        // Can detect Player -> new Chase State.
        if (Ai.CanDetectPlayer())
        {
            Ai.ChangeState(new ChasingState(Ai));
            return;
        }

        // AI -> Using waypoints?.
        if (Ai.useWaypoints)
        {
            // Player transform -> Moved to the position.
            Transform targetP = Ai.wayPoints[waypointsRemaining];
            Ai.GoToThePosition(targetP.position); // call funct.

            float dtc = Vector3.Distance(Ai.transform.position, targetP.position);
            if (dtc <Ai.waypointStopDistance)
            {
                waypointsRemaining = (waypointsRemaining + 1) % Ai.wayPoints.Length;
            }
        }
        else
        {
            Vector3 nextPos = Ai.GoToTheFirstWayPoint(Ai.transform.position, Ai.waypointsRadius);
            Ai.GoToThePosition(nextPos);
        }
        
    }

    public override void Exit()
    {
    }
}
