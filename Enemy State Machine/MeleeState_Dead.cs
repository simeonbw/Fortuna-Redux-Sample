using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeState_Dead : EnemyBaseState
{
    public MeleeState_Dead(EnemyBase context) : base(context) { }

    public override void EnterState()
    {
        _context.AI.canMove = false;
        _context.AI.enabled = false;
        _context.RVO.enabled = false;
    }

    public override void UpdateState() { }

    public override void ExitState() { }

    protected override void CheckSwitchState() { }
}
