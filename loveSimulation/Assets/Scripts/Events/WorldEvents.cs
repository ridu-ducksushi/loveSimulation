using LoveSimulation.Core;

namespace LoveSimulation.Events
{
    /// <summary>
    /// 시간대 변경 이벤트.
    /// </summary>
    public struct TimeOfDayChanged
    {
        public TimeOfDay PreviousTime;
        public TimeOfDay NewTime;
        public int CurrentDay;
    }

    /// <summary>
    /// 장소 변경 이벤트.
    /// </summary>
    public struct LocationChanged
    {
        public Location PreviousLocation;
        public Location NewLocation;
    }

    /// <summary>
    /// 일차 변경 이벤트.
    /// </summary>
    public struct DayChanged
    {
        public int PreviousDay;
        public int NewDay;
    }
}
