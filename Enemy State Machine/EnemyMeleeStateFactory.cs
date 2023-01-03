using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyMeleeStateFactory
{
    public static MeleeState_AttackEquipment AttackEquipment(EnemyBase context, Equipment target)
    {
        return new MeleeState_AttackEquipment(context, target);
    }

    public static MeleeState_AttackPlayer AttackPlayer(EnemyBase context)
    {
        return new MeleeState_AttackPlayer(context);
    }

    public static MeleeState_Dead Dead(EnemyBase context)
    {
        return new MeleeState_Dead(context);
    }
}
