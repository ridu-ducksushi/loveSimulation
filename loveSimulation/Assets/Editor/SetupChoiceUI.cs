using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using LoveSimulation.Dialogue;

/// <summary>
/// ChoicePanel 및 ChoiceButton 프리팹 자동 생성 에디터 메뉴.
/// </summary>
public static class SetupChoiceUI
{
    private const string ChoiceButtonPrefabPath = "Assets/Prefabs/UI/ChoiceButton.prefab";
    private const string PrefabFolderPath = "Assets/Prefabs/UI";

    [MenuItem("LoveSimulation/Setup ChoiceUI in Scene")]
    public static void Setup()
    {
        // DialogueCanvas 찾기
        var dialogueCanvas = GameObject.Find("DialogueCanvas");
        if (dialogueCanvas == null)
        {
            Debug.LogError("[SetupChoiceUI] DialogueCanvas가 씬에 없습니다. 먼저 Setup DialogueCanvas를 실행하세요.");
            return;
        }

        // 기존 ChoicePanel 제거
        var existingPanel = dialogueCanvas.transform.Find("ChoicePanel");
        if (existingPanel != null)
        {
            Object.DestroyImmediate(existingPanel.gameObject);
        }

        // ChoiceButton 프리팹 생성 (없으면)
        GameObject buttonPrefab = CreateChoiceButtonPrefab();

        // ChoicePanel 생성 (대화 패널 위에 배치)
        var choicePanelGo = CreateUIElement("ChoicePanel", dialogueCanvas.transform);
        Undo.RegisterCreatedObjectUndo(choicePanelGo, "Create ChoicePanel");

        var panelRect = choicePanelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.15f);
        panelRect.anchorMax = new Vector2(0.9f, 0.55f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 반투명 배경
        var panelImage = choicePanelGo.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        // VerticalLayoutGroup
        var layout = choicePanelGo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20f;
        layout.padding = new RectOffset(40, 40, 30, 30);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // ContentSizeFitter
        var fitter = choicePanelGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ChoiceUI 컴포넌트 부착
        var choiceUI = dialogueCanvas.GetComponent<ChoiceUI>();
        if (choiceUI == null)
        {
            choiceUI = dialogueCanvas.AddComponent<ChoiceUI>();
        }

        SetSerializedField(choiceUI, "_choicePanel", choicePanelGo);
        SetSerializedField(choiceUI, "_choiceButtonParent", choicePanelGo.transform);
        SetSerializedField(choiceUI, "_choiceButtonPrefab", buttonPrefab);

        // 초기 상태: 비활성화
        choicePanelGo.SetActive(false);

        Selection.activeGameObject = choicePanelGo;
        EditorUtility.SetDirty(dialogueCanvas);

        // EventSystem이 없으면 생성 (UI 버튼 클릭에 필수)
        EnsureEventSystem();

        Debug.Log("[SetupChoiceUI] ChoicePanel 생성 완료. ChoiceUI 컴포넌트 연결됨.");
    }

    /// <summary>
    /// EventSystem이 씬에 없으면 생성. New Input System 용 InputSystemUIInputModule 사용.
    /// </summary>
    private static void EnsureEventSystem()
    {
        var existingES = Object.FindFirstObjectByType<EventSystem>();
        if (existingES != null)
        {
            // InputSystemUIInputModule 확인
            if (existingES.GetComponent<InputSystemUIInputModule>() == null)
            {
                // 구형 StandaloneInputModule 제거 후 교체
                var oldModule = existingES.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Object.DestroyImmediate(oldModule);
                }
                existingES.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[SetupChoiceUI] InputSystemUIInputModule 추가됨.");
            }
            return;
        }

        var esGo = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        Debug.Log("[SetupChoiceUI] EventSystem 생성 완료 (InputSystemUIInputModule).");
    }

    /// <summary>
    /// ChoiceButton 프리팹 생성. 이미 존재하면 기존 것 반환.
    /// </summary>
    private static GameObject CreateChoiceButtonPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ChoiceButtonPrefabPath);
        if (existing != null)
        {
            Debug.Log("[SetupChoiceUI] 기존 ChoiceButton 프리팹 사용.");
            return existing;
        }

        // 폴더 생성
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // 버튼 GameObject 생성
        var buttonGo = new GameObject("ChoiceButton", typeof(RectTransform));

        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0f, 80f);

        // 버튼 배경
        var buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.3f, 0.9f);

        var button = buttonGo.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        button.colors = colors;

        // 버튼 텍스트
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(buttonGo.transform, false);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "선택지";
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 5f);
        textRect.offsetMax = new Vector2(-20f, -5f);

        // 프리팹으로 저장
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonGo, ChoiceButtonPrefabPath);
        Object.DestroyImmediate(buttonGo);

        Debug.Log($"[SetupChoiceUI] ChoiceButton 프리팹 생성: {ChoiceButtonPrefabPath}");
        return prefab;
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetSerializedField(Component component, string fieldName, Object value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning($"[SetupChoiceUI] 필드를 찾을 수 없음: {fieldName}");
        }
    }
}
