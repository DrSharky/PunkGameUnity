using System;
using System.Collections;
using PunkGame.Spawning;
using Unity.VisualScripting;
using UnityEngine;

namespace PunkGame.GameState
{
    /// <summary>
    /// Simple wave-driven game flow:
    /// Countdown -> Wave -> Upgrade Pause -> Wave -> ... -> Game Over.
    /// </summary>
    public sealed class WaveGameStateManager : MonoBehaviour
    {
        public enum Phase
        {
            None = 0,
            Countdown = 1,
            InWave = 2,
            UpgradePause = 3,
            GameOver = 4,
        }

        [Header("Start")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float startCountdownSeconds = 3f;

        [Header("Wave Rules")]
        [Tooltip("Enemies required to complete wave 1.")]
        [SerializeField] private int baseEnemiesToDefeat = 5;

        [Tooltip("Additional enemies required each new wave (waveIndex-1).")]
        [SerializeField] private int enemiesToDefeatIncrementPerWave = 2;

        [Header("Optional Event Channels")]
        [Tooltip("Used to detect player death. Uses Changed event to detect when CurrentHealth reaches 0.")]
        [SerializeField] private HealthEventChannel playerHealthChannel;

        [Tooltip("Optional: if enemies use a Health component wired to this channel and destroyOnDeath is true," +
            "their destruction will be counted toward wave completion")]
        [SerializeField] private HealthEventChannel enemyHealthChannel;

        [Tooltip("Optional: use this to drive UI without directly referencing the manager from UI scripts.")]
        [SerializeField] private WaveEventChannel waveEvents;

        [Header("Wave Spawning")]
        [SerializeField] private EnemySpawner enemySpawner;

        public Phase CurrentPhase { get; private set; } = Phase.None;
        public int CurrentWaveIndex { get; private set; } = 0; // 1-based while in a wave.

        private int _enemiesDefeatedThisWave;
        private bool _started;
        private Coroutine _countdownRoutine;

        private void OnEnable()
        {
            if (playerHealthChannel != null)
            {
                playerHealthChannel.Changed += OnPlayerHealthChanged;
            }

            if (enemyHealthChannel != null)
            {
                enemyHealthChannel.Destroyed += OnEnemyHealthDestroyed;
            }

            if (enemySpawner != null)
            {
                enemySpawner.EnemyDefeated += OnEnemyDefeated;
            }
        }

        private void OnDisable()
        {
            if (playerHealthChannel != null)
            {
                playerHealthChannel.Changed -= OnPlayerHealthChanged;
            }

            if (enemyHealthChannel != null)
            {
                enemyHealthChannel.Destroyed -= OnEnemyHealthDestroyed;
            }

            if (enemySpawner != null)
            {
                enemySpawner.EnemyDefeated -= OnEnemyDefeated;
            }
        }

        private void Start()
        {
            if (autoStart)
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            if (_started) return;
            _started = true;

            Time.timeScale = 1f;
            SetPhase(Phase.Countdown);

            if (_countdownRoutine != null)
            {
                StopCoroutine(_countdownRoutine);
            }
            _countdownRoutine = StartCoroutine(StartCountdownRoutine());
        }

        public void NotifyEnemyDefeated()
        {
            if (CurrentPhase != Phase.InWave) return;

            _enemiesDefeatedThisWave++;
            TryCompleteWave();
        }

        /// <summary>
        /// Call this from your upgrade UI once the player picks an upgrade.
        /// </summary>
        public void CompleteUpgradeSelectionAndStartNextWave()
        {
            if (CurrentPhase != Phase.UpgradePause) return;

            // TODO: Apply upgrade effects here.

            Time.timeScale = 1f;
            BeginNextWave();
        }

        /// <summary>
        /// Can be used by UI buttons etc.
        /// </summary>
        public void ForceGameOver()
        {
            if (CurrentPhase == Phase.GameOver) return;
            EnterGameOver();
        }

        private IEnumerator StartCountdownRoutine()
        {
            Time.timeScale = 0f;
            int secondsLeft = Mathf.CeilToInt(startCountdownSeconds);
            waveEvents?.RaiseCountdownTick(secondsLeft);

            float remaining = startCountdownSeconds;

            while (remaining > 0f)
            {
                yield return null;
                remaining -= Time.unscaledDeltaTime;

                int nextSecondsLeft = Mathf.CeilToInt(remaining);
                if (nextSecondsLeft != secondsLeft)
                {
                    secondsLeft = nextSecondsLeft;
                    waveEvents?.RaiseCountdownTick(secondsLeft);
                }

                if (CurrentPhase == Phase.GameOver)
                {
                    yield break;
                }
            }

            BeginNextWave();
        }

        private void BeginNextWave()
        {
            Time.timeScale = 1f;
            if (CurrentPhase == Phase.GameOver) return;

            CurrentWaveIndex = Mathf.Max(1, CurrentWaveIndex + 1);
            _enemiesDefeatedThisWave = 0;

            SetPhase(Phase.InWave);
            waveEvents?.RaiseWaveStarted(CurrentWaveIndex);

            if (enemySpawner != null)
            {
                enemySpawner.SpawnWave(CurrentWaveIndex);
            }
            else
            {
                enemySpawner = gameObject.AddComponent<EnemySpawner>();
                enemySpawner.SpawnWave(CurrentWaveIndex);

            }
        }

        private void TryCompleteWave()
        {
            int required = GetEnemiesRequiredForWave(CurrentWaveIndex);
            if (_enemiesDefeatedThisWave < required) return;

            EnterUpgradePause();
        }

        private int GetEnemiesRequiredForWave(int waveIndex)
        {
            if (waveIndex <= 0) return baseEnemiesToDefeat;
            return Mathf.Max(1, baseEnemiesToDefeat + (waveIndex - 1) * enemiesToDefeatIncrementPerWave);
        }

        private void EnterUpgradePause()
        {
            if (CurrentPhase == Phase.GameOver) return;

            SetPhase(Phase.UpgradePause);
            waveEvents?.RaiseUpgradePauseStarted(CurrentWaveIndex);

            // Pause gameplay while showing upgrade UI.
            Time.timeScale = 0f;

            // TODO: Open upgrade UI here.
        }

        private void EnterGameOver()
        {
            SetPhase(Phase.GameOver);
            waveEvents?.RaiseGameOver();

            // Freeze gameplay.
            Time.timeScale = 0f;

            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }

            // TODO: Show game over UI / restart menu.
        }

        private void SetPhase(Phase next)
        {
            if (CurrentPhase == next) return;
            CurrentPhase = next;
            waveEvents?.RaisePhaseChanged(ToUIPhase(next));
        }

        private static WavePhase ToUIPhase(Phase phase)
        {
            return phase switch
            {
                Phase.Countdown => WavePhase.Countdown,
                Phase.InWave => WavePhase.InWave,
                Phase.UpgradePause => WavePhase.UpgradePause,
                Phase.GameOver => WavePhase.GameOver,
                _ => WavePhase.None,
            };
        }

        private void OnPlayerHealthChanged(Health health, float delta, DamageRequest request)
        {
            if (health == null) return;

            // Health raises Changed before it sets its internal isDead flag,
            // so this is a reliable way to detect death without modifying Health.
            if (health.CurrentHealth <= 0f)
            {
                EnterGameOver();
            }
        }

        private void OnEnemyHealthDestroyed(Health health, DamageRequest request)
        {
            // Assumes enemy Health uses destroyOnDeath=true.
            NotifyEnemyDefeated();
        }

        private void OnEnemyDefeated(GameObject enemy)
        {
            NotifyEnemyDefeated();
        }
    }
}
