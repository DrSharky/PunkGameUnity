using UnityEngine;

public interface IDamageable
{
    DamageResponse ApplyDamage(in DamageRequest request);
}
