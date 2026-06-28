using System;
using UnityEngine;

namespace PunkGame.Spawning
{
    /// <summary>
    /// Lightweight death detector for enemies.
    /// This avoids requiring enemies to be destroyed on death or wired to a specific event channel.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyDeathWatcher : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private bool destroyOnDetectedDeath = true;

        public event Action<EnemyDeathWatcher> Died;

        private bool _reported;

        //private bool isDead = false;

        public Health Health => health;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        //public DamageResponse ApplyDamage(in DamageRequest request)
        //{
        //    if (isDead || request.Amount <= 0f)
        //    {
        //        return new DamageResponse(0f, false);
        //    }

        //    float 
        //}

        private void Update()
        {
            if (_reported) return;
            if (health == null) return;

            if (health.CurrentHealth <= 0f)
            {
                _reported = true;
                Died?.Invoke(this);

                if (destroyOnDetectedDeath)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
