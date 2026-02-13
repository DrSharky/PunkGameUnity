using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;

    public float MaxHealth => maxHealth;

    [SerializeField]
    public float CurrentHealth;

    private bool isDead = false;

    [SerializeField] private HealthEventChannel healthEvents;

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
            healthEvents?.RaiseDestroyed(this, request);

            if (destroyOnDeath)
            {
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

    public void Reset()
    {
        isDead = false;
        CurrentHealth = maxHealth;
        healthEvents?.RaiseReset(this);
    }
}
