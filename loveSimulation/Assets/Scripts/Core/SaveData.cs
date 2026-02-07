using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 세이브 슬롯 메타 정보. 슬롯 목록 표시에 사용.
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        [JsonProperty("slotIndex")]
        public int SlotIndex;

        [JsonProperty("saveDate")]
        public string SaveDate;

        [JsonProperty("sceneName")]
        public string SceneName;

        [JsonProperty("playTime")]
        public float PlayTime;

        [JsonProperty("isEmpty")]
        public bool IsEmpty = true;
    }

    /// <summary>
    /// 게임 전체 세이브 데이터 구조.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        [JsonProperty("version")]
        public int Version = 1;

        [JsonProperty("sceneName")]
        public string SceneName = string.Empty;

        [JsonProperty("gameState")]
        public GameState GameState = GameState.Playing;

        [JsonProperty("playTime")]
        public float PlayTime;

        [JsonProperty("saveDate")]
        public string SaveDate = string.Empty;

        /// <summary>
        /// 캐릭터별 호감도 데이터. key: 캐릭터 ID
        /// </summary>
        [JsonProperty("affectionData")]
        public Dictionary<string, int> AffectionData = new Dictionary<string, int>();

        /// <summary>
        /// 이벤트 플래그. key: 플래그 이름, value: 활성 여부
        /// </summary>
        [JsonProperty("flags")]
        public Dictionary<string, bool> Flags = new Dictionary<string, bool>();

        /// <summary>
        /// 범용 문자열 데이터 저장. 추후 시스템 확장용.
        /// </summary>
        [JsonProperty("extraData")]
        public Dictionary<string, string> ExtraData = new Dictionary<string, string>();

        /// <summary>
        /// 현재 일차.
        /// </summary>
        [JsonProperty("currentDay")]
        public int CurrentDay = 1;

        /// <summary>
        /// 현재 시간대 (TimeOfDay enum 값).
        /// </summary>
        [JsonProperty("currentTimeOfDay")]
        public int CurrentTimeOfDay = 0;

        /// <summary>
        /// 현재 장소 (Location enum 값).
        /// </summary>
        [JsonProperty("currentLocation")]
        public int CurrentLocation = 0;

        /// <summary>
        /// 트리거된 이벤트 ID 목록.
        /// </summary>
        [JsonProperty("triggeredEvents")]
        public List<string> TriggeredEvents = new List<string>();
    }
}
