using UnityEngine;

[CreateAssetMenu(fileName = "PickupEventChannel", menuName = "Events/PickupEventChannel")]
public class PickupEventChannel : ScriptableObject
{
    public event System.Action<Pickupable> OnPickup;
    public event System.Action<Pickupable> OnDrop;
    public event System.Action<Pickupable> OnThrow;

    public void RaisePickup(Pickupable pickupable) => OnPickup?.Invoke(pickupable);
    public void RaiseDrop(Pickupable pickupable) => OnDrop?.Invoke(pickupable);
    public void RaiseThrow(Pickupable pickupable) => OnThrow?.Invoke(pickupable);
}
