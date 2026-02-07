using System;
using UnityEngine;
using LoveSimulation.Core;

namespace LoveSimulation.Data
{
    /// <summary>
    /// 이벤트 발동 조건 정의.
    /// </summary>
    [Serializable]
    public class EventCondition
    {
        [Header("일차 조건")]
        [Tooltip("최소 일차 (null이면 무시)")]
        public int MinDay = -1;

        [Tooltip("최대 일차 (null이면 무시)")]
        public int MaxDay = -1;

        [Header("시간/장소 조건")]
        [Tooltip("필요 시간대 (-1이면 무시)")]
        public int RequiredTimeOfDay = -1;

        [Tooltip("필요 장소 (-1이면 무시)")]
        public int RequiredLocation = -1;

        [Header("호감도 조건")]
        [Tooltip("호감도 체크 대상 캐릭터 ID")]
        public string CharacterId = string.Empty;

        [Tooltip("최소 호감도 (-1이면 무시)")]
        public int MinAffection = -1;

        [Tooltip("최대 호감도 (-1이면 무시)")]
        public int MaxAffection = -1;

        [Header("플래그 조건")]
        [Tooltip("필요 플래그 (모두 충족 필요)")]
        public string[] RequiredFlags = Array.Empty<string>();

        [Tooltip("금지 플래그 (하나라도 있으면 불충족)")]
        public string[] ForbiddenFlags = Array.Empty<string>();

        /// <summary>
        /// 조건 충족 여부 평가.
        /// </summary>
        public bool Evaluate()
        {
            // 일차 조건
            if (MinDay > 0 && WorldState.CurrentDay < MinDay)
            {
                return false;
            }

            if (MaxDay > 0 && WorldState.CurrentDay > MaxDay)
            {
                return false;
            }

            // 시간대 조건
            if (RequiredTimeOfDay >= 0 && (int)WorldState.CurrentTimeOfDay != RequiredTimeOfDay)
            {
                return false;
            }

            // 장소 조건
            if (RequiredLocation >= 0 && (int)WorldState.CurrentLocation != RequiredLocation)
            {
                return false;
            }

            // 호감도 조건
            if (!string.IsNullOrEmpty(CharacterId))
            {
                int affection = GameData.GetAffection(CharacterId);

                if (MinAffection >= 0 && affection < MinAffection)
                {
                    return false;
                }

                if (MaxAffection >= 0 && affection > MaxAffection)
                {
                    return false;
                }
            }

            // 필수 플래그 조건
            if (RequiredFlags != null && RequiredFlags.Length > 0)
            {
                foreach (string flag in RequiredFlags)
                {
                    if (!string.IsNullOrEmpty(flag) && !GameData.GetFlag(flag))
                    {
                        return false;
                    }
                }
            }

            // 금지 플래그 조건
            if (ForbiddenFlags != null && ForbiddenFlags.Length > 0)
            {
                foreach (string flag in ForbiddenFlags)
                {
                    if (!string.IsNullOrEmpty(flag) && GameData.GetFlag(flag))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 조건이 비어있는지 (항상 충족) 확인.
        /// </summary>
        public bool IsEmpty()
        {
            return MinDay <= 0
                && MaxDay <= 0
                && RequiredTimeOfDay < 0
                && RequiredLocation < 0
                && string.IsNullOrEmpty(CharacterId)
                && (RequiredFlags == null || RequiredFlags.Length == 0)
                && (ForbiddenFlags == null || ForbiddenFlags.Length == 0);
        }
    }
}
