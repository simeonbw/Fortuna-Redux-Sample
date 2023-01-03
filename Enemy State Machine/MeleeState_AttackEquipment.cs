using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeState_AttackEquipment : EnemyBaseState
{
    private Equipment _target;
    private Transform _attackPoint;

    private bool _reachedEquipment;

    private float _currentAttackTime;
    private float _attackInterval = 1f;
    public MeleeState_AttackEquipment(EnemyBase context, Equipment target) : base(context)
    {
        _target = target;
    }

    public override void EnterState()
    {
        if(!_target.OccupyAvailableAttackPoint(out _attackPoint))
        {
            SwitchState(EnemyMeleeStateFactory.AttackPlayer(_context));
            return;
        }

        _context.OnEnemyAttack += OnAttack;

        _context.AI.destination = _attackPoint.position;

        _reachedEquipment = false;
        _currentAttackTime = 0f;
    }

    private void OnAttack()
    {
        _target.Damageable.TakeDamage(_context.Damage, _context.transform.forward, new RaycastHit(), _context.gameObject);
    }

    public override void UpdateState()
    {
        _context.Animator.SetFloat("MovementSpeed", _context.AI.velocity.magnitude);

        if (_context.AI.reachedDestination && !_reachedEquipment)
        {
            _reachedEquipment = true;
        }

        if (_reachedEquipment)
        {
            Quaternion rot = Quaternion.LookRotation(_attackPoint.forward);
            _context.transform.rotation = Quaternion.RotateTowards(_context.transform.rotation, Quaternion.Euler(0, rot.eulerAngles.y, 0), 1f);
            _currentAttackTime = Mathf.Clamp(_currentAttackTime + Time.deltaTime, 0, _attackInterval);

            if(_currentAttackTime == _attackInterval)
            {
                _context.Animator.SetTrigger("Claw");

                _currentAttackTime = 0f;
            }
        }

        CheckSwitchState();
    }

    public override void ExitState()
    {
        _context.OnEnemyAttack -= OnAttack;
    }

    protected override void CheckSwitchState()
    {
        if (_context.IsPlayerInDistanceThreshold(3f))
        {
            SwitchState(EnemyMeleeStateFactory.AttackPlayer(_context));
        }
        else if(_target.Damageable.CurrentHealth == 0)
        {
            Equipment e = Architect.Instance.ActiveRoom.GetClosestEquipment(_context.transform.position);

            if(e == null)
            {
                SwitchState(EnemyMeleeStateFactory.AttackPlayer(_context));
            }
            else
            {
                 SwitchState(EnemyMeleeStateFactory.AttackEquipment(_context, e));
            }
        }
    }
}
