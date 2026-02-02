using UnityEditor;
using UnityEngine;

/// <summary>
/// 프로젝트 해상도를 세로형(1080x1920)으로 변경하는 에디터 메뉴.
/// </summary>
public static class SetupPortraitResolution
{
    [MenuItem("LoveSimulation/Set Portrait Resolution (1080x1920)")]
    public static void SetPortrait()
    {
        PlayerSettings.defaultScreenWidth = 1080;
        PlayerSettings.defaultScreenHeight = 1920;

        // Game 뷰 해상도 힌트 로그
        Debug.Log("[Setup] 해상도 설정 완료: 1080x1920 (세로형). Game 뷰에서 해상도를 1080x1920으로 설정하세요.");
    }
}
