using UnityEngine;
using LoveSimulation.Core;
using LoveSimulation.Data;
using LoveSimulation.Events;

namespace LoveSimulation.DebugTools
{
    /// <summary>
    /// 디버그용 호감도 실시간 표시. 화면 좌상단.
    /// </summary>
    public class AffectionDebugUI : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool _showDebugUI = true;

        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;

        private void OnEnable()
        {
            EventBus.Subscribe<AffectionChanged>(OnAffectionChanged);
            EventBus.Subscribe<AffectionLevelChanged>(OnAffectionLevelChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AffectionChanged>(OnAffectionChanged);
            EventBus.Unsubscribe<AffectionLevelChanged>(OnAffectionLevelChanged);
        }

        private void OnAffectionChanged(AffectionChanged evt)
        {
            UnityEngine.Debug.Log($"[AffectionDebug] {evt.CharacterId}: {evt.PreviousValue} → {evt.NewValue} ({(evt.Delta >= 0 ? "+" : "")}{evt.Delta})");
        }

        private void OnAffectionLevelChanged(AffectionLevelChanged evt)
        {
            UnityEngine.Debug.Log($"[AffectionDebug] {evt.CharacterId} 레벨 변경: [{evt.PreviousLevel}] → [{evt.NewLevel}]");
        }

        private void InitStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            };
        }

        private void OnGUI()
        {
            if (!_showDebugUI)
            {
                return;
            }

            InitStyles();

            float x = 10f;
            float y = 10f;
            float lineHeight = 22f;

            GUI.Label(new Rect(x, y, 300, lineHeight), "[ 호감도 디버그 ]", _headerStyle);
            y += lineHeight;

            foreach (string characterId in CharacterDatabase.GetAllCharacterIds())
            {
                int affection = GameData.GetAffection(characterId);
                int maxAffection = GameData.GetMaxAffection(characterId);
                string level = CharacterDatabase.GetAffectionLevel(characterId);

                string text = $"{characterId}: {affection}/{maxAffection} [{level}]";
                GUI.Label(new Rect(x, y, 400, lineHeight), text, _labelStyle);
                y += lineHeight;
            }
        }
#endif
    }
}
