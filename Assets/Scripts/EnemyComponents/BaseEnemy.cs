using UnityEngine;

public class BaseEnemy : MonoBehaviour, IEnemy
{

    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackDistance = 1.3f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float damageAmount = 10f;

    private UnityEngine.AI.NavMeshAgent navMeshAgent;

    #region IEnemy

    public void Attack(GameObject target, DamageType type)
    {
        if (target == null)
        {
            return;
        }

        RaycastHit hitInfo;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out hitInfo, attackDistance, 1 << 3))
        {
            if (hitInfo.transform.gameObject == target)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.ApplyDamage(new DamageRequest(damageAmount, gameObject,
                        target.transform.position, hitInfo.normal, type));
                }
            }
        }
    }

    public void Move()
    {
        throw new System.NotImplementedException();
    }

    public void MoveTo(GameObject target)
    {
        navMeshAgent.Move(target.transform.position);
    }

    #endregion

    #region MB
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion
}
