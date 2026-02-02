using UnityEditor;
using UnityEngine;
using LoveSimulation.Core;

public static class SetupGameManager
{
    [MenuItem("LoveSimulation/Setup GameManager in Scene")]
    public static void Setup()
    {
        // 기존 GameManager 오브젝트 찾기 또는 생성
        var existing = GameObject.Find("GameManager");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var go = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(go, "Create GameManager");

        go.AddComponent<GameManager>();
        go.AddComponent<SaveLoadManager>();
        go.AddComponent<SceneTransitionManager>();

        Selection.activeGameObject = go;
        EditorUtility.SetDirty(go);

        Debug.Log("[Setup] GameManager 오브젝트 생성 완료. 컴포넌트 3개 부착됨.");
    }
}
