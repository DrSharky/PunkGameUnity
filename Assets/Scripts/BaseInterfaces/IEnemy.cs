using UnityEngine;

public interface IEnemy
{
    public void Attack(GameObject gameObject, DamageType type);
    public void Move();
    public void MoveTo(GameObject target);

}
