using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    private static int ANIMATOR_PARAM_SPEED = Animator.StringToHash("Speed");

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange = 10.0f;

    [SyncVar] private float speed = 0f;

    #region Server

    
    [ServerCallback]
    private void Update()
    {
        speed = agent.velocity.magnitude;
        animator.SetFloat(ANIMATOR_PARAM_SPEED, speed); 
        RpcSetSpeed(speed);

        if (!ChaseTarget())
        {
            CheckRange();
        }
    }

    private bool ChaseTarget()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return false; }

        if((target.transform.position - transform.position).sqrMagnitude > (chaseRange * chaseRange))
        {
            agent.SetDestination(target.transform.position);
            //transform.LookAt(target.transform);
        }
        else if(agent.hasPath)
        {
            agent.ResetPath();
        }

        return true;
    }

    private void CheckRange()
    {
        if (!agent.hasPath) { return; } 
        if (agent.remainingDistance > agent.stoppingDistance) { return; }
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget();
        
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcSetSpeed(float newSpeed)
    {
        speed = newSpeed;
        animator.SetFloat(ANIMATOR_PARAM_SPEED, speed); 
    }

    #endregion
}
