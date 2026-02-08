using System.Collections.Generic;
using UnityEngine;
using LoveSimulation.Events;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 런타임 호감도/플래그 데이터 관리. 단일 진실의 원천.
    /// </summary>
    public static class GameData
    {
        private static readonly Dictionary<string, int> _affection = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> _maxAffection = new Dictionary<string, int>();
        private static readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private static int _diamonds;

        private const int DefaultMaxAffection = 100;
        private const int MinAffection = 0;

        /// <summary>
        /// 캐릭터별 최대 호감도 등록. CharacterData에서 호출.
        /// </summary>
        public static void SetMaxAffection(string characterId, int max)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return;
            }

            _maxAffection[characterId] = max;
        }

        /// <summary>
        /// 캐릭터 최대 호감도 조회.
        /// </summary>
        public static int GetMaxAffection(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return DefaultMaxAffection;
            }

            return _maxAffection.TryGetValue(characterId, out int max) ? max : DefaultMaxAffection;
        }

        /// <summary>
        /// 캐릭터 호감도 변경. 클램핑 적용 및 이벤트 발행.
        /// </summary>
        public static void AddAffection(string characterId, int amount)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogWarning("[GameData] 빈 characterId로 호감도 변경 시도 무시.");
                return;
            }

            if (!_affection.ContainsKey(characterId))
            {
                _affection[characterId] = 0;
            }

            int previousValue = _affection[characterId];
            int maxAffection = GetMaxAffection(characterId);
            int newValue = Mathf.Clamp(previousValue + amount, MinAffection, maxAffection);

            _affection[characterId] = newValue;

            int actualDelta = newValue - previousValue;
            Debug.Log($"[GameData] 호감도 변경: {characterId} {(actualDelta >= 0 ? "+" : "")}{actualDelta} → {newValue}");

            PublishAffectionEvents(characterId, previousValue, newValue, actualDelta);
        }

        /// <summary>
        /// 캐릭터 호감도 절대값 설정. 로드 시 사용.
        /// </summary>
        public static void SetAffection(string characterId, int value)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogWarning("[GameData] 빈 characterId로 호감도 설정 시도 무시.");
                return;
            }

            int maxAffection = GetMaxAffection(characterId);
            int previousValue = _affection.TryGetValue(characterId, out int current) ? current : 0;
            int newValue = Mathf.Clamp(value, MinAffection, maxAffection);

            _affection[characterId] = newValue;

            if (previousValue != newValue)
            {
                PublishAffectionEvents(characterId, previousValue, newValue, newValue - previousValue);
            }
        }

        /// <summary>
        /// 호감도 변경 이벤트 발행. 레벨 변경 시 추가 이벤트 발행.
        /// </summary>
        private static void PublishAffectionEvents(string characterId, int previousValue, int newValue, int delta)
        {
            EventBus.Publish(new AffectionChanged
            {
                CharacterId = characterId,
                PreviousValue = previousValue,
                NewValue = newValue,
                Delta = delta
            });

            // 레벨 변경 체크 (CharacterDatabase가 초기화된 경우에만)
            string previousLevel = GetAffectionLevelName(characterId, previousValue);
            string newLevel = GetAffectionLevelName(characterId, newValue);

            if (!string.IsNullOrEmpty(previousLevel) && previousLevel != newLevel)
            {
                EventBus.Publish(new AffectionLevelChanged
                {
                    CharacterId = characterId,
                    PreviousLevel = previousLevel,
                    NewLevel = newLevel
                });

                Debug.Log($"[GameData] 호감도 레벨 변경: {characterId} [{previousLevel}] → [{newLevel}]");
            }
        }

        /// <summary>
        /// CharacterData를 통한 레벨명 조회. CharacterDatabase 미초기화 시 빈 문자열 반환.
        /// </summary>
        private static string GetAffectionLevelName(string characterId, int affection)
        {
            // CharacterDatabase에 직접 의존하지 않고, 있으면 사용
            var characterData = Data.CharacterDatabase.GetCharacter(characterId);
            if (characterData == null)
            {
                return string.Empty;
            }

            return characterData.GetLevelName(affection);
        }

        /// <summary>
        /// 캐릭터 호감도 조회. 없으면 0 반환.
        /// </summary>
        public static int GetAffection(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return 0;
            }

            return _affection.TryGetValue(characterId, out int value) ? value : 0;
        }

        /// <summary>
        /// 다이아몬드 현재 수량 조회.
        /// </summary>
        public static int GetDiamonds()
        {
            return _diamonds;
        }

        /// <summary>
        /// 다이아몬드 추가. CurrencyChanged 이벤트 발행.
        /// </summary>
        public static void AddDiamonds(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("[GameData] 0 이하의 다이아몬드 추가 시도 무시.");
                return;
            }

            int previousValue = _diamonds;
            _diamonds += amount;

            Debug.Log($"[GameData] 다이아몬드 추가: +{amount} → {_diamonds}");

            EventBus.Publish(new CurrencyChanged
            {
                PreviousValue = previousValue,
                NewValue = _diamonds,
                Delta = amount
            });
        }

        /// <summary>
        /// 다이아몬드 소모. 잔액 부족 시 false 반환.
        /// </summary>
        public static bool SpendDiamonds(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("[GameData] 0 이하의 다이아몬드 소모 시도 무시.");
                return false;
            }

            if (_diamonds < amount)
            {
                Debug.LogWarning($"[GameData] 다이아몬드 부족: 보유 {_diamonds}, 필요 {amount}");
                return false;
            }

            int previousValue = _diamonds;
            _diamonds -= amount;

            Debug.Log($"[GameData] 다이아몬드 소모: -{amount} → {_diamonds}");

            EventBus.Publish(new CurrencyChanged
            {
                PreviousValue = previousValue,
                NewValue = _diamonds,
                Delta = -amount
            });

            return true;
        }

        /// <summary>
        /// 플래그 설정.
        /// </summary>
        public static void SetFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName))
            {
                Debug.LogWarning("[GameData] 빈 flagName 설정 시도 무시.");
                return;
            }

            _flags[flagName] = true;
            Debug.Log($"[GameData] 플래그 설정: {flagName}");
        }

        /// <summary>
        /// 플래그 조회. 없으면 false 반환.
        /// </summary>
        public static bool GetFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName))
            {
                return false;
            }

            return _flags.TryGetValue(flagName, out bool value) && value;
        }

        /// <summary>
        /// 현재 데이터를 SaveData에 내보내기.
        /// </summary>
        public static void ExportTo(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[GameData] null SaveData에 내보내기 시도.");
                return;
            }

            data.AffectionData = new Dictionary<string, int>(_affection);
            data.Flags = new Dictionary<string, bool>(_flags);
            data.Diamonds = _diamonds;
        }

        /// <summary>
        /// SaveData에서 데이터 가져오기.
        /// </summary>
        public static void ImportFrom(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[GameData] null SaveData에서 가져오기 시도.");
                return;
            }

            _affection.Clear();
            _flags.Clear();
            _diamonds = data.Diamonds;

            if (data.AffectionData != null)
            {
                foreach (var pair in data.AffectionData)
                {
                    _affection[pair.Key] = pair.Value;
                }
            }

            if (data.Flags != null)
            {
                foreach (var pair in data.Flags)
                {
                    _flags[pair.Key] = pair.Value;
                }
            }

            Debug.Log($"[GameData] 데이터 로드 완료. 호감도: {_affection.Count}건, 플래그: {_flags.Count}건");
        }

        /// <summary>
        /// 모든 데이터 초기화.
        /// </summary>
        public static void Reset()
        {
            _affection.Clear();
            _maxAffection.Clear();
            _flags.Clear();
            _diamonds = 0;
            Debug.Log("[GameData] 데이터 초기화 완료.");
        }
    }
}
