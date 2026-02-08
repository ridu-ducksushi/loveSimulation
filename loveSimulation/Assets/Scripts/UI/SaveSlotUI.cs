using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 개별 세이브 슬롯 표시 컴포넌트. 프리팹에 부착.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI _slotNumberText;
        [SerializeField] private TextMeshProUGUI _saveDateText;
        [SerializeField] private TextMeshProUGUI _playTimeText;
        [SerializeField] private TextMeshProUGUI _sceneNameText;

        [Header("상태 표시")]
        [SerializeField] private GameObject _emptyLabel;
        [SerializeField] private GameObject _infoGroup;

        [Header("버튼")]
        [SerializeField] private Button _slotButton;
        [SerializeField] private Button _deleteButton;

        private int _slotIndex;
        private Action<int> _onClick;
        private Action<int> _onDelete;

        /// <summary>
        /// 슬롯 정보를 설정하고 표시를 갱신.
        /// </summary>
        public void Setup(int index, Core.SaveSlotInfo info, bool isSaveMode,
            Action<int> onClick, Action<int> onDelete)
        {
            _slotIndex = index;
            _onClick = onClick;
            _onDelete = onDelete;

            SetSlotNumber(index);
            BindButtons();

            if (info == null || info.IsEmpty)
            {
                ShowEmpty(isSaveMode);
            }
            else
            {
                ShowInfo(info, isSaveMode);
            }
        }

        private void SetSlotNumber(int index)
        {
            if (_slotNumberText != null)
            {
                _slotNumberText.text = $"슬롯 {index + 1}";
            }
        }

        private void BindButtons()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(OnSlotClicked);
            }

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveAllListeners();
                _deleteButton.onClick.AddListener(OnDeleteClicked);
            }
        }

        private void ShowEmpty(bool isSaveMode)
        {
            if (_emptyLabel != null) _emptyLabel.SetActive(true);
            if (_infoGroup != null) _infoGroup.SetActive(false);
            if (_deleteButton != null) _deleteButton.gameObject.SetActive(false);

            // 빈 슬롯은 저장 모드에서만 클릭 가능
            if (_slotButton != null) _slotButton.interactable = isSaveMode;
        }

        private void ShowInfo(Core.SaveSlotInfo info, bool isSaveMode)
        {
            if (_emptyLabel != null) _emptyLabel.SetActive(false);
            if (_infoGroup != null) _infoGroup.SetActive(true);

            if (_saveDateText != null) _saveDateText.text = info.SaveDate;
            if (_sceneNameText != null) _sceneNameText.text = info.SceneName;
            if (_playTimeText != null) _playTimeText.text = FormatPlayTime(info.PlayTime);

            // 삭제 버튼은 저장 모드에서만 표시
            if (_deleteButton != null) _deleteButton.gameObject.SetActive(isSaveMode);
            if (_slotButton != null) _slotButton.interactable = true;
        }

        private void OnSlotClicked()
        {
            _onClick?.Invoke(_slotIndex);
        }

        private void OnDeleteClicked()
        {
            _onDelete?.Invoke(_slotIndex);
        }

        /// <summary>
        /// 초 단위 플레이 시간을 "HH:MM:SS" 형식으로 변환.
        /// </summary>
        private string FormatPlayTime(float seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}
