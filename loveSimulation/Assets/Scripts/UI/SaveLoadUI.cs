using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 세이브/로드 전체 화면 오버레이 UI. 5개 슬롯 표시.
    /// </summary>
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("패널")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _closeButton;

        [Header("슬롯")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;

        [Header("확인 대화상자")]
        [SerializeField] private GameObject _confirmDialog;
        [SerializeField] private TextMeshProUGUI _confirmText;
        [SerializeField] private Button _confirmYesButton;
        [SerializeField] private Button _confirmNoButton;

        private bool _isSaveMode;
        private int _pendingSlotIndex;
        private readonly List<GameObject> _spawnedSlots = new List<GameObject>();

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_confirmDialog != null) _confirmDialog.SetActive(false);
            BindButtons();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SaveLoadUIRequested>(OnSaveLoadUIRequested);
            EventBus.Subscribe<SaveCompleted>(OnSaveCompleted);
            EventBus.Subscribe<LoadCompleted>(OnLoadCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SaveLoadUIRequested>(OnSaveLoadUIRequested);
            EventBus.Unsubscribe<SaveCompleted>(OnSaveCompleted);
            EventBus.Unsubscribe<LoadCompleted>(OnLoadCompleted);
        }

        private void BindButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_confirmYesButton != null)
            {
                _confirmYesButton.onClick.AddListener(OnConfirmYes);
            }

            if (_confirmNoButton != null)
            {
                _confirmNoButton.onClick.AddListener(HideConfirmDialog);
            }
        }

        private void OnSaveLoadUIRequested(SaveLoadUIRequested evt)
        {
            Show(evt.IsSaveMode);
        }

        /// <summary>
        /// UI 표시. 제목 설정 후 슬롯 갱신.
        /// </summary>
        public void Show(bool isSaveMode)
        {
            _isSaveMode = isSaveMode;

            if (_titleText != null)
            {
                _titleText.text = isSaveMode ? "저장하기" : "불러오기";
            }

            RefreshSlots();

            if (_panel != null) _panel.SetActive(true);
        }

        /// <summary>
        /// UI 숨김. 슬롯 정리 후 이벤트 발행.
        /// </summary>
        public void Hide()
        {
            HideConfirmDialog();
            ClearSlots();

            if (_panel != null) _panel.SetActive(false);

            EventBus.Publish(new SaveLoadUIClosed());
        }

        /// <summary>
        /// 모든 슬롯 정보를 읽어 프리팹 인스턴스 생성.
        /// </summary>
        private void RefreshSlots()
        {
            ClearSlots();

            if (_slotPrefab == null || _slotContainer == null)
            {
                Debug.LogError("[SaveLoadUI] 슬롯 프리팹 또는 컨테이너 미설정.");
                return;
            }

            SaveSlotInfo[] infos = SaveLoadManager.Instance.GetAllSlotInfos();

            for (int i = 0; i < infos.Length; i++)
            {
                GameObject slotGo = Instantiate(_slotPrefab, _slotContainer);
                _spawnedSlots.Add(slotGo);

                var slotUI = slotGo.GetComponent<SaveSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(i, infos[i], _isSaveMode, OnSlotClicked, OnDeleteClicked);
                }

                slotGo.SetActive(true);
            }
        }

        /// <summary>
        /// 슬롯 클릭 처리.
        /// </summary>
        private void OnSlotClicked(int slotIndex)
        {
            SaveSlotInfo info = SaveLoadManager.Instance.GetSlotInfo(slotIndex);

            if (_isSaveMode)
            {
                if (info.IsEmpty)
                {
                    // 빈 슬롯: 바로 저장
                    ExecuteSave(slotIndex);
                }
                else
                {
                    // 데이터 있음: 덮어쓰기 확인
                    _pendingSlotIndex = slotIndex;
                    ShowConfirmDialog($"슬롯 {slotIndex + 1}에 덮어쓰시겠습니까?");
                }
            }
            else
            {
                if (info.IsEmpty)
                {
                    return;
                }

                // 로드 확인
                _pendingSlotIndex = slotIndex;
                ShowConfirmDialog($"슬롯 {slotIndex + 1}을(를) 불러오시겠습니까?");
            }
        }

        /// <summary>
        /// 삭제 버튼 클릭 처리.
        /// </summary>
        private void OnDeleteClicked(int slotIndex)
        {
            _pendingSlotIndex = slotIndex;
            ShowConfirmDialog($"슬롯 {slotIndex + 1}을(를) 삭제하시겠습니까?");
        }

        private void ShowConfirmDialog(string message)
        {
            if (_confirmText != null) _confirmText.text = message;
            if (_confirmDialog != null) _confirmDialog.SetActive(true);
        }

        private void HideConfirmDialog()
        {
            if (_confirmDialog != null) _confirmDialog.SetActive(false);
        }

        /// <summary>
        /// 확인 대화상자에서 "예" 선택.
        /// </summary>
        private void OnConfirmYes()
        {
            HideConfirmDialog();

            // 삭제 요청인 경우 (확인 텍스트에 "삭제" 포함)
            if (_confirmText != null && _confirmText.text.Contains("삭제"))
            {
                SaveLoadManager.Instance.DeleteSlot(_pendingSlotIndex);
                RefreshSlots();
                return;
            }

            if (_isSaveMode)
            {
                ExecuteSave(_pendingSlotIndex);
            }
            else
            {
                ExecuteLoad(_pendingSlotIndex);
            }
        }

        private void ExecuteSave(int slotIndex)
        {
            EventBus.Publish(new SaveRequested { SlotIndex = slotIndex });
        }

        private void ExecuteLoad(int slotIndex)
        {
            EventBus.Publish(new LoadRequested { SlotIndex = slotIndex });
        }

        private void OnSaveCompleted(SaveCompleted evt)
        {
            if (_panel != null && _panel.activeSelf)
            {
                RefreshSlots();
            }
        }

        private void OnLoadCompleted(LoadCompleted evt)
        {
            if (evt.Success)
            {
                Hide();
            }
        }

        private void ClearSlots()
        {
            for (int i = _spawnedSlots.Count - 1; i >= 0; i--)
            {
                if (_spawnedSlots[i] != null)
                {
                    Destroy(_spawnedSlots[i]);
                }
            }

            _spawnedSlots.Clear();
        }
    }
}
