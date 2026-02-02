using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Dialogue;

/// <summary>
/// DialogueCanvas 씬 자동 생성 에디터 메뉴.
/// </summary>
public static class SetupDialogueUI
{
    [MenuItem("LoveSimulation/Setup DialogueCanvas in Scene")]
    public static void Setup()
    {
        // 기존 DialogueCanvas 제거
        var existing = GameObject.Find("DialogueCanvas");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        // Canvas 생성
        var canvasGo = new GameObject("DialogueCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create DialogueCanvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // DialoguePanel (하단 반투명 검정 배경)
        var panelGo = CreateUIElement("DialoguePanel", canvasGo.transform);
        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        var panelRect = panelGo.GetComponent<RectTransform>();
        SetAnchorsStretchBottom(panelRect, 250f);

        // SpeakerPanel (화자 이름 배경, 좌상단)
        var speakerPanelGo = CreateUIElement("SpeakerPanel", panelGo.transform);
        var speakerBg = speakerPanelGo.AddComponent<Image>();
        speakerBg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var speakerPanelRect = speakerPanelGo.GetComponent<RectTransform>();
        speakerPanelRect.anchorMin = new Vector2(0f, 1f);
        speakerPanelRect.anchorMax = new Vector2(0f, 1f);
        speakerPanelRect.pivot = new Vector2(0f, 0f);
        speakerPanelRect.anchoredPosition = new Vector2(30f, 5f);
        speakerPanelRect.sizeDelta = new Vector2(250f, 45f);

        // SpeakerText (TMP)
        var speakerTextGo = CreateUIElement("SpeakerText", speakerPanelGo.transform);
        var speakerTmp = speakerTextGo.AddComponent<TextMeshProUGUI>();
        speakerTmp.text = "화자 이름";
        speakerTmp.fontSize = 28;
        speakerTmp.alignment = TextAlignmentOptions.Center;
        speakerTmp.color = Color.white;

        var speakerTextRect = speakerTextGo.GetComponent<RectTransform>();
        StretchFull(speakerTextRect);

        // DialogueText (본문 TMP)
        var dialogueTextGo = CreateUIElement("DialogueText", panelGo.transform);
        var dialogueTmp = dialogueTextGo.AddComponent<TextMeshProUGUI>();
        dialogueTmp.text = "대화 내용이 여기에 표시됩니다.";
        dialogueTmp.fontSize = 32;
        dialogueTmp.alignment = TextAlignmentOptions.TopLeft;
        dialogueTmp.color = Color.white;

        var dialogueTextRect = dialogueTextGo.GetComponent<RectTransform>();
        StretchFull(dialogueTextRect);
        dialogueTextRect.offsetMin = new Vector2(40f, 30f);
        dialogueTextRect.offsetMax = new Vector2(-40f, -55f);

        // DialogueUI 컴포넌트 부착 및 참조 연결
        var dialogueUI = canvasGo.AddComponent<DialogueUI>();
        SetSerializedField(dialogueUI, "_dialoguePanel", panelGo);
        SetSerializedField(dialogueUI, "_speakerPanel", speakerPanelGo);
        SetSerializedField(dialogueUI, "_speakerText", speakerTmp);
        SetSerializedField(dialogueUI, "_dialogueText", dialogueTmp);

        // 초기 상태: 패널 비활성화
        panelGo.SetActive(false);

        Selection.activeGameObject = canvasGo;
        EditorUtility.SetDirty(canvasGo);

        Debug.Log("[Setup] DialogueCanvas 생성 완료. DialogueUI 컴포넌트 연결됨.");
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>
    /// 하단에 고정되는 스트레치 앵커 설정.
    /// </summary>
    private static void SetAnchorsStretchBottom(RectTransform rect, float height)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
    }

    /// <summary>
    /// 부모를 가득 채우는 앵커 설정.
    /// </summary>
    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// SerializedObject를 통해 private 필드에 값 설정.
    /// </summary>
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
            Debug.LogWarning($"[SetupDialogueUI] 필드를 찾을 수 없음: {fieldName}");
        }
    }
}
