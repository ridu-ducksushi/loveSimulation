using UnityEditor;
using UnityEngine;
using LoveSimulation.Data;

public static class SetupAffectionSystem
{
    [MenuItem("LoveSimulation/Create Sample CharacterData")]
    public static void CreateSampleCharacterData()
    {
        string folderPath = "Assets/Resources/CharacterData";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "CharacterData");
        }

        string assetPath = $"{folderPath}/Yuna.asset";

        // 기존 에셋이 있으면 덮어쓰지 않음
        var existing = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
        if (existing != null)
        {
            Debug.Log("[SetupAffection] 이미 Yuna.asset이 존재합니다.");
            Selection.activeObject = existing;
            return;
        }

        var characterData = ScriptableObject.CreateInstance<CharacterData>();

        // SerializedObject를 통해 private 필드 설정
        var so = new SerializedObject(characterData);
        so.FindProperty("_characterId").stringValue = "유나";
        so.FindProperty("_displayName").stringValue = "유나";
        so.FindProperty("_description").stringValue = "밝고 활발한 성격의 소꿉친구.";
        so.FindProperty("_maxAffection").intValue = 100;

        var levels = so.FindProperty("_affectionLevels");
        levels.ClearArray();

        AddAffectionLevel(levels, "낯선 사이", 0);
        AddAffectionLevel(levels, "지인", 20);
        AddAffectionLevel(levels, "친구", 40);
        AddAffectionLevel(levels, "호감", 60);
        AddAffectionLevel(levels, "연인", 80);

        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(characterData, assetPath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = characterData;
        Debug.Log($"[SetupAffection] Yuna CharacterData 생성 완료: {assetPath}");
    }

    private static void AddAffectionLevel(SerializedProperty array, string levelName, int threshold)
    {
        int index = array.arraySize;
        array.InsertArrayElementAtIndex(index);
        var element = array.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("LevelName").stringValue = levelName;
        element.FindPropertyRelative("Threshold").intValue = threshold;
    }
}
