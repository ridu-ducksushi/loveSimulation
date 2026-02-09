using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using LoveSimulation.Core;
using LoveSimulation.Dialogue;

namespace LoveSimulation.DebugTools
{
    /// <summary>
    /// 디버그용 에피소드 선택 패널. 로비에서 chapter01~05를 직접 선택하여 재생.
    /// </summary>
    public class DebugEpisodeSelectUI : MonoBehaviour
    {
        private const int ChapterCount = 5;
        private const float ButtonHeight = 120f;
        private const int FontSize = 32;
        [SerializeField] private Button _devButton;
        [SerializeField] private GameObject _selectPanel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private TMP_FontAsset _fontAsset;

        private void Awake()
        {
            Debug.Log($"[DebugEpisodeSelectUI] Awake - devButton:{_devButton != null}, panel:{_selectPanel != null}, container:{_buttonContainer != null}");

            if (_selectPanel != null)
            {
                _selectPanel.SetActive(false);
            }
        }

        private void Start()
        {
            SetupDevButton();
            CreateEpisodeButtons();
        }

        /// <summary>
        /// 개발자 버튼 클릭 시 패널 토글 설정.
        /// </summary>
        private void SetupDevButton()
        {
            if (_devButton == null)
            {
                Debug.LogWarning("[DebugEpisodeSelectUI] _devButton이 할당되지 않음.");
                return;
            }

            _devButton.onClick.AddListener(TogglePanel);
        }

        /// <summary>
        /// 에피소드 선택 패널 표시/숨김 토글.
        /// </summary>
        private void TogglePanel()
        {
            if (_selectPanel == null)
            {
                Debug.LogWarning("[DebugEpisodeSelectUI] _selectPanel이 null.");
                return;
            }

            bool newState = !_selectPanel.activeSelf;
            _selectPanel.SetActive(newState);
            Debug.Log($"[DebugEpisodeSelectUI] 패널 토글 → {newState}");

            // 비활성 상태에서 생성된 버튼의 레이아웃 강제 재계산
            if (newState && _buttonContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonContainer as RectTransform);
            }
        }

        /// <summary>
        /// chapter01~05 JSON을 로드하여 동적 버튼 생성.
        /// </summary>
        private void CreateEpisodeButtons()
        {
            if (_buttonContainer == null)
            {
                Debug.LogError("[DebugEpisodeSelectUI] _buttonContainer가 할당되지 않음.");
                return;
            }

            int createdCount = 0;

            for (int i = 1; i <= ChapterCount; i++)
            {
                string chapterId = $"chapter{i:D2}";
                string subtitle = LoadChapterSubtitle(chapterId);

                if (subtitle == null)
                {
                    continue;
                }

                bool isCompleted = GameData.GetFlag($"{chapterId}_completed");
                string label = isCompleted
                    ? $"\u2713 EP {i:D2} - {subtitle}"
                    : $"EP {i:D2} - {subtitle}";

                CreateButton(chapterId, label);
                createdCount++;
            }

            Debug.Log($"[DebugEpisodeSelectUI] 에피소드 버튼 {createdCount}개 생성 완료.");
        }

        /// <summary>
        /// JSON에서 챕터 부제목(ChapterTitle의 \n 이후 텍스트)을 추출.
        /// </summary>
        private string LoadChapterSubtitle(string chapterId)
        {
            TextAsset json = Resources.Load<TextAsset>($"Dialogues/{chapterId}");

            if (json == null)
            {
                Debug.LogWarning($"[DebugEpisodeSelectUI] {chapterId} JSON 로드 실패.");
                return null;
            }

            DialogueData data = JsonConvert.DeserializeObject<DialogueData>(json.text);

            if (data == null || string.IsNullOrEmpty(data.ChapterTitle))
            {
                return chapterId;
            }

            // ChapterTitle 형식: "Chapter01\n낯선 손님" → "\n" 이후 부분 추출
            string title = data.ChapterTitle;
            int newlineIndex = title.IndexOf('\n');

            return newlineIndex >= 0 && newlineIndex < title.Length - 1
                ? title.Substring(newlineIndex + 1)
                : title;
        }

        /// <summary>
        /// 에피소드 선택 버튼 하나를 동적으로 생성.
        /// </summary>
        private void CreateButton(string chapterId, string label)
        {
            // 버튼 GameObject 생성
            GameObject buttonObj = new GameObject($"Btn_{chapterId}", typeof(RectTransform));
            buttonObj.layer = gameObject.layer;
            buttonObj.transform.SetParent(_buttonContainer, false);

            // Image (Button 배경)
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

            // Button 컴포넌트
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // LayoutElement로 높이 고정
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredHeight = ButtonHeight;

            // 텍스트 자식 오브젝트 생성
            CreateButtonText(buttonObj.transform, label);

            // 클릭 이벤트 바인딩
            string capturedId = chapterId;
            button.onClick.AddListener(() => OnEpisodeSelected(capturedId));
        }

        /// <summary>
        /// 버튼 내부 TextMeshProUGUI 자식 생성.
        /// </summary>
        private void CreateButtonText(Transform parent, string label)
        {
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.layer = gameObject.layer;
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = FontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            // 한글 폰트 할당
            if (_fontAsset != null)
            {
                tmp.font = _fontAsset;
            }

            // RectTransform을 부모에 꽉 채움
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 에피소드 선택 시 대화 시작 및 패널 닫기.
        /// </summary>
        private void OnEpisodeSelected(string chapterId)
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DebugEpisodeSelectUI] DialogueManager 인스턴스 없음.");
                return;
            }

            Debug.Log($"[DebugEpisodeSelectUI] 에피소드 선택: {chapterId}");
            _selectPanel.SetActive(false);
            DialogueManager.Instance.StartDialogue(chapterId);
        }
    }
}
