using Pathfinding;
using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    protected EnemyBaseState _currentState;
    public EnemyBaseState CurrentState { get => _currentState; set => _currentState = value; }

    protected float _hitDirection;

    [SerializeField] protected string _enemyName;
    public string Name { get => _enemyName; }

    [SerializeField] protected float _detectionDistance = 10f;
    public float DetectionDistance { get => _detectionDistance; }

    [SerializeField] protected Animator _animator;
    public Animator Animator { get => _animator; }

    protected float _health;
    public float CurrentHealth { get => _health; }

    [SerializeField] private float _damage = 5f;
    public float Damage { get => _damage; }

    [SerializeField] protected float _maxHealth = 100f;
    public float MaxHealth { get => _maxHealth; }

    protected AIPath _ai;
    public AIPath AI { get => _ai; }

    protected RVOController _rvo;
    public RVOController RVO { get => _rvo; }


    [SerializeField] protected GameObject _target;

    protected CapsuleCollider _collider;
    public CapsuleCollider Collider { get => _collider; }

    protected bool _isAttacking;
    public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }

    public event Action OnEnemyAttack;

    private Transform _playerTransform;

    [SerializeField] private LayerMask _obstacleLayers;

    [SerializeField] private GameObject _bloodAttach;
    [SerializeField] private GameObject[] _bloodFX;
    private int _effectIdx;

    

    public void Init()
    {
        if (_ai != null) _ai.onSearchPath += Update;
        _health = _maxHealth;
    }

    public void OnAttack()
    {
        OnEnemyAttack?.Invoke();
    }

    private void Awake()
    {
        _ai = GetComponent<AIPath>();
        _rvo = GetComponent<RVOController>();
        _collider = GetComponent<CapsuleCollider>();
        _playerTransform = Ash_FP_PlayerController.Instance.transform;
        
        OnAwake();
    }

    protected void Death()
    {
        _animator.SetTrigger("Dead");
        _collider.enabled = false;
        _ai.onSearchPath -= Update;
    }

    private void Update()
    {
        if(_currentState != null)
        {
            _currentState.UpdateState();
        }
        
        OnUpdate();
    }

    protected abstract void OnAwake();
    protected abstract void OnUpdate();

    public abstract void TakeDamage(float damage, Vector3 direction, RaycastHit hit, GameObject source);

    /// <summary>
    /// Turn attack direction into a float that can be used in animator
    /// </summary>
    protected float DirectionToFloat(Vector3 dir)
    {
        float d = Vector3.Dot(transform.forward, dir);
        if(d > 0.5f)
        {
            return 0;
        }
        else if(d < -0.5f)
        {
            return 3;
        }
        d = Vector3.Dot(transform.right, dir);
        if (d > 0.5f)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    public bool QueryPlayerInLOS()
    {
        Ray ray = new Ray(transform.position + _collider.center, _playerTransform.position - transform.position);

        return !Physics.Raycast(ray, Vector3.Distance(transform.position, _playerTransform.position), _obstacleLayers);
    }

    public bool QueryPlayerInView()
    {
        return Vector3.Dot(transform.forward, _playerTransform.position - transform.position) > 0.5f && Vector3.Distance(transform.position, _playerTransform.position) <= _detectionDistance;
    }

    public bool IsPlayerInDistanceThreshold(float threshold)
    {
        return Vector3.Distance(transform.position, Ash_FP_PlayerController.Instance.transform.position) <= threshold;
    }

    protected Transform GetNearestObject(Transform hit, Vector3 hitPos)
    {
        float closestPos = 100f;
        Transform closestBone = null;
        Transform[] childs = hit.GetComponentsInChildren<Transform>();

        foreach (Transform child in childs)
        {
            float dist = Vector3.Distance(child.position, hitPos);
            if (dist < closestPos)
            {
                closestPos = dist;
                closestBone = child;
            }
        }

        float distRoot = Vector3.Distance(hit.position, hitPos);
        if (distRoot < closestPos)
        {
            closestPos = distRoot;
            closestBone = hit;
        }
        return closestBone;
    }

    protected void SpawnBloodDecals(RaycastHit hit, Vector3 direction)
    {
        float angle = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg + 180;

        if (_effectIdx == _bloodFX.Length) _effectIdx = 0;

        GameObject instance = Instantiate(_bloodFX[_effectIdx], hit.point, Quaternion.Euler(0, angle + 90, 0));
        _effectIdx++;

        BFX_BloodSettings settings = instance.GetComponent<BFX_BloodSettings>();
        settings.LightIntensityMultiplier = 1;

        Transform nearestBone = GetNearestObject(transform, hit.point);
        if (nearestBone != null)
        {
            GameObject attachBloodInstance = Instantiate(_bloodAttach);
            Transform bloodT = attachBloodInstance.transform;
            bloodT.position = hit.point;
            bloodT.localRotation = Quaternion.identity;
            bloodT.localScale = Vector3.one * UnityEngine.Random.Range(0.75f, 1.2f);
            bloodT.LookAt(hit.point + hit.normal, direction);
            bloodT.Rotate(90, 0, 0);
            bloodT.transform.parent = nearestBone;
            Destroy(attachBloodInstance, 20);
        }

        Destroy(instance, 20);
    }
    
}
