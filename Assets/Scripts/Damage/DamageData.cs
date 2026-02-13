using UnityEngine;

public struct DamageRequest
{
    public readonly float Amount;
    public readonly GameObject Source;
    public readonly Vector3 HitPoint;
    public readonly Vector3 HitNormal;
    public readonly DamageType Type;
    public DamageRequest(float amount, GameObject source = null, Vector3 hitPoint = default, Vector3 hitNormal = default, DamageType type = DamageType.Generic)
    {
        Amount = amount;
        Source = source;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        Type = type;
    }
}

public struct DamageResponse
{
    public readonly float Applied;
    public readonly bool Destroyed;
    public DamageResponse(float applied, bool destroyed)
    {
        Applied = applied;
        Destroyed = destroyed;
    }
}

public enum DamageType
{
    Generic,
    Physical,
    Fire,
    Ice,
    Electric,
    Poison
}