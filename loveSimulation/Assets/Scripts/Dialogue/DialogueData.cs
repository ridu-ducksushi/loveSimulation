using System.Collections.Generic;
using Newtonsoft.Json;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 캐릭터 위치.
    /// </summary>
    public enum CharacterPosition
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// 복수 캐릭터 배치 정보.
    /// </summary>
    [System.Serializable]
    public class CharacterDisplayInfo
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("position")]
        public CharacterPosition Position;

        [JsonProperty("emotion")]
        public string Emotion;
    }

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

        /// <summary>
        /// 같은 파일 내 섹션으로 점프. nextDialogueId보다 우선 처리.
        /// </summary>
        [JsonProperty("goto")]
        public string Goto;

        [JsonProperty("affectionChange")]
        public int AffectionChange;

        [JsonProperty("flagToSet")]
        public string FlagToSet;

        /// <summary>
        /// 같은 파일 내 섹션 점프 여부.
        /// </summary>
        public bool IsInternalJump => !string.IsNullOrEmpty(Goto);
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
        /// 캐릭터 표정 (neutral, smile, sad, angry 등). optional.
        /// </summary>
        [JsonProperty("emotion")]
        public string Emotion;

        /// <summary>
        /// 배경 ID. optional.
        /// </summary>
        [JsonProperty("background")]
        public string Background;

        /// <summary>
        /// 복수 캐릭터 배치 정보. optional.
        /// </summary>
        [JsonProperty("characters")]
        public List<CharacterDisplayInfo> Characters;

        /// <summary>
        /// 텍스트 정렬. "left", "center", "right". optional (기본값: left).
        /// </summary>
        [JsonProperty("textAlign")]
        public string TextAlign;

        /// <summary>
        /// 이 라인이 끝나면 캐릭터 숨김. optional (기본값: false).
        /// </summary>
        [JsonProperty("hideCharactersAfter")]
        public bool HideCharactersAfter;

        /// <summary>
        /// 선택지가 존재하는지 여부.
        /// </summary>
        public bool HasChoices => Choices != null && Choices.Count > 0;

        /// <summary>
        /// 복수 캐릭터 배치 정보가 있는지 여부.
        /// </summary>
        public bool HasCharacters => Characters != null && Characters.Count > 0;
    }

    /// <summary>
    /// 대화 섹션 데이터. 한 파일 내 여러 분기를 관리.
    /// </summary>
    [System.Serializable]
    public class DialogueSection
    {
        [JsonProperty("lines")]
        public List<DialogueLine> Lines;
    }

    /// <summary>
    /// JSON 대화 파일 전체 구조.
    /// </summary>
    [System.Serializable]
    public class DialogueData
    {
        [JsonProperty("dialogueId")]
        public string DialogueId;

        /// <summary>
        /// 챕터 제목. 챕터 시작 시 연출과 함께 표시.
        /// </summary>
        [JsonProperty("chapterTitle")]
        public string ChapterTitle;

        /// <summary>
        /// 기존 방식: 단일 라인 리스트.
        /// </summary>
        [JsonProperty("lines")]
        public List<DialogueLine> Lines;

        /// <summary>
        /// 섹션 방식: 여러 분기를 딕셔너리로 관리.
        /// </summary>
        [JsonProperty("sections")]
        public Dictionary<string, DialogueSection> Sections;

        /// <summary>
        /// 챕터 제목 존재 여부.
        /// </summary>
        public bool HasChapterTitle => !string.IsNullOrEmpty(ChapterTitle);

        /// <summary>
        /// 섹션 방식 사용 여부.
        /// </summary>
        public bool HasSections => Sections != null && Sections.Count > 0;

        /// <summary>
        /// 지정된 섹션의 라인 리스트 반환. 섹션이 없으면 null.
        /// </summary>
        public List<DialogueLine> GetSectionLines(string sectionName)
        {
            if (Sections == null)
            {
                return null;
            }

            if (Sections.TryGetValue(sectionName, out DialogueSection section))
            {
                return section.Lines;
            }

            return null;
        }
    }
}
