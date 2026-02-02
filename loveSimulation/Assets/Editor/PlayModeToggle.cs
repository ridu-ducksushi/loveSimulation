using UnityEditor;
using UnityEngine;

public static class PlayModeToggle
{
    [MenuItem("LoveSimulation/Enter Play Mode")]
    public static void EnterPlayMode()
    {
        EditorApplication.delayCall += () =>
        {
            EditorApplication.isPlaying = true;
        };
        Debug.Log("[PlayModeToggle] 플레이 모드 진입 예약됨.");
    }

    [MenuItem("LoveSimulation/Exit Play Mode")]
    public static void ExitPlayMode()
    {
        EditorApplication.delayCall += () =>
        {
            EditorApplication.isPlaying = false;
        };
        Debug.Log("[PlayModeToggle] 플레이 모드 종료 예약됨.");
    }
}
