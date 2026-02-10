using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 하단 탭 바 UI. 로비/조사 패널 전환 제어.
    /// GameState.Title일 때만 탭 바와 패널 표시.
    /// </summary>
    public class BottomTabBarUI : MonoBehaviour
    {
        [Header("패널 참조")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _investigationPanel;

        [Header("탭 바")]
        [SerializeField] private GameObject _tabBarRoot;
        [SerializeField] private Button _lobbyTabButton;
        [SerializeField] private Button _investigationTabButton;

        [Header("탭 하이라이트")]
        [SerializeField] private Image _lobbyTabBg;
        [SerializeField] private Image _investigationTabBg;

        private readonly Color _activeColor = new Color(0.3f, 0.6f, 1f, 1f);
        private readonly Color _inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private bool _isLobbyActive = true;

        private void Awake()
        {
            if (_lobbyTabButton != null)
            {
                _lobbyTabButton.onClick.AddListener(SwitchToLobby);
            }

            if (_investigationTabButton != null)
            {
                _investigationTabButton.onClick.AddListener(SwitchToInvestigation);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void Start()
        {
            bool isTitle = GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Title;

            SetTabBarVisible(isTitle);

            if (isTitle)
            {
                SwitchToLobby();
            }
            else
            {
                SetAllPanelsInactive();
            }
        }

        private void OnGameStateChanged(GameStateChanged evt)
        {
            bool isTitle = evt.NewState == GameState.Title;
            SetTabBarVisible(isTitle);

            if (isTitle)
            {
                // Title 상태로 돌아오면 로비 탭으로 복귀
                SwitchToLobby();
            }
            else
            {
                SetAllPanelsInactive();
            }
        }

        public void SwitchToLobby()
        {
            _isLobbyActive = true;

            if (_lobbyPanel != null)
            {
                _lobbyPanel.SetActive(true);
            }

            if (_investigationPanel != null)
            {
                _investigationPanel.SetActive(false);
            }

            UpdateTabHighlight();
        }

        public void SwitchToInvestigation()
        {
            _isLobbyActive = false;

            if (_lobbyPanel != null)
            {
                _lobbyPanel.SetActive(false);
            }

            if (_investigationPanel != null)
            {
                _investigationPanel.SetActive(true);
            }

            UpdateTabHighlight();
        }

        private void SetAllPanelsInactive()
        {
            if (_lobbyPanel != null)
            {
                _lobbyPanel.SetActive(false);
            }

            if (_investigationPanel != null)
            {
                _investigationPanel.SetActive(false);
            }
        }

        private void SetTabBarVisible(bool visible)
        {
            if (_tabBarRoot != null)
            {
                _tabBarRoot.SetActive(visible);
            }
        }

        private void UpdateTabHighlight()
        {
            if (_lobbyTabBg != null)
            {
                _lobbyTabBg.color = _isLobbyActive ? _activeColor : _inactiveColor;
            }

            if (_investigationTabBg != null)
            {
                _investigationTabBg.color = _isLobbyActive ? _inactiveColor : _activeColor;
            }
        }
    }
}
