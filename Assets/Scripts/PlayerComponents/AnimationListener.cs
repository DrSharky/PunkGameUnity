using UnityEngine;

public class AnimationListener : MonoBehaviour
{
    [SerializeField] private PickupEventChannel pickupEventChannel;
    [SerializeField] private Animator animator;
    [SerializeField] private string pickupBoolName = "Holding";

    private void Awake()
    {
        if (pickupEventChannel == null)
        {
            Debug.LogError($"PickupEventChannel reference is missing on {gameObject.name}");
        }
        if (animator == null)
        {
            Debug.LogWarning($"Animator reference is missing on {gameObject.name}, getting reference to {gameObject}'s animator...");
            animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning($"No Animator found on {gameObject.name}. Add Animator or assign one in inspector.");
            }
        }
    }

    private void OnEnable()
    {
        pickupEventChannel.OnPickup += OnPickup;
        pickupEventChannel.OnDrop += OnDrop;
        pickupEventChannel.OnThrow += OnThrow;
    }

    private void OnDisable()
    {
        pickupEventChannel.OnPickup -= OnPickup;
        pickupEventChannel.OnDrop -= OnDrop;
        pickupEventChannel.OnThrow -= OnThrow;
    }

    private void OnPickup(Pickupable obj)
    {
        animator.SetFloat(pickupBoolName, 1f);
    }

    private void OnDrop(Pickupable obj)
    {
        animator.SetFloat(pickupBoolName, 0f);
    }

    private void OnThrow(Pickupable obj)
    {
        animator.SetFloat(pickupBoolName, 0f);
    }
}
