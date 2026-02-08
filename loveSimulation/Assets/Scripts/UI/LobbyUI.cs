using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private Button _episode01Button;

        private void Awake()
        {
            BindButtons();
        }

        private void Start()
        {
            // 초기 상태에 따라 패널 표시 여부 결정
            bool isTitle = GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Title;
            SetPanelActive(isTitle);
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
                _episode01Button.onClick.AddListener(OnEpisode01Clicked);
            }
        }

        private void OnGameStateChanged(GameStateChanged evt)
        {
            SetPanelActive(evt.NewState == GameState.Title);
        }

        private void OnEpisode01Clicked()
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[LobbyUI] DialogueManager 인스턴스 없음.");
                return;
            }

            DialogueManager.Instance.StartDialogue("chapter01");
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
