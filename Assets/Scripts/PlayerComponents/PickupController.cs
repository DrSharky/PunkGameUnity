using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pickupRoot;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private float interactDistance = 1.75f;
    [SerializeField] private float aimRadius = 0.15f;
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private Transform pickupOrigin; // defaulting this to chest.
    [SerializeField] private PickupEventChannel pickupEvents;

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
            if (_held != null)
            {
                Drop();
            }
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

    /// <summary>
    /// Raycast from the 3rd person player using the pickupOrigin as the origin point,
    /// using the camera as the direction of the ray,
    /// and check if it hits a pickupable object within the interact distance and aim radius.
    /// </summary>
    private void TryPickup()
    {
        var rayOrigin = pickupOrigin.position;
        var rayDirection = cam.transform.forward;

        //Debug line to show the raycast in the editor.
        Debug.DrawRay(rayOrigin, rayDirection * interactDistance, Color.red, 5f);

        if (Physics.SphereCast(rayOrigin, aimRadius, rayDirection, out var hit, interactDistance, pickupMask))
        {
            var pickupable = hit.collider.GetComponent<Pickupable>();
            if (pickupable != null)
            {
                _held = pickupable;
                _held.OnPickup(pickupRoot);
                pickupEvents?.RaisePickup(pickupable);
            }
        }
    }

    private void Drop()
    {
        if (_held != null)
        {
            pickupEvents?.RaiseDrop(_held);
            _held.OnDrop();
            _held = null;
        }
    }

    private void Throw()
    {
        var throwDirection = cam.transform.forward;
        pickupEvents?.RaiseThrow(_held);
        _held.OnThrow(throwDirection, throwSpeed);
        _held = null;
    }
}
