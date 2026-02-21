using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float currentHealth;
    [SerializeField] private HealthEventChannel healthEvents;

    private bool isDead = false;

    public float MaxHealth
    {
        get => maxHealth;
        private set => maxHealth = value;
    }
    public float CurrentHealth
    {
        get => currentHealth;
        private set => currentHealth = value;
    }

    private void Awake()
    {
        CurrentHealth = maxHealth;
        healthEvents?.RaiseSpawned(this);
    }

    public DamageResponse ApplyDamage(in DamageRequest request)
    {
        if (isDead || request.Amount <= 0f)
        {
            return new DamageResponse(0f, false);
        }

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth - request.Amount, 0f, maxHealth);
        float delta = CurrentHealth - oldHealth;

        healthEvents?.RaiseChanged(this, delta, request);

        if (CurrentHealth <= 0f && !isDead)
        {
            isDead = true;

            if (destroyOnDeath)
            {
                healthEvents?.RaiseDestroyed(this, request);
                Destroy(gameObject);
            }

            return new DamageResponse(delta, true);
        }

        return new DamageResponse(delta, false);
    }

    public void Heal(float amount, GameObject healSource = null)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        float delta = CurrentHealth - oldHealth;
        
        var healRequest = new DamageRequest(0f, healSource);
        healthEvents?.RaiseChanged(this, delta, healRequest);
    }

    private void Reset()
    {
        isDead = false;
        CurrentHealth = maxHealth;
        healthEvents?.RaiseReset(this);
    }

#if DEVTOOLS
    private void Update()
    {
        // DEBUG LINE. Remove this in production.
        if (Input.GetKeyDown(KeyCode.K))
        {
            Reset();
        }
    }
#endif
}
