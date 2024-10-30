using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : NPCStateBase
{
    public WanderState(FSMController_YH fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.GetComponent<FSMController_YH>().ChangeColor(Color.green);
        fsm.StartCoroutine(fsm.GetComponent<FSMController_YH>().WanderToPatrol());
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        fsm.GetComponent<FSMController_YH>().HandleWander();
    }
}

public class PatrolState : NPCStateBase
{
    public PatrolState(FSMController_YH fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.GetComponent<FSMController_YH>().ChangeColor(new Color(1.0f, 0.75f, 0.8f));
        fsm.StartCoroutine(fsm.GetComponent<FSMController_YH>().PatrolToChase());
    }

    public override void Exit() 
    {
        fsm.GetComponent<FSMController_YH>().HandlePatrol(false);
    }

    public override void Update()
    {
        fsm.GetComponent<FSMController_YH>().HandlePatrol(true);
        
    }
}

public class ChaseState : NPCStateBase
{
    public ChaseState(FSMController_YH fsm) : base(fsm) { }

    public override void Enter() 
    {
        fsm.GetComponent<FSMController_YH>().ChangeColor(Color.red);
    }

    public override void Exit() { }

    public override void Update()
    {
        fsm.GetComponent<FSMController_YH>().HandleChase();
    }
}

public class EvadeState : NPCStateBase
{
    public EvadeState(FSMController_YH fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.GetComponent<FSMController_YH>().ChangeColor(Color.yellow);
    }

    public override void Exit()
    {

    }

    public override void Update() 
    {
        fsm.GetComponent<FSMController_YH>().HandleEvade();
    }
}



