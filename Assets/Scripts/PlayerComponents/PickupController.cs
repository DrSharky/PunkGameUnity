using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pickupRoot;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float aimRadius = 0.15f;
    [SerializeField] private float throwSpeed = 12f;

    private Pickupable _held;

    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_held != null) Drop();
            else
            {
                 TryPickup();
            }
        }

        if (Input.GetMouseButtonDown(0) && _held != null)
        {
            Throw();
        }
    }

    private void TryPickup()
    {
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.SphereCast(ray, aimRadius, out var hit, interactDistance, pickupMask, QueryTriggerInteraction.Ignore))
        {
            var pickup = hit.collider.GetComponentInParent<Pickupable>();
            if (pickup != null && pickup.CanInteract(gameObject))
            {
                pickup.Interact(gameObject);
                _held = pickup;
            }
        }
    }

    private void Drop()
    {
        if (_held != null)
        {
            _held.OnDrop();
            _held = null;
        }
    }

    private void Throw()
    {
        var throwDirection = cam.transform.forward;
        _held.OnThrow(throwDirection, throwSpeed);
        _held = null;
    }
}
