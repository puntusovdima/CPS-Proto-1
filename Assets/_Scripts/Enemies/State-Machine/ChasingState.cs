using System;
using UnityEngine;

public class ChasingState : AI_State
{
    public ChasingState(Base_AI ai) : base(ai) { }
        public override void Enter()
    {
        // Ai.IdleAnimationSet();
        //Ai.ChaselAnimationSet();
    }

    public override void Update()
    {
        // Cant detect Player? -> Patroll State.
        if (!Ai.CanDetectPlayer())
        {
            Ai.ChangeState(new PatrollingState(Ai));
            return;
        }

        // Else -> Keep Chasing.
        Ai.GoToThePosition(Ai.GetPlayerT().position);
    }
    public override void Exit()
    {
    }
}