using System.Collections.Generic;
using UnityEngine;
using LoveSimulation.Core;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 전체 캐릭터 데이터 관리. Resources/CharacterData에서 로드.
    /// </summary>
    public static class CharacterDatabase
    {
        private static readonly Dictionary<string, CharacterData> _characters = new Dictionary<string, CharacterData>();
        private static bool _initialized;

        /// <summary>
        /// Resources/CharacterData 폴더에서 모든 CharacterData 로드.
        /// </summary>
        public static void Initialize()
        {
            _characters.Clear();

            CharacterData[] allData = Resources.LoadAll<CharacterData>("CharacterData");
            foreach (var data in allData)
            {
                if (string.IsNullOrEmpty(data.CharacterId))
                {
                    Debug.LogWarning($"[CharacterDatabase] CharacterId가 비어있는 SO 무시: {data.name}");
                    continue;
                }

                if (_characters.ContainsKey(data.CharacterId))
                {
                    Debug.LogWarning($"[CharacterDatabase] 중복 CharacterId: {data.CharacterId}");
                    continue;
                }

                _characters[data.CharacterId] = data;
                GameData.SetMaxAffection(data.CharacterId, data.MaxAffection);
            }

            _initialized = true;
            Debug.Log($"[CharacterDatabase] 초기화 완료. {_characters.Count}명 로드.");
        }

        /// <summary>
        /// 캐릭터 데이터 조회. 미초기화 시 자동 초기화.
        /// </summary>
        public static CharacterData GetCharacter(string characterId)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(characterId))
            {
                return null;
            }

            _characters.TryGetValue(characterId, out CharacterData data);
            return data;
        }

        /// <summary>
        /// 현재 호감도 기반으로 캐릭터의 레벨명 반환.
        /// </summary>
        public static string GetAffectionLevel(string characterId)
        {
            EnsureInitialized();

            CharacterData data = GetCharacter(characterId);
            if (data == null)
            {
                return string.Empty;
            }

            int affection = GameData.GetAffection(characterId);
            return data.GetLevelName(affection);
        }

        /// <summary>
        /// 등록된 모든 캐릭터 ID 반환.
        /// </summary>
        public static IEnumerable<string> GetAllCharacterIds()
        {
            EnsureInitialized();
            return _characters.Keys;
        }

        /// <summary>
        /// 초기화 여부 확인 후 필요 시 자동 초기화.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 데이터베이스 리셋. 테스트 용도.
        /// </summary>
        public static void Reset()
        {
            _characters.Clear();
            _initialized = false;
        }
    }
}
