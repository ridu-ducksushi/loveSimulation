using UnityEngine;
using Newtonsoft.Json;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 대화 흐름 제어 싱글톤. JSON 로드, 진행, 종료 관리.
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private bool _isTyping;
        private bool _isDialogueActive;
        private GameState _previousState;

        public bool IsDialogueActive => _isDialogueActive;

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueTypingCompleted>(OnTypingCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueTypingCompleted>(OnTypingCompleted);
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

            // 현재 GameState 저장 후 Dialogue로 전환
            _previousState = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameState.Dialogue);

            EventBus.Publish(new DialogueStarted { DialogueId = dialogueId });
            Debug.Log($"[DialogueManager] 대화 시작: {dialogueId}");

            ShowCurrentLine();
        }

        /// <summary>
        /// 사용자 입력에 의한 대화 진행. 타이핑 중이면 스킵, 완료면 다음 라인.
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!_isDialogueActive)
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

            // 선택지가 있는 라인이면 대화 종료 (Phase 3에서 선택지 처리)
            if (currentLine.HasChoices)
            {
                EndDialogue();
                return;
            }

            _currentLineIndex++;
            ShowCurrentLine();
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
            _currentDialogue = null;
            _currentLineIndex = 0;

            GameManager.Instance.ChangeState(_previousState);

            EventBus.Publish(new DialogueEnded { DialogueId = dialogueId });
            Debug.Log($"[DialogueManager] 대화 종료: {dialogueId}");
        }

        /// <summary>
        /// 타이핑 완료 이벤트 핸들러.
        /// </summary>
        private void OnTypingCompleted(DialogueTypingCompleted _)
        {
            _isTyping = false;
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
