using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeState_AttackPlayer : EnemyBaseState
{
    private float _attackRange = 1f;
    private float _attackInterval = 1f;
    private float _currentTime;

    public MeleeState_AttackPlayer(EnemyBase context) : base(context)
    {
        
    }

    public override void EnterState()
    {
        _currentTime = 0f;
        _context.OnEnemyAttack += OnAttack;
    }

    /// <summary>
    /// Called when animation event triggers to check if player is hit
    /// </summary>
    private void OnAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(_context.transform.position + _context.transform.forward, 1f, LayerMask.GetMask("Player"));
        
        if (colliders.Length != 0 && colliders[0].TryGetComponent(out IDamageable dam))
        {
            dam.TakeDamage(_context.Damage, _context.transform.forward, new RaycastHit(), _context.gameObject);
        }
    }

    public override void UpdateState()
    {
        _currentTime = Mathf.Clamp(_currentTime + Time.deltaTime, 0, _attackInterval);

        if(Vector3.Distance(_context.transform.position, Ash_FP_PlayerController.Instance.transform.position) <= _attackRange && !_context.IsAttacking)
        {
            Vector3 dir = (Ash_FP_PlayerController.Instance.transform.position - _context.transform.position).normalized;
            if (Vector3.Dot(dir, _context.transform.forward) > 0.5f)
            {
                _context.AI.isStopped = true;
                _context.AI.enableRotation = false;
                _context.Animator.SetTrigger("Claw");
                _context.IsAttacking = true;
                _context.RVO.locked = true;
            }
            else
            {
                _context.transform.rotation = Quaternion.RotateTowards(_context.transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.deltaTime * 2);
            }
        }
        else if (!_context.IsAttacking)
        {
            if (_context.AI.isStopped)
            {
                _context.AI.isStopped = false;
                _context.AI.enableRotation = true;
                _context.RVO.locked = false;
            }
            _context.AI.destination = Ash_FP_PlayerController.Instance.transform.position;
        }

        _context.Animator.SetFloat("MovementSpeed", _context.AI.velocity.magnitude);
    }

    public override void ExitState()
    {
        _context.OnEnemyAttack -= OnAttack;
    }

    protected override void CheckSwitchState()
    {
        if (!_context.IsPlayerInDistanceThreshold(3f))
        {
            Equipment e = Architect.Instance.ActiveRoom.GetClosestEquipment(_context.transform.position);

            if (e != null)
            {
                SwitchState(EnemyMeleeStateFactory.AttackEquipment(_context, e));
            }
        }
    }
}
