using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : NetworkBehaviour
{

    private static int ANIMATOR_PARAM_ATTACKING = Animator.StringToHash("Attacking");
    private static int ANIMATOR_PARAM_ATTACK = Animator.StringToHash("Attack");

    [SerializeField] private Targeter targeter;
    //[SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRange;
    [Tooltip("Attack in seconds")]
    [SerializeField] private float attackSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Animator animator;

    private float lastAttack;
    private bool isAttacking = false;

    #region Server

    [Server]
    public void EngageAttack()
    {
        isAttacking = true;
        animator.SetBool(ANIMATOR_PARAM_ATTACKING, true);
        RpcSetCombat(true);
    }

    [Server]
    public void DisengageAttack()
    {
        isAttacking = false;
        animator.SetBool(ANIMATOR_PARAM_ATTACKING, false);
        RpcSetCombat(false);
    }

    [ServerCallback]
    private void Update()
    {
        if (isAttacking)
        {
            if (!CanFireAtTarget())
            {

                DisengageAttack();
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targeter.GetTarget().transform.position - transform.position);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Time.time > lastAttack + attackSpeed)
            {
                Attack();
                lastAttack = Time.time;
            }
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        if (targeter.GetTarget() == null)
        {
            return false;
        }

        return BSCUtil.InRange(targeter.GetTarget().transform.position, transform.position, attackRange);
    }

    [Server]
    private void Attack()
    {
        animator.SetTrigger(ANIMATOR_PARAM_ATTACK);
        RpcAttack();
    }

    #endregion

    #region Client

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
