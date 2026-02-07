using System.Collections.Generic;
using UnityEngine;
using LoveSimulation.Events;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 시간/장소/일차 상태 관리. 단일 진실의 원천.
    /// </summary>
    public static class WorldState
    {
        private static int _currentDay = 1;
        private static TimeOfDay _currentTimeOfDay = TimeOfDay.Morning;
        private static Location _currentLocation = Location.Home;
        private static HashSet<string> _triggeredEvents = new HashSet<string>();

        private const int TimeOfDayCount = 4;

        public static int CurrentDay => _currentDay;
        public static TimeOfDay CurrentTimeOfDay => _currentTimeOfDay;
        public static Location CurrentLocation => _currentLocation;

        /// <summary>
        /// 시간을 다음 시간대로 진행. 밤 이후는 다음 날 아침으로.
        /// </summary>
        public static void AdvanceTime()
        {
            TimeOfDay previousTime = _currentTimeOfDay;
            int previousDay = _currentDay;

            int nextTimeIndex = ((int)_currentTimeOfDay + 1) % TimeOfDayCount;
            _currentTimeOfDay = (TimeOfDay)nextTimeIndex;

            // 밤 → 아침 전환 시 일차 증가
            if (previousTime == TimeOfDay.Night)
            {
                _currentDay++;

                Debug.Log($"[WorldState] 일차 변경: {previousDay}일 → {_currentDay}일");

                EventBus.Publish(new DayChanged
                {
                    PreviousDay = previousDay,
                    NewDay = _currentDay
                });
            }

            Debug.Log($"[WorldState] 시간 진행: {previousTime} → {_currentTimeOfDay}");

            EventBus.Publish(new TimeOfDayChanged
            {
                PreviousTime = previousTime,
                NewTime = _currentTimeOfDay,
                CurrentDay = _currentDay
            });
        }

        /// <summary>
        /// 특정 시간대로 설정. 이벤트 발행.
        /// </summary>
        public static void SetTimeOfDay(TimeOfDay time)
        {
            if (_currentTimeOfDay == time)
            {
                return;
            }

            TimeOfDay previousTime = _currentTimeOfDay;
            _currentTimeOfDay = time;

            Debug.Log($"[WorldState] 시간대 설정: {previousTime} → {_currentTimeOfDay}");

            EventBus.Publish(new TimeOfDayChanged
            {
                PreviousTime = previousTime,
                NewTime = _currentTimeOfDay,
                CurrentDay = _currentDay
            });
        }

        /// <summary>
        /// 장소 변경. 이벤트 발행.
        /// </summary>
        public static void SetLocation(Location location)
        {
            if (_currentLocation == location)
            {
                return;
            }

            Location previousLocation = _currentLocation;
            _currentLocation = location;

            Debug.Log($"[WorldState] 장소 변경: {previousLocation} → {_currentLocation}");

            EventBus.Publish(new LocationChanged
            {
                PreviousLocation = previousLocation,
                NewLocation = _currentLocation
            });
        }

        /// <summary>
        /// 일차 설정. 로드 시 사용.
        /// </summary>
        public static void SetDay(int day)
        {
            if (day < 1)
            {
                Debug.LogWarning("[WorldState] 잘못된 일차 값 무시: " + day);
                return;
            }

            int previousDay = _currentDay;
            _currentDay = day;

            if (previousDay != _currentDay)
            {
                EventBus.Publish(new DayChanged
                {
                    PreviousDay = previousDay,
                    NewDay = _currentDay
                });
            }
        }

        /// <summary>
        /// 이벤트 트리거 기록.
        /// </summary>
        public static void MarkEventTriggered(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return;
            }

            _triggeredEvents.Add(eventId);
            Debug.Log($"[WorldState] 이벤트 트리거 기록: {eventId}");
        }

        /// <summary>
        /// 이벤트가 이미 트리거되었는지 확인.
        /// </summary>
        public static bool IsEventTriggered(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return false;
            }

            return _triggeredEvents.Contains(eventId);
        }

        /// <summary>
        /// SaveData에 현재 상태 내보내기.
        /// </summary>
        public static void ExportTo(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[WorldState] null SaveData에 내보내기 시도.");
                return;
            }

            data.CurrentDay = _currentDay;
            data.CurrentTimeOfDay = (int)_currentTimeOfDay;
            data.CurrentLocation = (int)_currentLocation;
            data.TriggeredEvents = new List<string>(_triggeredEvents);
        }

        /// <summary>
        /// SaveData에서 상태 가져오기.
        /// </summary>
        public static void ImportFrom(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[WorldState] null SaveData에서 가져오기 시도.");
                return;
            }

            _currentDay = data.CurrentDay > 0 ? data.CurrentDay : 1;
            _currentTimeOfDay = (TimeOfDay)data.CurrentTimeOfDay;
            _currentLocation = (Location)data.CurrentLocation;

            _triggeredEvents.Clear();
            if (data.TriggeredEvents != null)
            {
                foreach (string eventId in data.TriggeredEvents)
                {
                    _triggeredEvents.Add(eventId);
                }
            }

            Debug.Log($"[WorldState] 상태 로드 완료. {_currentDay}일차, {_currentTimeOfDay}, {_currentLocation}");
        }

        /// <summary>
        /// 상태 초기화.
        /// </summary>
        public static void Reset()
        {
            _currentDay = 1;
            _currentTimeOfDay = TimeOfDay.Morning;
            _currentLocation = Location.Home;
            _triggeredEvents.Clear();

            Debug.Log("[WorldState] 상태 초기화 완료.");
        }
    }
}
