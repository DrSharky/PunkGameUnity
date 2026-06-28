using System;
using UnityEngine;

namespace PunkGame.GameState
{
    public enum WavePhase
    {
        None = 0,
        Countdown = 1,
        InWave = 2,
        UpgradePause = 3,
        GameOver = 4,
    }

    [CreateAssetMenu(fileName = "WaveEventChannel", menuName = "Events/Wave Event Channel")]
    public sealed class WaveEventChannel : ScriptableObject
    {
        public event Action<WavePhase> PhaseChanged;
        public event Action<int> CountdownTick;
        public event Action<int> WaveStarted;
        public event Action<int> UpgradePauseStarted;
        public event Action GameOver;

        public void RaisePhaseChanged(WavePhase phase) => PhaseChanged?.Invoke(phase);
        public void RaiseCountdownTick(int secondsLeft) => CountdownTick?.Invoke(secondsLeft);
        public void RaiseWaveStarted(int waveIndex) => WaveStarted?.Invoke(waveIndex);
        public void RaiseUpgradePauseStarted(int waveIndex) => UpgradePauseStarted?.Invoke(waveIndex);
        public void RaiseGameOver() => GameOver?.Invoke();
    }
}
