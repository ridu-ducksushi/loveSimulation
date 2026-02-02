namespace LoveSimulation.Events
{
    /// <summary>
    /// 호감도 변경 이벤트. GameData에서 발행.
    /// </summary>
    public struct AffectionChanged
    {
        public string CharacterId;
        public int PreviousValue;
        public int NewValue;
        public int Delta;
    }

    /// <summary>
    /// 호감도 레벨 변경 이벤트. 레벨 임계값을 넘었을 때 발행.
    /// </summary>
    public struct AffectionLevelChanged
    {
        public string CharacterId;
        public string PreviousLevel;
        public string NewLevel;
    }
}
