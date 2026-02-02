using LoveSimulation.Core;

namespace LoveSimulation.Events
{
    /// <summary>
    /// 게임 상태 변경 이벤트.
    /// </summary>
    public struct GameStateChanged
    {
        public GameState PreviousState;
        public GameState NewState;
    }

    /// <summary>
    /// 씬 전환 요청 이벤트.
    /// </summary>
    public struct SceneTransitionRequested
    {
        public string SceneName;
        public float FadeDuration;
    }

    /// <summary>
    /// 씬 전환 완료 이벤트.
    /// </summary>
    public struct SceneTransitionCompleted
    {
        public string SceneName;
    }

    /// <summary>
    /// 저장 요청 이벤트.
    /// </summary>
    public struct SaveRequested
    {
        public int SlotIndex;
    }

    /// <summary>
    /// 저장 완료 이벤트.
    /// </summary>
    public struct SaveCompleted
    {
        public int SlotIndex;
        public bool Success;
    }

    /// <summary>
    /// 로드 요청 이벤트.
    /// </summary>
    public struct LoadRequested
    {
        public int SlotIndex;
    }

    /// <summary>
    /// 로드 완료 이벤트.
    /// </summary>
    public struct LoadCompleted
    {
        public int SlotIndex;
        public bool Success;
    }
}
