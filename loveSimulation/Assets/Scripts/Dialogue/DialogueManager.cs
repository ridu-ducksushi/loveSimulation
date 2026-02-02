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

            // 현재 GameState 저장 후 Dialogue로 전환
            _previousState = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameState.Dialogue);

            EventBus.Publish(new DialogueStarted { DialogueId = dialogueId });
            Debug.Log($"[DialogueManager] 대화 시작: {dialogueId}");

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
        /// 현재 라인을 UI에 표시 요청.
        /// </summary>
        private void ShowCurrentLine()
        {
            if (_currentDialogue == null || _currentDialogue.Lines == null)
            {
                EndDialogue();
                return;
            }

            if (_currentLineIndex >= _currentDialogue.Lines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = _currentDialogue.Lines[_currentLineIndex];
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
            DialogueLine currentLine = _currentDialogue.Lines[_currentLineIndex];

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
        /// </summary>
        private void OnTypingCompleted(DialogueTypingCompleted _)
        {
            _isTyping = false;

            if (_currentDialogue == null || _currentDialogue.Lines == null)
            {
                return;
            }

            if (_currentLineIndex >= _currentDialogue.Lines.Count)
            {
                return;
            }

            DialogueLine currentLine = _currentDialogue.Lines[_currentLineIndex];
            if (currentLine.HasChoices)
            {
                ShowChoices(currentLine.Choices);
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
        /// 선택지 선택 이벤트 핸들러. 효과 적용 후 다음 대화로 연결.
        /// </summary>
        private void OnChoiceSelected(ChoiceSelected evt)
        {
            if (!_isShowingChoices || _currentDialogue == null)
            {
                return;
            }

            DialogueLine currentLine = _currentDialogue.Lines[_currentLineIndex];
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
            ChainToNextDialogue(choice.NextDialogueId);
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
            for (int i = _currentLineIndex; i >= 0; i--)
            {
                string speaker = _currentDialogue.Lines[i].Speaker;
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
                if (data == null || data.Lines == null || data.Lines.Count == 0)
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
