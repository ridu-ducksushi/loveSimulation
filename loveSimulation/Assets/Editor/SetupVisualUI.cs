using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LoveSimulation.Dialogue;

/// <summary>
/// 배경 및 캐릭터 스프라이트 UI 자동 생성 에디터 메뉴.
/// DialogueCanvas에 BackgroundLayer와 CharacterLayer 추가.
/// </summary>
public static class SetupVisualUI
{
    [MenuItem("LoveSimulation/Setup Visual UI (Background & Character)")]
    public static void Setup()
    {
        // DialogueCanvas 찾기
        var dialogueCanvas = GameObject.Find("DialogueCanvas");
        if (dialogueCanvas == null)
        {
            Debug.LogError("[SetupVisualUI] DialogueCanvas를 찾을 수 없음. 먼저 Setup DialogueCanvas를 실행하세요.");
            return;
        }

        var canvasTransform = dialogueCanvas.transform;

        // 기존 레이어 제거
        RemoveExistingLayer("BackgroundLayer");
        RemoveExistingLayer("CharacterLayer");

        // BackgroundLayer 생성 (Order 0 - 가장 뒤)
        var backgroundLayerGo = CreateBackgroundLayer(canvasTransform);

        // CharacterLayer 생성 (Order 10 - 배경 앞, 대화창 뒤)
        var characterLayerGo = CreateCharacterLayer(canvasTransform);

        // 하이러키 순서 조정 (BackgroundLayer를 가장 위로)
        backgroundLayerGo.transform.SetAsFirstSibling();
        characterLayerGo.transform.SetSiblingIndex(1);

        // 씬 더티 표시
        EditorUtility.SetDirty(dialogueCanvas);

        Debug.Log("[SetupVisualUI] BackgroundLayer와 CharacterLayer 생성 완료.");
        Debug.Log("  - Resources/Backgrounds/ 폴더에 배경 이미지 추가");
        Debug.Log("  - Resources/CharacterSprites/ 폴더에 캐릭터 스프라이트 추가");
        Debug.Log("  - 스프라이트 명명 규칙: {characterId}_{emotion}.png (예: adelin_smile.png)");
    }

    private static void RemoveExistingLayer(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }
    }

    private static GameObject CreateBackgroundLayer(Transform parent)
    {
        // BackgroundLayer 컨테이너
        var layerGo = CreateUIElement("BackgroundLayer", parent);
        Undo.RegisterCreatedObjectUndo(layerGo, "Create BackgroundLayer");

        var layerRect = layerGo.GetComponent<RectTransform>();
        StretchFull(layerRect);

        // BackgroundImage1
        var bg1Go = CreateUIElement("BackgroundImage1", layerGo.transform);
        var bg1Image = bg1Go.AddComponent<Image>();
        bg1Image.color = new Color(0.1f, 0.1f, 0.15f, 1f); // 기본 어두운 배경
        var bg1Rect = bg1Go.GetComponent<RectTransform>();
        StretchFull(bg1Rect);

        // BackgroundImage2 (크로스페이드용)
        var bg2Go = CreateUIElement("BackgroundImage2", layerGo.transform);
        var bg2Image = bg2Go.AddComponent<Image>();
        bg2Image.color = new Color(1f, 1f, 1f, 0f); // 투명
        var bg2Rect = bg2Go.GetComponent<RectTransform>();
        StretchFull(bg2Rect);

        // BackgroundManager 컴포넌트 부착
        var manager = layerGo.AddComponent<BackgroundManager>();
        SetSerializedField(manager, "_backgroundImage1", bg1Image);
        SetSerializedField(manager, "_backgroundImage2", bg2Image);

        return layerGo;
    }

    private static GameObject CreateCharacterLayer(Transform parent)
    {
        // CharacterLayer 컨테이너
        var layerGo = CreateUIElement("CharacterLayer", parent);
        Undo.RegisterCreatedObjectUndo(layerGo, "Create CharacterLayer");

        var layerRect = layerGo.GetComponent<RectTransform>();
        StretchFull(layerRect);

        // CenterCharacter (화면 하단 중앙)
        var centerGo = CreateUIElement("CenterCharacter", layerGo.transform);
        var centerImage = centerGo.AddComponent<Image>();
        centerImage.color = new Color(1f, 1f, 1f, 0f); // 투명
        centerImage.preserveAspect = true;
        centerImage.raycastTarget = false;

        var centerRect = centerGo.GetComponent<RectTransform>();
        // 앵커: 하단 중앙
        centerRect.anchorMin = new Vector2(0.5f, 0f);
        centerRect.anchorMax = new Vector2(0.5f, 0f);
        centerRect.pivot = new Vector2(0.5f, 0f);
        centerRect.anchoredPosition = new Vector2(0f, 250f); // 대화창 위에 위치
        centerRect.sizeDelta = new Vector2(600f, 1200f); // 기본 캐릭터 크기

        // CharacterSpriteUI 컴포넌트 부착
        var spriteUI = layerGo.AddComponent<CharacterSpriteUI>();
        SetSerializedField(spriteUI, "_characterImage", centerImage);

        return layerGo;
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
            Debug.LogWarning($"[SetupVisualUI] 필드를 찾을 수 없음: {fieldName}");
        }
    }
}
