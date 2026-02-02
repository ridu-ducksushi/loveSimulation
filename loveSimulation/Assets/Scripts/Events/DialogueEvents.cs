using System.Collections.Generic;
using LoveSimulation.Core;
using LoveSimulation.Dialogue;

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
    /// 선택지 표시 요청. Manager → ChoiceUI.
    /// </summary>
    public struct ChoiceRequested
    {
        public List<DialogueChoice> Choices;
    }

    /// <summary>
    /// 선택지 선택 완료. ChoiceUI → Manager.
    /// </summary>
    public struct ChoiceSelected
    {
        public int ChoiceIndex;
    }

    /// <summary>
    /// 대화 종료 이벤트. Manager → UI.
    /// </summary>
    public struct DialogueEnded
    {
        public string DialogueId;
    }
}
