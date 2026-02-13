using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/HealthEventChannel")]
public class HealthEventChannel : ScriptableObject
{
    public event Action<Health> Spawned;
    public event Action<Health, float, DamageRequest> Changed;
    public event Action<Health, DamageRequest> Destroyed;
    public event Action<Health> Reset;

    public void RaiseSpawned(Health health) => Spawned?.Invoke(health);
    public void RaiseChanged(Health health, float delta, DamageRequest damageRequest) => Changed?.Invoke(health, delta, damageRequest);
    public void RaiseDestroyed(Health health, DamageRequest damageRequest) => Destroyed?.Invoke(health, damageRequest);
    public void RaiseReset(Health health) => Reset?.Invoke(health);
}
