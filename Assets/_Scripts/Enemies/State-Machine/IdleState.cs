using UnityEngine;

public class IdleState : AI_State
{
    public IdleState(Base_AI ai) : base(ai) { }
    public override void Enter()
    {
        //Ai.IdleAnimationSet();
        Ai.SetPatrolSpeed();
    }
    public override void Update()
    {
        // Detect Player? -> Chase.
        if (Ai.CanDetectPlayer())
        {
            Ai.ChangeState(new ChasingState(Ai));
        }
    }
    public override void Exit()
    {
    }
}
