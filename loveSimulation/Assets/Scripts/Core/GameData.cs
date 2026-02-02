using System.Collections.Generic;
using UnityEngine;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 런타임 호감도/플래그 데이터 관리. 단일 진실의 원천.
    /// </summary>
    public static class GameData
    {
        private static readonly Dictionary<string, int> _affection = new Dictionary<string, int>();
        private static readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        /// <summary>
        /// 캐릭터 호감도 변경.
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

            _affection[characterId] += amount;
            Debug.Log($"[GameData] 호감도 변경: {characterId} {(amount >= 0 ? "+" : "")}{amount} → {_affection[characterId]}");
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
            _flags.Clear();
            Debug.Log("[GameData] 데이터 초기화 완료.");
        }
    }
}
