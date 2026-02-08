using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 에피소드 완료 팝업. 대화 종료 시 표시.
    /// </summary>
    public class EpisodeCompleteUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _lobbyButton;
        [SerializeField] private Button _adRewardButton;
        [SerializeField] private TextMeshProUGUI _adRewardButtonText;

        [Header("설정")]
        [SerializeField] private int _adRewardAmount = 10;
        [SerializeField] private float _fadeInDuration = 0.3f;

        private bool _adRewarded;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            ResolveReferences();
            HideImmediate();
            BindButtons();
        }

        /// <summary>
        /// SerializeField가 null인 참조를 자식 오브젝트에서 자동 탐색.
        /// </summary>
        private void ResolveReferences()
        {
            // 항상 이름으로 올바른 패널 탐색 (잘못된 오브젝트 참조 대비)
            Transform panelTransform = transform.Find("EpisodeCompletePanel");
            if (panelTransform != null)
            {
                _popupPanel = panelTransform.gameObject;
                _canvasGroup = panelTransform.GetComponent<CanvasGroup>();
            }

            if (_titleText == null)
            {
                _titleText = FindComponentInChildren<TextMeshProUGUI>("CompleteTitleText");
            }

            if (_lobbyButton == null)
            {
                _lobbyButton = FindComponentInChildren<Button>("LobbyButton");
            }

            if (_adRewardButton == null)
            {
                _adRewardButton = FindComponentInChildren<Button>("AdRewardButton");
            }

            if (_adRewardButtonText == null)
            {
                _adRewardButtonText = FindComponentInChildren<TextMeshProUGUI>("AdRewardButtonText");
            }
        }

        private T FindComponentInChildren<T>(string childName) where T : Component
        {
            // 비활성 포함 전체 자식에서 이름으로 탐색
            var components = GetComponentsInChildren<T>(true);
            foreach (var comp in components)
            {
                if (comp.gameObject.name == childName)
                {
                    return comp;
                }
            }

            Debug.LogWarning($"[EpisodeCompleteUI] '{childName}' 오브젝트를 찾을 수 없습니다.");
            return null;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void BindButtons()
        {
            if (_lobbyButton != null)
            {
                _lobbyButton.onClick.AddListener(OnLobbyButtonClicked);
            }

            if (_adRewardButton != null)
            {
                _adRewardButton.onClick.AddListener(OnAdRewardButtonClicked);
            }
        }

        private void OnDialogueEnded(DialogueEnded evt)
        {
            if (string.IsNullOrEmpty(evt.DialogueId))
            {
                return;
            }

            // chapter로 시작하는 대화가 끝나면 완료 팝업 표시
            if (!evt.DialogueId.StartsWith("chapter"))
            {
                return;
            }

            string episodeLabel = GetEpisodeLabel(evt.DialogueId);
            GameData.SetFlag($"{evt.DialogueId}_completed");
            ShowPopup(episodeLabel);
        }

        /// <summary>
        /// dialogueId에서 표시용 에피소드 레이블 추출.
        /// </summary>
        private string GetEpisodeLabel(string dialogueId)
        {
            // "chapter01" → "Episode 01", "chapter02" → "Episode 02"
            if (dialogueId.Length > 7)
            {
                string number = dialogueId.Substring(7);
                return $"Episode {number}";
            }

            return "Episode";
        }

        /// <summary>
        /// 팝업 표시 + FadeIn.
        /// </summary>
        private void ShowPopup(string episodeLabel)
        {
            _adRewarded = false;

            if (_titleText != null)
            {
                _titleText.text = $"{episodeLabel} \uc644\ub8cc!";
            }

            UpdateAdButton();

            if (_popupPanel != null)
            {
                _popupPanel.SetActive(true);
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeIn());
        }

        /// <summary>
        /// 광고 버튼 상태 갱신.
        /// </summary>
        private void UpdateAdButton()
        {
            if (_adRewardButton != null)
            {
                _adRewardButton.interactable = !_adRewarded;
            }

            if (_adRewardButtonText != null)
            {
                _adRewardButtonText.text = _adRewarded
                    ? "\uc218\ub839 \uc644\ub8cc!"
                    : $"\uad11\uace0 \ubcf4\uace0 \ud83d\udc8e{_adRewardAmount} \ubc1b\uae30";
            }
        }

        private void OnLobbyButtonClicked()
        {
            HidePopup();
        }

        private void OnAdRewardButtonClicked()
        {
            if (_adRewarded)
            {
                return;
            }

            // 광고 플레이스홀더 - 바로 보상 지급 (추후 실제 광고 연동)
            Debug.Log("[EpisodeCompleteUI] 광고 시청 (플레이스홀더) → 다이아몬드 보상 지급.");
            GameData.AddDiamonds(_adRewardAmount);
            _adRewarded = true;

            // 보상 수령 후 바로 로비로 복귀
            HidePopup();
        }

        private void HidePopup()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            HideImmediate();
        }

        private void HideImmediate()
        {
            if (_popupPanel != null)
            {
                _popupPanel.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null)
            {
                yield break;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _fadeCoroutine = null;
        }
    }
}
