using System;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 호감도 레벨 정의. 임계값 이상일 때 해당 레벨로 판정.
    /// </summary>
    [Serializable]
    public class AffectionLevel
    {
        public string LevelName;
        public int Threshold;
    }
}
