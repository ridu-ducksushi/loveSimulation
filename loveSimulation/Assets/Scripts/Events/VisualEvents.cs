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

    /// <summary>
    /// 화자 강조 요청. 해당 위치의 캐릭터를 밝게, 나머지는 어둡게.
    /// </summary>
    public struct CharacterHighlightRequested
    {
        /// <summary>
        /// 강조할 캐릭터 위치. null이면 모두 동일 밝기.
        /// </summary>
        public CharacterPosition? Position;
    }
}
