using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 선택지 버튼 동적 생성 및 표시 UI. 프리미엄 선택지 지원.
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _choiceButtonParent;
        [SerializeField] private GameObject _choiceButtonPrefab;

        [Header("프리미엄 선택지 색상")]
        [SerializeField] private Color _premiumButtonColor = new Color(0.6f, 0.4f, 0.9f, 1f);
        [SerializeField] private Color _disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private List<DialogueChoice> _currentChoices;

        private void Awake()
        {
            if (_choicePanel != null)
            {
                _choicePanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ChoiceRequested>(OnChoiceRequested);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChoiceRequested>(OnChoiceRequested);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        /// <summary>
        /// 선택지 요청 → 버튼 동적 생성, 패널 표시.
        /// </summary>
        private void OnChoiceRequested(ChoiceRequested evt)
        {
            if (evt.Choices == null || evt.Choices.Count == 0)
            {
                return;
            }

            ClearButtons();
            _currentChoices = evt.Choices;

            for (int i = 0; i < evt.Choices.Count; i++)
            {
                CreateChoiceButton(i, evt.Choices[i]);
            }

            if (_choicePanel != null)
            {
                _choicePanel.SetActive(true);
            }
        }

        /// <summary>
        /// 대화 종료 시 패널 정리.
        /// </summary>
        private void OnDialogueEnded(DialogueEnded _)
        {
            HideAndClear();
        }

        /// <summary>
        /// 선택지 버튼 생성. 프리미엄이면 비용 표시 및 잔액 체크.
        /// </summary>
        private void CreateChoiceButton(int index, DialogueChoice choice)
        {
            if (_choiceButtonPrefab == null || _choiceButtonParent == null)
            {
                Debug.LogError("[ChoiceUI] 버튼 프리팹 또는 부모 Transform 미설정.");
                return;
            }

            GameObject buttonGo = Instantiate(_choiceButtonPrefab, _choiceButtonParent);
            _spawnedButtons.Add(buttonGo);

            // 버튼 텍스트 설정
            var tmp = buttonGo.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = choice.IsPremium
                    ? $"\ud83d\udd12 \ud83d\udc8e{choice.CurrencyCost} {choice.Text}"
                    : choice.Text;
            }

            var button = buttonGo.GetComponent<Button>();
            if (button != null)
            {
                if (choice.IsPremium)
                {
                    bool canAfford = GameData.GetDiamonds() >= choice.CurrencyCost;
                    button.interactable = canAfford;

                    // 프리미엄 버튼 색상 변경
                    var colors = button.colors;
                    colors.normalColor = canAfford ? _premiumButtonColor : _disabledButtonColor;
                    colors.highlightedColor = canAfford ? _premiumButtonColor * 1.1f : _disabledButtonColor;
                    colors.disabledColor = _disabledButtonColor;
                    button.colors = colors;
                }

                int capturedIndex = index;
                button.onClick.AddListener(() => OnButtonClicked(capturedIndex));
            }

            buttonGo.SetActive(true);
        }

        /// <summary>
        /// 버튼 클릭 → 프리미엄이면 재화 소모, 패널 숨김, ChoiceSelected 발행.
        /// </summary>
        private void OnButtonClicked(int index)
        {
            if (_currentChoices == null || index < 0 || index >= _currentChoices.Count)
            {
                return;
            }

            DialogueChoice choice = _currentChoices[index];

            // 프리미엄 선택지면 다이아몬드 소모
            if (choice.IsPremium)
            {
                if (!GameData.SpendDiamonds(choice.CurrencyCost))
                {
                    Debug.LogWarning("[ChoiceUI] 다이아몬드 부족으로 프리미엄 선택지 실행 불가.");
                    return;
                }
            }

            HideAndClear();
            EventBus.Publish(new ChoiceSelected { ChoiceIndex = index });
        }

        /// <summary>
        /// 패널 숨김 및 버튼 정리.
        /// </summary>
        private void HideAndClear()
        {
            if (_choicePanel != null)
            {
                _choicePanel.SetActive(false);
            }

            _currentChoices = null;
            ClearButtons();
        }

        /// <summary>
        /// 생성된 버튼 모두 제거.
        /// </summary>
        private void ClearButtons()
        {
            for (int i = _spawnedButtons.Count - 1; i >= 0; i--)
            {
                if (_spawnedButtons[i] != null)
                {
                    Destroy(_spawnedButtons[i]);
                }
            }

            _spawnedButtons.Clear();
        }
    }
}
