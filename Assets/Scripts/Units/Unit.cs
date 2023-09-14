using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Unit : NetworkBehaviour
{
    private static int ANIMATOR_PARAM_ATTACKING = Animator.StringToHash("Attacking");
    private static int ANIMATOR_PARAM_ATTACK = Animator.StringToHash("Attack");
    private static int ANIMATOR_PARAM_SPEED = Animator.StringToHash("Speed");

    [Header("References")]
    [SerializeField] private Targeter targeter;
    [SerializeField] private Health health;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer selector;
    [SerializeField] private GameObject deathPrefab;

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float chaseRange = 10.0f;

    [SyncVar] private float speed = 0f;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private int resourceCost = 10;

    private float attackTime = 0;
    private bool isAttacking = false;

    //Events
    public event Action OnSelected;
    public event Action OnDeselected;

    // Notify server when unit is spawned and despawned;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDeSpawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDeSpawned;

    public int GetResourceCost()
    {
        return resourceCost;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server
    public override void OnStartServer()
    {
        health.ServerOnDie += Die;
        ServerOnUnitSpawned.Invoke(this);
        GameHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= Die;
        ServerOnUnitDeSpawned?.Invoke(this);
        GameHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        speed = agent.velocity.magnitude;
        animator.SetFloat(ANIMATOR_PARAM_SPEED, speed);
        RpcSetSpeed(speed);

        if (ChaseTarget())
        {
            if (isAttacking)
            {
                if (!CanFireAtTarget())
                {
                    DisengageAttack();
                    return;
                }

                Targetable target = targeter.GetTarget();

                Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                attackTime += Time.deltaTime;

                if (attackTime >= attackSpeed)
                {
                    attackTime = 0f;
                    Attack(target);
                }
            }
        }
        else
        {
            CheckRange();
        }
    }

    [Server]
    private bool ChaseTarget()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) 
        {
            if (isAttacking)
            {
                DisengageAttack();
            }
            return false; 
        }


        if (BSCUtil.OutOfRange(target.transform.position, transform.position, attackRange))
        {
            DisengageAttack();
            agent.SetDestination(target.transform.position);

        }
        else
        {

            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            EngageAttack();
        }

        return true;
    }

    [Server]
    private void CheckRange()
    {
        if (!agent.hasPath) { return; }
        if (agent.remainingDistance > agent.stoppingDistance) { return; }
        agent.ResetPath();
    }


    [Server]
    public void EngageAttack()
    {
        if (isAttacking) { return; }

        isAttacking = true;
        attackTime = attackSpeed;
        RpcSetCombat(true);
    }

    [Server]
    public void DisengageAttack()
    {
        isAttacking = false;
        RpcSetCombat(false);
    }


    [Server]
    private bool CanFireAtTarget()
    {
        if (targeter.GetTarget() == null)
        {
            return false;
        }

        bool result = BSCUtil.InRange(targeter.GetTarget().transform.position, transform.position, attackRange);

        return result;
    }

    [Server]
    private void Attack(Targetable target)
    {
        if (target.TryGetComponent(out Health health))
        {
            health.DealDamage(attackDamage);
        }

        RpcAttack();
    }

    [Server]
    private void Die()
    {
        GameObject unitInstance = Instantiate(deathPrefab, transform.position, transform.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        FindObjectOfType<UnitSelectionHandler>().RemoveUnitFromSelection(this);

        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
        targeter.ClearTarget();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        DisengageAttack();
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !isOwned) { return; }

        AuthorityOnUnitDeSpawned.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!isOwned) { return; }
        selector.enabled = true;
        OnSelected?.Invoke();
    }

    [Client]
    public void DeSelect()
    {
        if (!isOwned) { return; }
        selector.enabled = false;
        OnDeselected?.Invoke();
    }

    [ClientRpc]
    private void RpcSetSpeed(float newSpeed)
    {
        speed = newSpeed;
        animator.SetFloat(ANIMATOR_PARAM_SPEED, speed);
    }

    [ClientRpc]
    private void RpcAttack()
    {
        animator.SetTrigger(ANIMATOR_PARAM_ATTACK);
    }

    [ClientRpc]
    private void RpcSetCombat(bool isAttacking)
    {
        this.isAttacking = isAttacking;
        animator.SetBool(ANIMATOR_PARAM_ATTACKING, isAttacking);
    }
    #endregion
}
