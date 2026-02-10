namespace LoveSimulation.Events
{
    /// <summary>
    /// 단서 재화 변경 이벤트.
    /// </summary>
    public struct ClueCurrencyChanged
    {
        public int PreviousValue;
        public int NewValue;
        public int Delta;
    }

    /// <summary>
    /// 조사 시작 이벤트.
    /// </summary>
    public struct InvestigationStarted { }

    /// <summary>
    /// 조사 완료 이벤트.
    /// </summary>
    public struct InvestigationCompleted
    {
        public int CluesRewarded;
        public bool WasAdSkip;
    }
}
