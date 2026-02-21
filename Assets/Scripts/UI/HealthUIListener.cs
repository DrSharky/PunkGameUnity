using UnityEngine;
using UnityEngine.UI;

public class HealthUIListener : MonoBehaviour
{
    [SerializeField] private HealthEventChannel playerHealthChannel;

    [SerializeField] private Image healthBar;

    private Health _playerHealth;

    private void OnEnable()
    {
        playerHealthChannel.Changed += OnHealthChanged;
        playerHealthChannel.Destroyed += OnHealthDestroyed;
        playerHealthChannel.Reset += OnHealthReset;
    }

    private void OnDisable()
    {
        playerHealthChannel.Changed -= OnHealthChanged;
        playerHealthChannel.Destroyed -= OnHealthDestroyed;
        playerHealthChannel.Reset -= OnHealthReset;
    }

    private void OnHealthReset(Health health)
    {
        _playerHealth = health;
        Refresh();
    }

    private void OnHealthChanged(Health health, float delta, DamageRequest request)
    {
        _playerHealth = health;
        Refresh();
    }

    private void Refresh()
    {
        if (_playerHealth == null || healthBar == null)
        {
            return;
        }
        healthBar.fillAmount = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
    }

    private void OnHealthDestroyed(Health health, DamageRequest request)
    {
        Destroy(gameObject);
    }
}
