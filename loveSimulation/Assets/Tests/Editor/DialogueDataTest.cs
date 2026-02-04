using NUnit.Framework;
using Newtonsoft.Json;
using LoveSimulation.Dialogue;

namespace LoveSimulation.Tests
{
    public class DialogueDataTest
    {
        private const string SampleJson = @"{
            ""dialogueId"": ""sample_dialogue"",
            ""lines"": [
                {
                    ""speaker"": """",
                    ""text"": ""나레이션 텍스트.""
                },
                {
                    ""speaker"": ""유나"",
                    ""text"": ""유나의 대사."",
                    ""choices"": [
                        { ""text"": ""선택지 1"", ""nextDialogueId"": ""next_01"", ""affectionChange"": 5 },
                        { ""text"": ""선택지 2"", ""nextDialogueId"": ""next_02"", ""affectionChange"": -2 }
                    ]
                }
            ]
        }";

        private const string SectionJson = @"{
            ""dialogueId"": ""chapter01"",
            ""sections"": {
                ""start"": {
                    ""lines"": [
                        { ""speaker"": """", ""text"": ""시작 나레이션."" },
                        {
                            ""speaker"": ""유나"",
                            ""text"": ""선택해줘."",
                            ""choices"": [
                                { ""text"": ""옵션 A"", ""goto"": ""branch_a"", ""affectionChange"": 5 },
                                { ""text"": ""옵션 B"", ""goto"": ""branch_b"", ""affectionChange"": -2 }
                            ]
                        }
                    ]
                },
                ""branch_a"": {
                    ""lines"": [
                        { ""speaker"": ""유나"", ""text"": ""A를 골랐구나!"" }
                    ]
                },
                ""branch_b"": {
                    ""lines"": [
                        { ""speaker"": ""유나"", ""text"": ""B를 골랐구나!"" }
                    ]
                }
            }
        }";

        [Test]
        public void DialogueData_JSON_역직렬화_성공()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);

            Assert.IsNotNull(data);
            Assert.AreEqual("sample_dialogue", data.DialogueId);
            Assert.AreEqual(2, data.Lines.Count);
        }

        [Test]
        public void DialogueLine_나레이션_화자_빈문자열()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);
            var narrationLine = data.Lines[0];

            Assert.AreEqual(string.Empty, narrationLine.Speaker);
            Assert.AreEqual("나레이션 텍스트.", narrationLine.Text);
            Assert.IsFalse(narrationLine.HasChoices);
        }

        [Test]
        public void DialogueLine_화자_대사_선택지_파싱()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);
            var speakerLine = data.Lines[1];

            Assert.AreEqual("유나", speakerLine.Speaker);
            Assert.IsTrue(speakerLine.HasChoices);
            Assert.AreEqual(2, speakerLine.Choices.Count);
        }

        [Test]
        public void DialogueChoice_필드값_정확히_파싱()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);
            var choice = data.Lines[1].Choices[0];

            Assert.AreEqual("선택지 1", choice.Text);
            Assert.AreEqual("next_01", choice.NextDialogueId);
            Assert.AreEqual(5, choice.AffectionChange);
        }

        [Test]
        public void DialogueLine_선택지_없으면_HasChoices_False()
        {
            const string json = @"{
                ""dialogueId"": ""test"",
                ""lines"": [
                    { ""speaker"": ""A"", ""text"": ""안녕"" }
                ]
            }";

            var data = JsonConvert.DeserializeObject<DialogueData>(json);
            Assert.IsFalse(data.Lines[0].HasChoices);
            Assert.IsNull(data.Lines[0].Choices);
        }

        [Test]
        public void SampleDialogue_JSON파일_Resources_로드()
        {
            var textAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Dialogues/sample_dialogue");
            Assert.IsNotNull(textAsset, "sample_dialogue.json이 Resources/Dialogues/에 존재해야 합니다.");

            var data = JsonConvert.DeserializeObject<DialogueData>(textAsset.text);
            Assert.IsNotNull(data);
            Assert.AreEqual("sample_dialogue", data.DialogueId);
            Assert.IsTrue(data.Lines.Count > 0);
        }

        [Test]
        public void DialogueData_섹션방식_역직렬화_성공()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SectionJson);

            Assert.IsNotNull(data);
            Assert.AreEqual("chapter01", data.DialogueId);
            Assert.IsTrue(data.HasSections);
            Assert.AreEqual(3, data.Sections.Count);
        }

        [Test]
        public void DialogueData_섹션_라인_정상_조회()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SectionJson);

            var startLines = data.GetSectionLines("start");
            Assert.IsNotNull(startLines);
            Assert.AreEqual(2, startLines.Count);

            var branchALines = data.GetSectionLines("branch_a");
            Assert.IsNotNull(branchALines);
            Assert.AreEqual(1, branchALines.Count);
            Assert.AreEqual("A를 골랐구나!", branchALines[0].Text);
        }

        [Test]
        public void DialogueData_존재하지않는_섹션_Null반환()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SectionJson);

            var invalidSection = data.GetSectionLines("invalid_section");
            Assert.IsNull(invalidSection);
        }

        [Test]
        public void DialogueChoice_Goto필드_정상파싱()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SectionJson);

            var startLines = data.GetSectionLines("start");
            var choiceLine = startLines[1];

            Assert.IsTrue(choiceLine.HasChoices);
            Assert.AreEqual(2, choiceLine.Choices.Count);

            var choiceA = choiceLine.Choices[0];
            Assert.AreEqual("branch_a", choiceA.Goto);
            Assert.IsTrue(choiceA.IsInternalJump);
            Assert.IsNull(choiceA.NextDialogueId);
        }

        [Test]
        public void DialogueChoice_NextDialogueId만_있으면_IsInternalJump_False()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);
            var choice = data.Lines[1].Choices[0];

            Assert.AreEqual("next_01", choice.NextDialogueId);
            Assert.IsFalse(choice.IsInternalJump);
            Assert.IsNull(choice.Goto);
        }

        [Test]
        public void DialogueData_기존방식_HasSections_False()
        {
            var data = JsonConvert.DeserializeObject<DialogueData>(SampleJson);

            Assert.IsFalse(data.HasSections);
            Assert.IsNull(data.Sections);
            Assert.IsNotNull(data.Lines);
        }

        [Test]
        public void Chapter01_JSON파일_Resources_로드()
        {
            var textAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Dialogues/chapter01");
            Assert.IsNotNull(textAsset, "chapter01.json이 Resources/Dialogues/에 존재해야 합니다.");

            var data = JsonConvert.DeserializeObject<DialogueData>(textAsset.text);
            Assert.IsNotNull(data);
            Assert.AreEqual("chapter01", data.DialogueId);
            Assert.IsTrue(data.HasSections);
            Assert.IsTrue(data.Sections.ContainsKey("start"));
            Assert.IsTrue(data.Sections.ContainsKey("walk_branch"));
            Assert.IsTrue(data.Sections.ContainsKey("refuse_branch"));
        }
    }
}
