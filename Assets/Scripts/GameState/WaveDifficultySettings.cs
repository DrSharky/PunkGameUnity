using UnityEngine;

namespace PunkGame.GameState
{
    /// <summary>
    /// Tunable difficulty knobs for wave spawning.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveDifficultySettings", menuName = "Game/Waves/Wave Difficulty Settings")]
    public sealed class WaveDifficultySettings : ScriptableObject
    {
        [Header("Per-wave enemy count")]
        [Tooltip("Enemies spawned in wave 1.")]
        [Min(0)]
        public int baseSpawnCount = 5;

        [Tooltip("Additional enemies per wave.")]
        public AnimationCurve spawnCountOverWave = AnimationCurve.Linear(1, 1, 10, 3);

        [Header("Rate limits")]
        [Tooltip("Maximum alive enemies at once. Extra spawns wait until an enemy dies.")]
        [Min(1)]
        public int maxAlive = 8;

        [Tooltip("Seconds between spawns within the wave.")]
        [Min(0f)]
        public float spawnIntervalSeconds = 0.25f;

        [Header("TODO")]
        [TextArea]
        public string todo = "TODO: Add per-player saved difficulty (e.g., PlayerPrefs / save file) and expose these knobs in UI.";

        public int GetSpawnCountForWave(int waveIndex)
        {
            waveIndex = Mathf.Max(1, waveIndex);
            float multiplier = spawnCountOverWave != null ? spawnCountOverWave.Evaluate(waveIndex) : 1f;
            return Mathf.Max(0, Mathf.RoundToInt(baseSpawnCount * multiplier));
        }
    }
}
