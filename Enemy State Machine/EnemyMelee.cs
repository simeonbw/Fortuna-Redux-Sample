using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : EnemyBase
{
    public static event Action<EnemyMelee> OnMeleeEnemyKilled;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField] private AudioClip _onLandClip;

    public void OnAttackEnd()
    {
        _isAttacking = false;
    }

    public override void TakeDamage(float damage, Vector3 direction, RaycastHit hit, GameObject source)
    {
        _animator.SetFloat("HitDirection", DirectionToFloat(-direction));
        _animator.SetTrigger("Hit");

        _health = Mathf.Clamp(_health - damage, 0, _maxHealth);

        if (_health == 0)
        {
            Death();
            _currentState.SwitchState(EnemyMeleeStateFactory.Dead(this));
            OnMeleeEnemyKilled?.Invoke(this);
        }

        _target = source;

        SpawnBloodDecals(hit, direction);
    }

    protected override void OnAwake()
    {
        
    }

    protected override void OnUpdate()
    {
        if (transform.position.y <= -20)
        {
            Death();
            _currentState.SwitchState(EnemyMeleeStateFactory.Dead(this));
            OnMeleeEnemyKilled?.Invoke(this);
            enabled = false;
        }
    }

    public void OnLand()
    {
        _audioSource.clip = _onLandClip;
        _audioSource.Play();

        Equipment e = Architect.Instance.ActiveRoom.GetClosestEquipment(transform.position);

        if (e == null)
        {
            _currentState.SwitchState(EnemyMeleeStateFactory.AttackPlayer(this));
        }
        else
        {
            _currentState.SwitchState(EnemyMeleeStateFactory.AttackEquipment(this, e));
        }
    }

    public void Footstep()
    {
        _audioSource.clip = _footstepClips[UnityEngine.Random.Range(0, _footstepClips.Length)];
        _audioSource.Play();
    }
}
