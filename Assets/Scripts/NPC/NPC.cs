using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIType
{
    Passive,
    Scared,
    Aggressive
}

public enum AIState
{
    Idle,
    Wandering,
    Attacking,
    Fleeing
}

public class NPC : MonoBehaviour, IDamagable
{
    public NPCData data;

    [Header("Stats")]
    public int health;
    public float walkSpeed;
    public float runSpeed;
    public ItemData[] dropOnDeath;

    [Header("AI")]
    public AIType aiType;
    public AIState aiState;
    public float detectDistance;
    public float safeDistance;

    [Header("Wandering")]
    public float minWanderDistance;
    public float maxWanderDistance;
    public float minWanderWaitTime;
    public float maxWanderWaitTime;

    [Header("Combat")]
    public int damage;
    public float attackRate;
    private float _lastAttackTime;
    public float attackDistance;

    private float _playerDistance;

    [HideInInspector]
    public NavMeshAgent agent;
    private Animator _anim;
    private SkinnedMeshRenderer[] _meshRenderers;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();

        _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        SetState(AIState.Wandering);
    }

    private void Update()
    {
        _playerDistance = Vector3.Distance(transform.position, PlayerController.instance.transform.position);

        _anim.SetBool("Moving", aiState != AIState.Idle);

        switch(aiState)
        {
            case AIState.Idle: PassiveUpdate(); break;
            case AIState.Wandering: PassiveUpdate(); break;
            case AIState.Attacking: AttackingUpdate(); break;
            case AIState.Fleeing: FleeingUpdate(); break;
        }
    }

    private void PassiveUpdate()
    {
        if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        if(aiType == AIType.Aggressive && _playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
        else if(aiType == AIType.Scared && _playerDistance < detectDistance)
        {
            SetState(AIState.Fleeing);
            agent.SetDestination(GetFleeLocation());
        }
    }

    private void AttackingUpdate()
    {
        if(_playerDistance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerController.instance.transform.position);
        }
        else
        {
            agent.isStopped = true;

            if(Time.time - _lastAttackTime > attackRate)
            {
                _lastAttackTime = Time.time;
                PlayerController.instance.GetComponent<IDamagable>().TakePhysicalDamage(damage);
                _anim.SetTrigger("Attack");
            }
        }
    }

    private void FleeingUpdate()
    {
        if(_playerDistance < safeDistance && agent.remainingDistance < 0.1f)
        {
            agent.SetDestination(GetFleeLocation());
        }
        else if(_playerDistance > safeDistance)
        {
            SetState(AIState.Wandering);
        }
    }

    private void SetState(AIState newState)
    {
        aiState = newState;

        switch(aiState)
        {
            case AIState.Idle:
            {
                agent.speed = walkSpeed;
                agent.isStopped = true;
                break;
            }
            case AIState.Wandering:
            {
                agent.speed = walkSpeed;
                agent.isStopped = false;
                break;
            }
            case AIState.Attacking:
            {
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
            }
            case AIState.Fleeing:
            {
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
            }
        }
    }

    private void WanderToNewLocation()
    {
        if(aiState != AIState.Idle)
            return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    private Vector3 GetWanderLocation()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;

        while(Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

            i++;

            if(i == 30)
                break;
        }

        return hit.position;
    }

    private Vector3 GetFleeLocation()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, safeDistance, NavMesh.AllAreas);

        int i = 0;

        while(GetDestinationAngle(hit.position) > 90 || _playerDistance < safeDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, safeDistance, NavMesh.AllAreas);

            i++;

            if(i == 30)
                break;
        }

        return hit.position;
    }

    private float GetDestinationAngle(Vector3 targetPos)
    {
        return Vector3.Angle(transform.position - PlayerController.instance.transform.position, transform.position + targetPos);
    }

    public void TakePhysicalDamage(int damageAmount)
    {
        health -= damageAmount;

        if(health <= 0)
            Die();

        StartCoroutine(DamageFlash());

        if(aiType == AIType.Passive)
            SetState(AIState.Fleeing);
    }

    private void Die()
    {
        for(int x = 0; x < dropOnDeath.Length; x++)
        {
            Instantiate(dropOnDeath[x].dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        for(int x = 0; x < _meshRenderers.Length; x++)
            _meshRenderers[x].material.color = new Color(1.0f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.1f);

        for(int x = 0; x < _meshRenderers.Length; x++)
            _meshRenderers[x].material.color = Color.white;
    }
}