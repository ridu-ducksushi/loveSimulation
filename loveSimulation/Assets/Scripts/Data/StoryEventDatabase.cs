using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 스토리 이벤트 레지스트리. Resources 폴더에서 자동 로드.
    /// </summary>
    public static class StoryEventDatabase
    {
        private static Dictionary<string, StoryEventData> _events = new Dictionary<string, StoryEventData>();
        private static List<StoryEventData> _eventList = new List<StoryEventData>();
        private static bool _initialized = false;

        private const string ResourcePath = "Events";

        /// <summary>
        /// 데이터베이스 초기화. Resources/Events 폴더에서 로드.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _events.Clear();
            _eventList.Clear();

            StoryEventData[] loadedEvents = Resources.LoadAll<StoryEventData>(ResourcePath);

            foreach (StoryEventData eventData in loadedEvents)
            {
                if (eventData == null || string.IsNullOrEmpty(eventData.EventId))
                {
                    continue;
                }

                if (_events.ContainsKey(eventData.EventId))
                {
                    Debug.LogWarning($"[StoryEventDatabase] 중복 이벤트 ID 무시: {eventData.EventId}");
                    continue;
                }

                _events[eventData.EventId] = eventData;
                _eventList.Add(eventData);
            }

            // 우선순위 내림차순 정렬
            _eventList = _eventList.OrderByDescending(e => e.Priority).ToList();

            _initialized = true;

            Debug.Log($"[StoryEventDatabase] 초기화 완료. {_events.Count}개 이벤트 로드.");
        }

        /// <summary>
        /// ID로 이벤트 조회.
        /// </summary>
        public static StoryEventData GetEvent(string eventId)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrEmpty(eventId))
            {
                return null;
            }

            return _events.TryGetValue(eventId, out StoryEventData data) ? data : null;
        }

        /// <summary>
        /// 모든 이벤트 조회 (우선순위 순).
        /// </summary>
        public static IReadOnlyList<StoryEventData> GetAllEvents()
        {
            if (!_initialized)
            {
                Initialize();
            }

            return _eventList;
        }

        /// <summary>
        /// 현재 트리거 가능한 이벤트 목록 조회 (우선순위 순).
        /// </summary>
        public static List<StoryEventData> GetTriggerable()
        {
            if (!_initialized)
            {
                Initialize();
            }

            List<StoryEventData> result = new List<StoryEventData>();

            foreach (StoryEventData eventData in _eventList)
            {
                if (eventData.CanTrigger())
                {
                    result.Add(eventData);
                }
            }

            return result;
        }

        /// <summary>
        /// 가장 높은 우선순위의 트리거 가능 이벤트 조회.
        /// </summary>
        public static StoryEventData GetTopTriggerable()
        {
            if (!_initialized)
            {
                Initialize();
            }

            foreach (StoryEventData eventData in _eventList)
            {
                if (eventData.CanTrigger())
                {
                    return eventData;
                }
            }

            return null;
        }

        /// <summary>
        /// 이벤트 개수.
        /// </summary>
        public static int Count
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }
                return _events.Count;
            }
        }

        /// <summary>
        /// 데이터베이스 리로드. 에디터에서 사용.
        /// </summary>
        public static void Reload()
        {
            _initialized = false;
            Initialize();
        }
    }
}
