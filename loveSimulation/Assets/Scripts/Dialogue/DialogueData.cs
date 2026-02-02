using System.Collections.Generic;
using Newtonsoft.Json;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 대화 선택지 데이터.
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("nextDialogueId")]
        public string NextDialogueId;

        // Phase 3에서 활용
        [JsonProperty("affectionChange")]
        public int AffectionChange;

        [JsonProperty("flagToSet")]
        public string FlagToSet;
    }

    /// <summary>
    /// 대화 한 줄 데이터.
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        [JsonProperty("speaker")]
        public string Speaker;

        [JsonProperty("text")]
        public string Text;

        [JsonProperty("choices")]
        public List<DialogueChoice> Choices;

        /// <summary>
        /// 선택지가 존재하는지 여부.
        /// </summary>
        public bool HasChoices => Choices != null && Choices.Count > 0;
    }

    /// <summary>
    /// JSON 대화 파일 전체 구조.
    /// </summary>
    [System.Serializable]
    public class DialogueData
    {
        [JsonProperty("dialogueId")]
        public string DialogueId;

        [JsonProperty("lines")]
        public List<DialogueLine> Lines;
    }
}
