using UnityEngine;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 로비 캐릭터 말풍선 대사 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLobbyDialogue", menuName = "LoveSimulation/Lobby Dialogue")]
    public class LobbyDialogueData : ScriptableObject
    {
        [Header("발동 조건")]
        [SerializeField] private EventCondition _condition = new EventCondition();

        [Header("우선순위 (높을수록 우선)")]
        [SerializeField] private int _priority = 0;

        [Header("대사 목록")]
        [SerializeField] [TextArea(1, 3)] private string[] _lines;

        public EventCondition Condition => _condition;
        public int Priority => _priority;
        public string[] Lines => _lines;

        /// <summary>
        /// 조건 충족 + 대사 존재 여부 확인.
        /// </summary>
        public bool IsAvailable()
        {
            if (_lines == null || _lines.Length == 0)
            {
                return false;
            }

            if (_condition == null || _condition.IsEmpty())
            {
                return true;
            }

            return _condition.Evaluate();
        }
    }
}
