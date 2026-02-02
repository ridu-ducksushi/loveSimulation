using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 선택지 버튼 동적 생성 및 표시 UI.
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _choiceButtonParent;
        [SerializeField] private GameObject _choiceButtonPrefab;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

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

            for (int i = 0; i < evt.Choices.Count; i++)
            {
                CreateChoiceButton(i, evt.Choices[i].Text);
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
        /// 선택지 버튼 생성.
        /// </summary>
        private void CreateChoiceButton(int index, string text)
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
                tmp.text = text;
            }

            // 클릭 이벤트 바인딩
            var button = buttonGo.GetComponent<Button>();
            if (button != null)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() => OnButtonClicked(capturedIndex));
            }

            buttonGo.SetActive(true);
        }

        /// <summary>
        /// 버튼 클릭 → 패널 숨김, ChoiceSelected 발행.
        /// </summary>
        private void OnButtonClicked(int index)
        {
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
