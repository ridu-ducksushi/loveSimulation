using UnityEngine;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 스토리 이벤트 정의 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStoryEvent", menuName = "LoveSimulation/Story Event")]
    public class StoryEventData : ScriptableObject
    {
        [Header("이벤트 식별")]
        [SerializeField] private string _eventId = string.Empty;

        [Header("트리거할 대화")]
        [SerializeField] private string _dialogueId = string.Empty;

        [Header("발동 조건")]
        [SerializeField] private EventCondition _conditions = new EventCondition();

        [Header("이벤트 설정")]
        [Tooltip("우선순위 (높을수록 먼저 트리거)")]
        [SerializeField] private int _priority = 0;

        [Tooltip("반복 가능 여부")]
        [SerializeField] private bool _repeatable = false;

        [Header("디버그")]
        [SerializeField] [TextArea(2, 5)] private string _description = string.Empty;

        public string EventId => _eventId;
        public string DialogueId => _dialogueId;
        public EventCondition Conditions => _conditions;
        public int Priority => _priority;
        public bool Repeatable => _repeatable;
        public string Description => _description;

        /// <summary>
        /// 이벤트 발동 가능 여부 확인.
        /// </summary>
        public bool CanTrigger()
        {
            if (string.IsNullOrEmpty(_eventId))
            {
                Debug.LogWarning($"[StoryEventData] 이벤트 ID가 비어있음: {name}");
                return false;
            }

            // 반복 불가 이벤트가 이미 트리거된 경우
            if (!_repeatable && Core.WorldState.IsEventTriggered(_eventId))
            {
                return false;
            }

            // 조건 평가
            if (_conditions == null)
            {
                return true;
            }

            return _conditions.Evaluate();
        }

        private void OnValidate()
        {
            // 에디터에서 ID가 비어있으면 파일명으로 자동 설정
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = name;
            }
        }
    }
}
