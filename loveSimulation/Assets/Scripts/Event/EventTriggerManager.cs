using UnityEngine;
using LoveSimulation.Core;
using LoveSimulation.Data;
using LoveSimulation.Dialogue;
using LoveSimulation.Events;

namespace LoveSimulation.Event
{
    /// <summary>
    /// 조건 체크 및 스토리 이벤트 트리거 관리자.
    /// </summary>
    public class EventTriggerManager : Singleton<EventTriggerManager>
    {
        [SerializeField] private bool _autoCheckOnTimeChange = true;
        [SerializeField] private bool _autoCheckOnLocationChange = true;

        private bool _isTriggeringEvent = false;

        private void OnEnable()
        {
            EventBus.Subscribe<TimeOfDayChanged>(OnTimeOfDayChanged);
            EventBus.Subscribe<LocationChanged>(OnLocationChanged);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TimeOfDayChanged>(OnTimeOfDayChanged);
            EventBus.Unsubscribe<LocationChanged>(OnLocationChanged);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        /// <summary>
        /// 시간대 변경 시 이벤트 체크.
        /// </summary>
        private void OnTimeOfDayChanged(TimeOfDayChanged evt)
        {
            if (_autoCheckOnTimeChange)
            {
                CheckAndTriggerEvents();
            }
        }

        /// <summary>
        /// 장소 변경 시 이벤트 체크.
        /// </summary>
        private void OnLocationChanged(LocationChanged evt)
        {
            if (_autoCheckOnLocationChange)
            {
                CheckAndTriggerEvents();
            }
        }

        /// <summary>
        /// 대화 종료 시 트리거 플래그 해제.
        /// </summary>
        private void OnDialogueEnded(DialogueEnded evt)
        {
            _isTriggeringEvent = false;
        }

        /// <summary>
        /// 현재 조건에 맞는 이벤트를 찾아 트리거.
        /// </summary>
        public void CheckAndTriggerEvents()
        {
            // 이미 이벤트 트리거 중이면 중복 방지
            if (_isTriggeringEvent)
            {
                Debug.Log("[EventTriggerManager] 이벤트 트리거 중, 중복 체크 건너뜀.");
                return;
            }

            // 대화 중이면 체크하지 않음
            if (DialogueManager.HasInstance && DialogueManager.Instance.IsDialogueActive)
            {
                Debug.Log("[EventTriggerManager] 대화 진행 중, 이벤트 체크 건너뜀.");
                return;
            }

            StoryEventData topEvent = StoryEventDatabase.GetTopTriggerable();
            if (topEvent == null)
            {
                Debug.Log("[EventTriggerManager] 트리거 가능한 이벤트 없음.");
                return;
            }

            TriggerEvent(topEvent);
        }

        /// <summary>
        /// 특정 이벤트 강제 트리거.
        /// </summary>
        public void TriggerEvent(StoryEventData eventData)
        {
            if (eventData == null)
            {
                Debug.LogError("[EventTriggerManager] null 이벤트 트리거 시도.");
                return;
            }

            if (_isTriggeringEvent)
            {
                Debug.LogWarning("[EventTriggerManager] 이미 이벤트 트리거 중.");
                return;
            }

            _isTriggeringEvent = true;

            // 비반복 이벤트는 트리거 기록
            if (!eventData.Repeatable)
            {
                WorldState.MarkEventTriggered(eventData.EventId);
            }

            Debug.Log($"[EventTriggerManager] 이벤트 트리거: {eventData.EventId} → 대화 {eventData.DialogueId}");

            // DialogueManager가 없으면 경고
            if (!DialogueManager.HasInstance)
            {
                Debug.LogError("[EventTriggerManager] DialogueManager가 없음.");
                _isTriggeringEvent = false;
                return;
            }

            // 대화 시작
            if (string.IsNullOrEmpty(eventData.DialogueId))
            {
                Debug.LogWarning($"[EventTriggerManager] 이벤트 {eventData.EventId}에 대화 ID가 없음.");
                _isTriggeringEvent = false;
                return;
            }

            DialogueManager.Instance.StartDialogue(eventData.DialogueId);
        }

        /// <summary>
        /// ID로 이벤트 트리거.
        /// </summary>
        public void TriggerEventById(string eventId)
        {
            StoryEventData eventData = StoryEventDatabase.GetEvent(eventId);
            if (eventData == null)
            {
                Debug.LogError($"[EventTriggerManager] 이벤트를 찾을 수 없음: {eventId}");
                return;
            }

            TriggerEvent(eventData);
        }

        /// <summary>
        /// 현재 트리거 가능한 이벤트 개수.
        /// </summary>
        public int GetTriggerableCount()
        {
            return StoryEventDatabase.GetTriggerable().Count;
        }
    }
}
