using System;
using System.Collections;
using System.Collections.Generic;
using PunkGame.GameState;
using UnityEngine;

namespace PunkGame.Spawning
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        public enum SpawnMode
        {
            SpawnPoints = 0,
            RandomInRadius = 1,
        }

        [Header("What to spawn")]
        [SerializeField] private GameObject enemyPrefab;

        [Header("Where to spawn")]
        [SerializeField] private SpawnMode spawnMode = SpawnMode.SpawnPoints;
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("Used for RandomInRadius mode.")]
        [SerializeField] private Transform randomCenter;

        [Min(0f)]
        [SerializeField] private float randomRadius = 10f;

        [Header("Difficulty")]
        [SerializeField] private WaveDifficultySettings difficulty;

        public event Action<GameObject> EnemySpawned;
        public event Action<GameObject> EnemyDefeated;
        public event Action<int, int> WaveSpawnProgress; // (spawnedSoFar, totalToSpawn)

        private readonly HashSet<GameObject> _alive = new();
        private Coroutine _spawnRoutine;

        public int AliveCount => _alive.Count;

        public void SpawnWave(int waveIndex)
        {
            StopSpawning();

            int totalToSpawn = GetTotalToSpawn(waveIndex);
            _spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveIndex, totalToSpawn));
        }

        public void StopSpawning()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        private int GetTotalToSpawn(int waveIndex)
        {
            if (difficulty != null)
            {
                return difficulty.GetSpawnCountForWave(waveIndex);
            }

            // Reasonable fallback.
            return Mathf.Max(0, 5 + (waveIndex - 1) * 2);
        }

        private IEnumerator SpawnWaveRoutine(int waveIndex, int totalToSpawn)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError($"{nameof(EnemySpawner)} has no enemyPrefab assigned.", this);
                yield break;
            }

            if (spawnMode == SpawnMode.SpawnPoints && (spawnPoints == null || spawnPoints.Length == 0))
            {
                Debug.LogError($"{nameof(EnemySpawner)} is in SpawnPoints mode but has no spawnPoints assigned.", this);
                yield break;
            }

            if (spawnMode == SpawnMode.RandomInRadius && randomCenter == null)
            {
                randomCenter = transform;
            }

            int spawned = 0;
            float interval = difficulty != null ? difficulty.spawnIntervalSeconds : 0.25f;
            int maxAlive = difficulty != null ? Mathf.Max(1, difficulty.maxAlive) : 8;

            while (spawned < totalToSpawn)
            {
                // Wait until we have room.
                while (_alive.Count >= maxAlive)
                {
                    yield return null;
                }

                SpawnOne(spawned);
                spawned++;

                WaveSpawnProgress?.Invoke(spawned, totalToSpawn);

                if (interval > 0f)
                {
                    yield return new WaitForSeconds(interval);
                }
                else
                {
                    yield return null;
                }
            }

            _spawnRoutine = null;
        }

        private void SpawnOne(int index)
        {
            Vector3 pos;
            Quaternion rot;
            GetSpawnPose(index, out pos, out rot);

            GameObject instance = Instantiate(enemyPrefab, pos, rot);
            _alive.Add(instance);

            // Ensure the instance reports death.
            var watcher = instance.GetComponent<EnemyDeathWatcher>();
            if (watcher == null)
            {
                watcher = instance.AddComponent<EnemyDeathWatcher>();
            }
            watcher.Died -= OnEnemyDied; // avoid double-subscribe in weird cases
            watcher.Died += OnEnemyDied;

            EnemySpawned?.Invoke(instance);
        }

        private void OnEnemyDied(EnemyDeathWatcher watcher)
        {
            if (watcher == null) return;

            watcher.Died -= OnEnemyDied;

            GameObject go = watcher.gameObject;
            _alive.Remove(go);

            EnemyDefeated?.Invoke(go);
        }

        private void GetSpawnPose(int index, out Vector3 pos, out Quaternion rot)
        {
            rot = Quaternion.identity;

            switch (spawnMode)
            {
                case SpawnMode.SpawnPoints:
                {
                    Transform t = spawnPoints[index % spawnPoints.Length];
                    pos = t != null ? t.position : transform.position;
                    rot = t != null ? t.rotation : Quaternion.identity;
                    return;
                }
                case SpawnMode.RandomInRadius:
                default:
                {
                    Vector2 r = UnityEngine.Random.insideUnitCircle * randomRadius;
                    pos = (randomCenter != null ? randomCenter.position : transform.position) + new Vector3(r.x, 0f, r.y);
                    return;
                }
            }
        }
    }
}
