using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour, IInteractable
{
    [SerializeField] private bool canPickup = true;
    /// <summary>
    /// Represents the transform used as the handle for pickup interactions.
    /// </summary>
    [SerializeField] private Transform pickupHandle;

    private Rigidbody _rb;
    private Collider[] _colliders;
    private Transform _originalParent;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponents<Collider>();
        _originalParent = transform.parent;
    }

    public bool CanInteract(GameObject interactor)
    {
        return canPickup;
    }

    public void Interact(GameObject interactor)
    {
        OnPickup(interactor.transform);
    }

    public void OnPickup(Transform pickupRoot)
    {
        if (canPickup)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            transform.SetParent(pickupRoot, false);
            transform.localPosition = Vector3.zero;
            transform.localPosition = pickupHandle.localPosition; // Offset the pickup to the handle position
            transform.localRotation = Quaternion.identity;

            foreach (var col in _colliders)
            {
                col.excludeLayers = 1 << pickupRoot.gameObject.layer; // Exclude collisions with the pickup root's layer
            }
        }
    }

    public void OnThrow(Vector3 throwDirection, float throwSpeed)
    {
        transform.SetParent(_originalParent, true);
        _rb.isKinematic = false;
        _rb.AddForce(throwDirection.normalized * throwSpeed, ForceMode.VelocityChange);

        foreach (var col in _colliders)
        {
            col.excludeLayers = 0; // Re-enable collisions with all layers
        }
    }

    public void OnDrop()
    {
        transform.SetParent(_originalParent, true);
        _rb.isKinematic = false;
        foreach (var col in _colliders)
        {
            col.excludeLayers = 0; // Re-enable collisions with all layers
        }
    }
}
