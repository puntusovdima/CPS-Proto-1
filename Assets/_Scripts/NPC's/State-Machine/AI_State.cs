using UnityEngine;

// BASE FOR THE STATES -> STATE PATTERN BASE / STATE MACHINE.
public abstract class AI_State
{
    protected Base_AI Ai;
    public AI_State(Base_AI ai)
    {
        this.Ai = ai;
    }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}