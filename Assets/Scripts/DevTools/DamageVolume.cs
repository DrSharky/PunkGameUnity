using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageVolume : MonoBehaviour
{
    [SerializeField] private float damagePerTick = 10f;
    [SerializeField] private float tickInterval = 1f;

    private readonly HashSet<MonoBehaviour> _targets = new();

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable is MonoBehaviour damageableMb)
        {
            _targets.Add(damageableMb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable is MonoBehaviour damageableMb)
        {
            _targets.Remove(damageableMb);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        var wait = new WaitForSeconds(tickInterval);

        while (true)
        {
            if (_targets.Count > 0)
            {
                foreach (var target in _targets)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    if (target is IDamageable damageable)
                    {
                        damageable.ApplyDamage(new DamageRequest(damagePerTick, gameObject, target.transform.position));
                    }
                }
            }
            yield return wait;
        }
    }
}