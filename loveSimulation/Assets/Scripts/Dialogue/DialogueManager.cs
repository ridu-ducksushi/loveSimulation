using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 대화 흐름 제어 싱글톤. JSON 로드, 진행, 종료, 선택지 관리.
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private string _currentSection;
        private bool _isTyping;
        private bool _isDialogueActive;
        private bool _isShowingChoices;
        private GameState _previousState;

        public bool IsDialogueActive => _isDialogueActive;

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueTypingCompleted>(OnTypingCompleted);
            EventBus.Subscribe<ChoiceSelected>(OnChoiceSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueTypingCompleted>(OnTypingCompleted);
            EventBus.Unsubscribe<ChoiceSelected>(OnChoiceSelected);
        }

        /// <summary>
        /// 대화 시작. JSON 로드 후 첫 라인 표시.
        /// </summary>
        public void StartDialogue(string dialogueId)
        {
            StartDialogue(dialogueId, null);
        }

        /// <summary>
        /// 대화 시작 (섹션 지정 가능). JSON 로드 후 첫 라인 표시.
        /// </summary>
        public void StartDialogue(string dialogueId, string sectionName)
        {
            if (_isDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] 이미 대화 진행 중.");
                return;
            }

            DialogueData data = LoadDialogue(dialogueId);
            if (data == null)
            {
                return;
            }

            _currentDialogue = data;
            _currentLineIndex = 0;
            _isDialogueActive = true;
            _isTyping = false;
            _isShowingChoices = false;

            // 섹션 방식이면 시작 섹션 설정
            if (data.HasSections)
            {
                _currentSection = string.IsNullOrEmpty(sectionName) ? "start" : sectionName;
                if (data.GetSectionLines(_currentSection) == null)
                {
                    Debug.LogError($"[DialogueManager] 섹션을 찾을 수 없음: {_currentSection}");
                    _isDialogueActive = false;
                    return;
                }
            }
            else
            {
                _currentSection = null;
            }

            // 현재 GameState 저장 후 Dialogue로 전환
            _previousState = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameState.Dialogue);

            EventBus.Publish(new DialogueStarted { DialogueId = dialogueId });
            Debug.Log($"[DialogueManager] 대화 시작: {dialogueId}" +
                      (_currentSection != null ? $" (섹션: {_currentSection})" : ""));

            ShowCurrentLine();
        }

        /// <summary>
        /// 사용자 입력에 의한 대화 진행. 타이핑 중이면 스킵, 선택지 표시 중이면 무시.
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!_isDialogueActive)
            {
                return;
            }

            // 선택지 표시 중에는 입력 차단
            if (_isShowingChoices)
            {
                return;
            }

            if (_isTyping)
            {
                EventBus.Publish(new DialogueSkipRequested());
                return;
            }

            RequestNextLine();
        }

        /// <summary>
        /// 현재 섹션 또는 기본 라인 리스트 반환.
        /// </summary>
        private List<DialogueLine> GetCurrentLines()
        {
            if (_currentDialogue == null)
            {
                return null;
            }

            if (_currentDialogue.HasSections && !string.IsNullOrEmpty(_currentSection))
            {
                return _currentDialogue.GetSectionLines(_currentSection);
            }

            return _currentDialogue.Lines;
        }

        /// <summary>
        /// 현재 라인을 UI에 표시 요청.
        /// </summary>
        private void ShowCurrentLine()
        {
            List<DialogueLine> lines = GetCurrentLines();
            if (lines == null)
            {
                EndDialogue();
                return;
            }

            if (_currentLineIndex >= lines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = lines[_currentLineIndex];
            _isTyping = true;

            EventBus.Publish(new DialogueLineRequested
            {
                Speaker = line.Speaker,
                Text = line.Text,
                HasChoices = line.HasChoices
            });
        }

        /// <summary>
        /// 다음 라인으로 진행 또는 대화 종료.
        /// </summary>
        private void RequestNextLine()
        {
            List<DialogueLine> lines = GetCurrentLines();
            if (lines == null || _currentLineIndex >= lines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine currentLine = lines[_currentLineIndex];

            // 선택지가 있는 라인이면 선택지 대기 (선택지 UI에서 처리)
            if (currentLine.HasChoices)
            {
                return;
            }

            _currentLineIndex++;
            ShowCurrentLine();
        }

        /// <summary>
        /// 타이핑 완료 이벤트 핸들러. 선택지 라인이면 자동으로 선택지 표시.
        /// 단, 선택지가 1개면 버튼 없이 바로 진행.
        /// </summary>
        private void OnTypingCompleted(DialogueTypingCompleted _)
        {
            _isTyping = false;

            List<DialogueLine> lines = GetCurrentLines();
            if (lines == null || _currentLineIndex >= lines.Count)
            {
                return;
            }

            DialogueLine currentLine = lines[_currentLineIndex];
            if (currentLine.HasChoices)
            {
                // 선택지가 1개면 자동 진행 (계속하기 버튼 제거)
                if (currentLine.Choices.Count == 1)
                {
                    ProcessSingleChoice(currentLine.Choices[0]);
                }
                else
                {
                    ShowChoices(currentLine.Choices);
                }
            }
        }

        /// <summary>
        /// 선택지 표시 요청. 입력 차단 활성화.
        /// </summary>
        private void ShowChoices(List<DialogueChoice> choices)
        {
            _isShowingChoices = true;
            EventBus.Publish(new ChoiceRequested { Choices = choices });
            Debug.Log($"[DialogueManager] 선택지 {choices.Count}개 표시.");
        }

        /// <summary>
        /// 선택지가 1개일 때 자동으로 해당 선택지 처리.
        /// </summary>
        private void ProcessSingleChoice(DialogueChoice choice)
        {
            ApplyChoiceEffects(choice);

            if (choice.IsInternalJump)
            {
                JumpToSection(choice.Goto);
            }
            else
            {
                ChainToNextDialogue(choice.NextDialogueId);
            }
        }

        /// <summary>
        /// 선택지 선택 이벤트 핸들러. 효과 적용 후 다음 대화로 연결.
        /// </summary>
        private void OnChoiceSelected(ChoiceSelected evt)
        {
            if (!_isShowingChoices || _currentDialogue == null)
            {
                return;
            }

            List<DialogueLine> lines = GetCurrentLines();
            if (lines == null || _currentLineIndex >= lines.Count)
            {
                return;
            }

            DialogueLine currentLine = lines[_currentLineIndex];
            if (!currentLine.HasChoices)
            {
                return;
            }

            // 인덱스 검증
            if (evt.ChoiceIndex < 0 || evt.ChoiceIndex >= currentLine.Choices.Count)
            {
                Debug.LogError($"[DialogueManager] 잘못된 선택지 인덱스: {evt.ChoiceIndex}");
                return;
            }

            DialogueChoice choice = currentLine.Choices[evt.ChoiceIndex];
            ApplyChoiceEffects(choice);

            // goto가 있으면 내부 섹션 점프, 없으면 외부 대화 연결
            if (choice.IsInternalJump)
            {
                JumpToSection(choice.Goto);
            }
            else
            {
                ChainToNextDialogue(choice.NextDialogueId);
            }
        }

        /// <summary>
        /// 같은 파일 내 다른 섹션으로 점프.
        /// </summary>
        private void JumpToSection(string sectionName)
        {
            if (!_currentDialogue.HasSections)
            {
                Debug.LogError("[DialogueManager] 섹션 방식이 아닌 대화에서 goto 사용 불가.");
                EndDialogue();
                return;
            }

            List<DialogueLine> sectionLines = _currentDialogue.GetSectionLines(sectionName);
            if (sectionLines == null)
            {
                Debug.LogError($"[DialogueManager] 섹션을 찾을 수 없음: {sectionName}");
                EndDialogue();
                return;
            }

            _currentSection = sectionName;
            _currentLineIndex = 0;
            _isShowingChoices = false;

            Debug.Log($"[DialogueManager] 섹션 점프: {sectionName}");
            ShowCurrentLine();
        }

        /// <summary>
        /// 선택지 효과 적용. 호감도 변경 및 플래그 설정.
        /// </summary>
        private void ApplyChoiceEffects(DialogueChoice choice)
        {
            if (choice.AffectionChange != 0)
            {
                string speaker = GetCurrentSpeaker();
                if (!string.IsNullOrEmpty(speaker))
                {
                    GameData.AddAffection(speaker, choice.AffectionChange);
                }
            }

            if (!string.IsNullOrEmpty(choice.FlagToSet))
            {
                GameData.SetFlag(choice.FlagToSet);
            }
        }

        /// <summary>
        /// 현재 라인부터 역순으로 화자를 탐색하여 호감도 대상 결정.
        /// </summary>
        private string GetCurrentSpeaker()
        {
            List<DialogueLine> lines = GetCurrentLines();
            if (lines == null)
            {
                return string.Empty;
            }

            for (int i = _currentLineIndex; i >= 0; i--)
            {
                string speaker = lines[i].Speaker;
                if (!string.IsNullOrEmpty(speaker))
                {
                    return speaker;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 현재 대화 종료 후 다음 대화 시작.
        /// </summary>
        private void ChainToNextDialogue(string nextDialogueId)
        {
            EndDialogue();

            if (!string.IsNullOrEmpty(nextDialogueId))
            {
                StartDialogue(nextDialogueId);
            }
        }

        /// <summary>
        /// 대화 종료. 이전 GameState 복원.
        /// </summary>
        private void EndDialogue()
        {
            if (!_isDialogueActive)
            {
                return;
            }

            string dialogueId = _currentDialogue?.DialogueId ?? string.Empty;

            _isDialogueActive = false;
            _isTyping = false;
            _isShowingChoices = false;
            _currentDialogue = null;
            _currentLineIndex = 0;
            _currentSection = null;

            GameManager.Instance.ChangeState(_previousState);

            EventBus.Publish(new DialogueEnded { DialogueId = dialogueId });
            Debug.Log($"[DialogueManager] 대화 종료: {dialogueId}");
        }

        /// <summary>
        /// Resources 폴더에서 대화 JSON 로드.
        /// </summary>
        private DialogueData LoadDialogue(string dialogueId)
        {
            TextAsset textAsset = Resources.Load<TextAsset>($"Dialogues/{dialogueId}");
            if (textAsset == null)
            {
                Debug.LogError($"[DialogueManager] 대화 데이터 로드 실패: Dialogues/{dialogueId}");
                return null;
            }

            try
            {
                DialogueData data = JsonConvert.DeserializeObject<DialogueData>(textAsset.text);
                if (data == null)
                {
                    Debug.LogError($"[DialogueManager] 대화 데이터가 비어있음: {dialogueId}");
                    return null;
                }

                // 섹션 방식 또는 기존 방식 중 하나는 유효해야 함
                bool hasLines = data.Lines != null && data.Lines.Count > 0;
                bool hasSections = data.HasSections;

                if (!hasLines && !hasSections)
                {
                    Debug.LogError($"[DialogueManager] 대화 데이터가 비어있음: {dialogueId}");
                    return null;
                }

                return data;
            }
            catch (JsonException e)
            {
                Debug.LogError($"[DialogueManager] JSON 파싱 실패: {dialogueId}\n{e.Message}");
                return null;
            }
        }
    }
}
