using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 조사 패널 UI. 5초 타이머 조사 → 단서 보상 획득.
    /// </summary>
    public class InvestigationUI : MonoBehaviour
    {
        private const float InvestigationDuration = 5f;
        private const int ClueReward = 1;

        [Header("UI 참조")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _remainingCountText;
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _startButtonText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _adSkipButton;

        private bool _isInvestigating;
        private Coroutine _investigationCoroutine;

        private void OnEnable()
        {
            RefreshUI();
        }

        private void OnDisable()
        {
            CancelInvestigation();
        }

        private void Awake()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_adSkipButton != null)
            {
                _adSkipButton.onClick.AddListener(OnAdSkipClicked);
            }
        }

        private void OnStartClicked()
        {
            if (_isInvestigating)
            {
                return;
            }

            if (GameData.GetDailyInvestigationsRemaining() <= 0)
            {
                Debug.LogWarning("[InvestigationUI] 일일 조사 횟수 소진.");
                return;
            }

            StartInvestigation(false);
        }

        private void OnAdSkipClicked()
        {
            if (!_isInvestigating)
            {
                return;
            }

            // 광고 시청 플레이스홀더 - 즉시 완료 처리
            Debug.Log("[InvestigationUI] 광고 스킵 (플레이스홀더).");
            CancelInvestigation();
            CompleteInvestigation(true);
        }

        private void StartInvestigation(bool isAdSkip)
        {
            _isInvestigating = true;
            _investigationCoroutine = StartCoroutine(InvestigationTimerCoroutine());

            EventBus.Publish(new InvestigationStarted());
            UpdateButtonStates();
        }

        private IEnumerator InvestigationTimerCoroutine()
        {
            SetProgressBarVisible(true);
            SetAdSkipVisible(true);

            float elapsed = 0f;
            while (elapsed < InvestigationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / InvestigationDuration);
                UpdateTimerDisplay(InvestigationDuration - elapsed, progress);
                yield return null;
            }

            UpdateTimerDisplay(0f, 1f);
            CompleteInvestigation(false);
        }

        private void CompleteInvestigation(bool wasAdSkip)
        {
            _isInvestigating = false;
            _investigationCoroutine = null;

            GameData.UseInvestigation();
            GameData.AddClues(ClueReward);

            EventBus.Publish(new InvestigationCompleted
            {
                CluesRewarded = ClueReward,
                WasAdSkip = wasAdSkip
            });

            Debug.Log($"[InvestigationUI] 조사 완료. 단서 +{ClueReward}, 광고스킵: {wasAdSkip}");

            SetProgressBarVisible(false);
            SetAdSkipVisible(false);
            RefreshUI();
        }

        private void CancelInvestigation()
        {
            if (_investigationCoroutine != null)
            {
                StopCoroutine(_investigationCoroutine);
                _investigationCoroutine = null;
            }

            _isInvestigating = false;
            SetProgressBarVisible(false);
            SetAdSkipVisible(false);
        }

        private void RefreshUI()
        {
            int remaining = GameData.GetDailyInvestigationsRemaining();
            bool hasRemaining = remaining > 0;

            if (_remainingCountText != null)
            {
                _remainingCountText.text = $"남은 조사 횟수: {remaining}/{GameData.MaxDailyInvestigations}";
            }

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int remaining = GameData.GetDailyInvestigationsRemaining();
            bool canStart = remaining > 0 && !_isInvestigating;

            if (_startButton != null)
            {
                _startButton.interactable = canStart;
            }

            if (_startButtonText != null)
            {
                _startButtonText.text = _isInvestigating ? "조사 중..."
                    : remaining > 0 ? "조사 시작"
                    : "오늘 조사 완료";
            }
        }

        private void UpdateTimerDisplay(float remainingTime, float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.Max(0f, remainingTime):F1}s";
            }
        }

        private void SetProgressBarVisible(bool visible)
        {
            if (_progressBar != null)
            {
                _progressBar.gameObject.SetActive(visible);
            }

            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(visible);
            }
        }

        private void SetAdSkipVisible(bool visible)
        {
            if (_adSkipButton != null)
            {
                _adSkipButton.gameObject.SetActive(visible);
            }
        }
    }
}
