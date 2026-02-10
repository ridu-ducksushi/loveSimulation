using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 상단바 다이아몬드 재화 표시 UI.
    /// </summary>
    public class CurrencyUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _diamondText;
        [SerializeField] private Image _diamondIcon;
        [SerializeField] private TextMeshProUGUI _clueText;
        [SerializeField] private Image _clueIcon;

        private void Awake()
        {
            ResolveReferences();
        }

        /// <summary>
        /// SerializeField가 null인 참조를 자식 오브젝트에서 자동 탐색.
        /// </summary>
        private void ResolveReferences()
        {
            // 항상 이름으로 올바른 패널 탐색 (잘못된 오브젝트 참조 대비)
            Transform panelTransform = transform.Find("CurrencyBar");
            if (panelTransform != null)
            {
                _panel = panelTransform.gameObject;
            }

            if (_diamondText == null)
            {
                _diamondText = FindComponentInChildren<TextMeshProUGUI>("DiamondText");
            }

            if (_diamondIcon == null)
            {
                _diamondIcon = FindComponentInChildren<Image>("DiamondIcon");
            }

            if (_clueText == null)
            {
                _clueText = FindComponentInChildren<TextMeshProUGUI>("ClueText");
            }

            if (_clueIcon == null)
            {
                _clueIcon = FindComponentInChildren<Image>("ClueIcon");
            }
        }

        private T FindComponentInChildren<T>(string childName) where T : Component
        {
            var components = GetComponentsInChildren<T>(true);
            foreach (var comp in components)
            {
                if (comp.gameObject.name == childName)
                {
                    return comp;
                }
            }

            Debug.LogWarning($"[CurrencyUI] '{childName}' 오브젝트를 찾을 수 없습니다.");
            return null;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CurrencyChanged>(OnCurrencyChanged);
            EventBus.Subscribe<ClueCurrencyChanged>(OnClueCurrencyChanged);
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            UpdateDisplay();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChanged>(OnCurrencyChanged);
            EventBus.Unsubscribe<ClueCurrencyChanged>(OnClueCurrencyChanged);
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void Start()
        {
            UpdateDisplay();
            UpdateVisibility();
        }

        private void OnCurrencyChanged(CurrencyChanged evt)
        {
            UpdateDiamondDisplay();
        }

        private void OnClueCurrencyChanged(ClueCurrencyChanged evt)
        {
            UpdateClueDisplay();
        }

        private void OnGameStateChanged(GameStateChanged evt)
        {
            UpdateVisibility(evt.NewState);
        }

        private void UpdateDisplay()
        {
            UpdateDiamondDisplay();
            UpdateClueDisplay();
        }

        private void UpdateDiamondDisplay()
        {
            if (_diamondText != null)
            {
                _diamondText.text = GameData.GetDiamonds().ToString();
            }
        }

        private void UpdateClueDisplay()
        {
            if (_clueText != null)
            {
                _clueText.text = GameData.GetClues().ToString();
            }
        }

        /// <summary>
        /// 게임 상태에 따라 패널 표시/숨김.
        /// </summary>
        private void UpdateVisibility()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            UpdateVisibility(GameManager.Instance.CurrentState);
        }

        private void UpdateVisibility(GameState state)
        {
            if (_panel == null)
            {
                return;
            }

            // 로비와 대화 중에 표시
            bool visible = state == GameState.Title || state == GameState.Dialogue;
            _panel.SetActive(visible);
        }
    }
}
