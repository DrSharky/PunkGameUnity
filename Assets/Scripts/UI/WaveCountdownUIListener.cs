using TMPro;
using UnityEngine;

namespace PunkGame.UI
{
    /// <summary>
    /// Simple UI hook for displaying the wave start countdown.
    /// </summary>
    public sealed class WaveCountdownUIListener : MonoBehaviour
    {
        [SerializeField] private GameState.WaveEventChannel waveEvents;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private string prefix = "";

        private void OnEnable()
        {
            if (waveEvents == null) return;
            waveEvents.CountdownTick += OnCountdownTick;
            waveEvents.PhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            if (waveEvents == null) return;
            waveEvents.CountdownTick -= OnCountdownTick;
            waveEvents.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(PunkGame.GameState.WavePhase phase)
        {
            if (phase == PunkGame.GameState.WavePhase.Countdown)
            {
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnCountdownTick(int secondsLeft)
        {
            if (countdownText == null) return;

            // When the timer hits 0, the manager will transition to InWave.
            countdownText.text = secondsLeft > 0 ? $"{prefix}{secondsLeft}" : string.Empty;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
