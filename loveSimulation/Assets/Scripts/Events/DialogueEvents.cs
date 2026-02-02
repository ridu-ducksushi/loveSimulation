using LoveSimulation.Core;

namespace LoveSimulation.Events
{
    /// <summary>
    /// 대화 시작 이벤트. Manager → UI.
    /// </summary>
    public struct DialogueStarted
    {
        public string DialogueId;
    }

    /// <summary>
    /// 새 대화 라인 표시 요청. Manager → UI.
    /// </summary>
    public struct DialogueLineRequested
    {
        public string Speaker;
        public string Text;
        public bool HasChoices;
    }

    /// <summary>
    /// 타이핑 즉시 완료 요청. Manager → UI.
    /// </summary>
    public struct DialogueSkipRequested { }

    /// <summary>
    /// 타이핑 완료 통보. UI → Manager.
    /// </summary>
    public struct DialogueTypingCompleted { }

    /// <summary>
    /// 대화 종료 이벤트. Manager → UI.
    /// </summary>
    public struct DialogueEnded
    {
        public string DialogueId;
    }
}
