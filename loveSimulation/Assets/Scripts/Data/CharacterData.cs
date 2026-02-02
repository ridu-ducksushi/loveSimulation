using UnityEngine;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 캐릭터 정적 데이터. ScriptableObject 기반.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "LoveSimulation/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string _characterId;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 4)] private string _description;

        [Header("호감도 설정")]
        [SerializeField] private int _maxAffection = 100;
        [SerializeField] private AffectionLevel[] _affectionLevels = new AffectionLevel[]
        {
            new AffectionLevel { LevelName = "낯선 사이", Threshold = 0 },
            new AffectionLevel { LevelName = "지인", Threshold = 20 },
            new AffectionLevel { LevelName = "친구", Threshold = 40 },
            new AffectionLevel { LevelName = "호감", Threshold = 60 },
            new AffectionLevel { LevelName = "연인", Threshold = 80 }
        };

        [Header("비주얼")]
        [SerializeField] private Sprite _portrait;

        public string CharacterId => _characterId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public int MaxAffection => _maxAffection;
        public AffectionLevel[] AffectionLevels => _affectionLevels;
        public Sprite Portrait => _portrait;

        /// <summary>
        /// 호감도 값으로 레벨명 반환. 임계값 내림차순으로 검색.
        /// </summary>
        public string GetLevelName(int affection)
        {
            if (_affectionLevels == null || _affectionLevels.Length == 0)
            {
                return string.Empty;
            }

            // 높은 임계값부터 검색하여 첫 번째 일치 반환
            string result = _affectionLevels[0].LevelName;
            for (int i = 0; i < _affectionLevels.Length; i++)
            {
                if (affection >= _affectionLevels[i].Threshold)
                {
                    result = _affectionLevels[i].LevelName;
                }
            }

            return result;
        }
    }
}
