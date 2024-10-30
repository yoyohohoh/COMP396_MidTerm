using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCStateBase
{
    protected FSMController_YH fsm;

    public NPCStateBase(FSMController_YH fsm)
    {
        this.fsm = fsm;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
}




