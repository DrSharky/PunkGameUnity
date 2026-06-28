using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour, IEnemy
{
    [Header("Combat")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackDistance = 1.3f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private DamageType damageType;

    [Header("Detection / Movement")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float destinationUpdateInterval = 0.2f;

    [Header("Jumping")]
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private bool useManualOffMeshJump = true;

    private NavMeshAgent navMeshAgent;
    private GameObject player;
    private bool isAttacking = false;
    private bool isJumping = false;
    private float destinationUpdateTimer = 0f;

    private enum EnemyState { Idle, Chase, Attack }
    private EnemyState currentState = EnemyState.Idle;

    public int Health { get { return health; } }
    private int health { get; set; }

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

    public void Move()
    {
        if (isJumping) return;
        navMeshAgent.isStopped = false;
    }

    public void MoveTo(GameObject target)
    {
        if (target == null || isJumping) return;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.transform.position);
    }

    #endregion

    #region MB

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");

        if (useManualOffMeshJump)
        {
            navMeshAgent.autoTraverseOffMeshLink = false;
        }
    }

    private void Update()
    {
        if (navMeshAgent == null || player == null) return;

        if (useManualOffMeshJump && !isJumping && navMeshAgent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossOffMeshLink());
            return;
        }

        if (isJumping) return;

        UpdateState();
        HandleState();
    }

    #endregion

    #region State Machine

    private void UpdateState()
    {
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
                destinationUpdateTimer += Time.deltaTime;
                if (destinationUpdateTimer >= destinationUpdateInterval)
                {
                    MoveTo(player);
                    destinationUpdateTimer = 0f;
                }

                SmoothLookAt(player.transform.position);
                break;

            case EnemyState.Attack:
                navMeshAgent.isStopped = true;
                SmoothLookAt(player.transform.position);

                if (!isAttacking)
                    StartCoroutine(AttackRoutine());
                break;
        }
    }

    private void SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        Attack(player, damageType);
        yield return new WaitForSeconds(1f / attackRate);
        isAttacking = false;
    }

    #endregion

    #region Jumping

    private IEnumerator JumpAcrossOffMeshLink()
    {
        isJumping = true;
        navMeshAgent.isStopped = true;

        OffMeshLinkData linkData = navMeshAgent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * navMeshAgent.baseOffset;

        Vector3 flatDirection = endPos - startPos;
        flatDirection.y = 0f;
        if (flatDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(flatDirection.normalized);
        }

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;

            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
            float arc = 4f * jumpHeight * t * (1f - t); // parabola
            Vector3 nextPos = horizontal + Vector3.up * arc;

            navMeshAgent.Warp(nextPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.Warp(endPos);
        navMeshAgent.CompleteOffMeshLink();
        navMeshAgent.isStopped = false;
        isJumping = false;
    }

    #endregion
}