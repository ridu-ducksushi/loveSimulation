using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 로비 대사 레지스트리. Resources/LobbyDialogues 폴더에서 자동 로드.
    /// </summary>
    public static class LobbyDialogueDatabase
    {
        private static List<LobbyDialogueData> _dialogues = new List<LobbyDialogueData>();
        private static bool _initialized = false;

        private const string ResourcePath = "LobbyDialogues";

        /// <summary>
        /// 데이터베이스 초기화.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _dialogues.Clear();

            LobbyDialogueData[] loaded = Resources.LoadAll<LobbyDialogueData>(ResourcePath);

            foreach (LobbyDialogueData data in loaded)
            {
                if (data != null)
                {
                    _dialogues.Add(data);
                }
            }

            // 우선순위 내림차순 정렬
            _dialogues = _dialogues.OrderByDescending(d => d.Priority).ToList();
            _initialized = true;

            Debug.Log($"[LobbyDialogueDatabase] 초기화 완료. {_dialogues.Count}개 대사 그룹 로드.");
        }

        /// <summary>
        /// 조건 충족하는 최고 우선순위 그룹의 대사 풀 반환.
        /// 같은 우선순위 그룹이 여러 개면 대사를 합산.
        /// </summary>
        public static string[] GetAvailableLines()
        {
            if (!_initialized)
            {
                Initialize();
            }

            int topPriority = int.MinValue;
            List<string> lines = new List<string>();

            foreach (LobbyDialogueData data in _dialogues)
            {
                if (!data.IsAvailable())
                {
                    continue;
                }

                // 첫 번째 충족 그룹의 우선순위 기록
                if (topPriority == int.MinValue)
                {
                    topPriority = data.Priority;
                }

                // 같은 우선순위만 합산
                if (data.Priority < topPriority)
                {
                    break;
                }

                lines.AddRange(data.Lines);
            }

            return lines.ToArray();
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
