using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour, IEnemy
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackDistance = 1.3f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private DamageType damageType;

    private NavMeshAgent navMeshAgent;
    private GameObject player;
    private bool isAttacking = false;

    private enum EnemyState { Idle, Chase, Attack }
    private EnemyState currentState = EnemyState.Idle;

    #region IEnemy

    public void Attack(GameObject target, DamageType type)
    {
        if (target == null) return;

        RaycastHit hitInfo;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out hitInfo, attackDistance, 1 << 3))
        {
            if (hitInfo.transform.gameObject == target)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.ApplyDamage(new DamageRequest(
                        damageAmount,
                        gameObject,
                        target.transform.position,
                        hitInfo.normal,
                        type));
                }
            }
        }
    }

    // Resumes NavMeshAgent movement toward last set destination
    public void Move()
    {
        navMeshAgent.isStopped = false;
    }

    // Sets the NavMeshAgent destination to the target's position
    public void MoveTo(GameObject target)
    {
        if (target == null) return;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.transform.position);
    }

    #endregion

    #region MB

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        UpdateState();
        HandleState();
    }

    #endregion

    #region State Machine

    private void UpdateState()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attackDistance)
            currentState = EnemyState.Attack;
        else if (distanceToPlayer <= detectionRange)
            currentState = EnemyState.Chase;
        else
            currentState = EnemyState.Idle;
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                navMeshAgent.isStopped = true;
                break;

            case EnemyState.Chase:
                MoveTo(player);
                break;

            case EnemyState.Attack:
                navMeshAgent.isStopped = true;
                // Face the player before attacking
                transform.LookAt(player.transform.position);
                if (!isAttacking)
                    StartCoroutine(AttackRoutine());
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        Attack(player, damageType);
        yield return new WaitForSeconds(1f / attackRate);
        isAttacking = false;
    }

    #endregion
}