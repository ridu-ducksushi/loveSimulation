using LoveSimulation.Dialogue;

namespace LoveSimulation.Events
{
    /// <summary>
    /// 캐릭터 스프라이트 표시 요청. DialogueManager -> CharacterSpriteUI.
    /// </summary>
    public struct CharacterDisplayRequested
    {
        public string CharacterId;
        public string Emotion;
        public CharacterPosition Position;
        public bool FadeIn;
    }

    /// <summary>
    /// 캐릭터 스프라이트 숨김 요청. DialogueManager -> CharacterSpriteUI.
    /// </summary>
    public struct CharacterHideRequested
    {
        /// <summary>
        /// null이면 모든 캐릭터 숨김.
        /// </summary>
        public CharacterPosition? Position;
    }

    /// <summary>
    /// 배경 변경 요청. DialogueManager -> BackgroundManager.
    /// </summary>
    public struct BackgroundChangeRequested
    {
        public string BackgroundId;
        public float Duration;
    }
}
