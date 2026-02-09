using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Dialogue;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 로비 화면 UI. GameState.Title일 때 표시, 그 외 상태에서 숨김.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        private const int MaxEpisode = 5;
        private const string ChapterPrefix = "chapter";
        private const string CompletedSuffix = "_completed";

        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private Button _episode01Button;
        [SerializeField] private Button _characterButton;
        [SerializeField] private SpeechBubbleUI _speechBubbleUI;
        [SerializeField] private TextMeshProUGUI _episodeButtonText;

        private void Awake()
        {
            if (_episodeButtonText == null && _episode01Button != null)
            {
                _episodeButtonText = _episode01Button.GetComponentInChildren<TextMeshProUGUI>();
            }

            BindButtons();
        }

        private void Start()
        {
            // 초기 상태에 따라 패널 표시 여부 결정
            bool isTitle = GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Title;
            SetPanelActive(isTitle);
            UpdateEpisodeButton();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void BindButtons()
        {
            if (_episode01Button != null)
            {
                _episode01Button.onClick.AddListener(OnEpisodeButtonClicked);
            }

            if (_characterButton != null)
            {
                _characterButton.onClick.AddListener(OnCharacterClicked);
            }
        }

        private void OnGameStateChanged(GameStateChanged evt)
        {
            SetPanelActive(evt.NewState == GameState.Title);

            if (evt.NewState == GameState.Title)
            {
                // 다른 핸들러에서 완료 플래그 설정 후 버튼 업데이트되도록 한 프레임 지연
                StartCoroutine(UpdateEpisodeButtonDeferred());
            }
        }

        private IEnumerator UpdateEpisodeButtonDeferred()
        {
            yield return null;
            UpdateEpisodeButton();
        }

        private void OnCharacterClicked()
        {
            if (_speechBubbleUI != null)
            {
                _speechBubbleUI.ShowRandomLine();
            }
        }

        private void OnEpisodeButtonClicked()
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[LobbyUI] DialogueManager 인스턴스 없음.");
                return;
            }

            int episodeNumber = GetNextEpisodeNumber();
            string chapterId = $"{ChapterPrefix}{episodeNumber:D2}";
            DialogueManager.Instance.StartDialogue(chapterId);
        }

        private int GetNextEpisodeNumber()
        {
            for (int i = 1; i <= MaxEpisode; i++)
            {
                string flag = $"{ChapterPrefix}{i:D2}{CompletedSuffix}";
                if (!GameData.GetFlag(flag))
                {
                    return i;
                }
            }

            // 모든 에피소드 완료 시 마지막 에피소드 반환
            return MaxEpisode;
        }

        private void UpdateEpisodeButton()
        {
            if (_episodeButtonText == null)
            {
                return;
            }

            int episodeNumber = GetNextEpisodeNumber();
            _episodeButtonText.text = $"Episode {episodeNumber:D2}\n시작하기";
        }

        private void SetPanelActive(bool active)
        {
            if (_lobbyPanel != null)
            {
                _lobbyPanel.SetActive(active);
            }
        }
    }
}
