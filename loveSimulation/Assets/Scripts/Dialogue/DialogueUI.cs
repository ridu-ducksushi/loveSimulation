using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 대화 UI 표시 및 타이핑 효과 처리.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private GameObject _speakerPanel;
        [SerializeField] private TextMeshProUGUI _speakerText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        [Header("설정")]
        [SerializeField] private float _typingSpeed = 0.03f;

        private Coroutine _typingCoroutine;
        private WaitForSeconds _typingWait;
        private bool _isTyping;

        private void Awake()
        {
            _typingWait = new WaitForSeconds(_typingSpeed);

            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueStarted>(OnDialogueStarted);
            EventBus.Subscribe<DialogueLineRequested>(OnDialogueLineRequested);
            EventBus.Subscribe<DialogueSkipRequested>(OnDialogueSkipRequested);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
            EventBus.Subscribe<ChapterTitleRequested>(OnChapterTitleRequested);
            EventBus.Subscribe<ChapterTitleCompleted>(OnChapterTitleCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStarted>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueLineRequested>(OnDialogueLineRequested);
            EventBus.Unsubscribe<DialogueSkipRequested>(OnDialogueSkipRequested);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
            EventBus.Unsubscribe<ChapterTitleRequested>(OnChapterTitleRequested);
            EventBus.Unsubscribe<ChapterTitleCompleted>(OnChapterTitleCompleted);
        }

        private void Update()
        {
            // 대화 진행 중일 때만 입력 처리
            if (_dialoguePanel == null || !_dialoguePanel.activeSelf)
            {
                return;
            }

            bool advancePressed = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                advancePressed = true;
            }

            if (!advancePressed && Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    advancePressed = true;
                }
            }

            if (advancePressed)
            {
                DialogueManager.Instance?.AdvanceDialogue();
            }
        }

        /// <summary>
        /// 대화 시작 → 패널 활성화.
        /// </summary>
        private void OnDialogueStarted(DialogueStarted evt)
        {
            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(true);
            }
        }

        /// <summary>
        /// 챕터 제목 연출 시작 시 패널 숨김.
        /// </summary>
        private void OnChapterTitleRequested(ChapterTitleRequested _)
        {
            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 챕터 제목 연출 완료 시 패널 재활성화.
        /// </summary>
        private void OnChapterTitleCompleted(ChapterTitleCompleted _)
        {
            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(true);
            }
        }

        /// <summary>
        /// 새 라인 표시 → 화자 설정 + 타이핑 시작.
        /// </summary>
        private void OnDialogueLineRequested(DialogueLineRequested evt)
        {
            // 화자 표시 처리
            bool isNarration = string.IsNullOrEmpty(evt.Speaker);

            if (_speakerPanel != null)
            {
                _speakerPanel.SetActive(!isNarration);
            }

            if (_speakerText != null && !isNarration)
            {
                _speakerText.text = evt.Speaker;
            }

            // 텍스트 정렬 적용
            ApplyTextAlignment(evt.TextAlign);

            // 타이핑 시작
            StartTyping(evt.Text);
        }

        /// <summary>
        /// 텍스트 정렬 적용.
        /// </summary>
        private void ApplyTextAlignment(string textAlign)
        {
            if (_dialogueText == null)
            {
                return;
            }

            _dialogueText.alignment = textAlign switch
            {
                "center" => TextAlignmentOptions.Top,
                "right" => TextAlignmentOptions.TopRight,
                _ => TextAlignmentOptions.TopLeft
            };
        }

        /// <summary>
        /// 타이핑 스킵 요청 → 즉시 전체 표시.
        /// </summary>
        private void OnDialogueSkipRequested(DialogueSkipRequested _)
        {
            CompleteTypingImmediately();
        }

        /// <summary>
        /// 대화 종료 → 패널 비활성화.
        /// </summary>
        private void OnDialogueEnded(DialogueEnded evt)
        {
            StopTypingCoroutine();

            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 타이핑 효과 시작. TMP maxVisibleCharacters 활용.
        /// </summary>
        private void StartTyping(string text)
        {
            StopTypingCoroutine();

            if (_dialogueText == null)
            {
                return;
            }

            _dialogueText.text = text;
            _dialogueText.maxVisibleCharacters = 0;
            _isTyping = true;
            _typingCoroutine = StartCoroutine(TypeTextCoroutine());
        }

        /// <summary>
        /// 글자를 하나씩 공개하는 코루틴.
        /// </summary>
        private IEnumerator TypeTextCoroutine()
        {
            int totalCharacters = _dialogueText.text.Length;

            for (int i = 1; i <= totalCharacters; i++)
            {
                _dialogueText.maxVisibleCharacters = i;
                yield return _typingWait;
            }

            OnTypingFinished();
        }

        /// <summary>
        /// 타이핑 즉시 완료.
        /// </summary>
        private void CompleteTypingImmediately()
        {
            if (!_isTyping)
            {
                return;
            }

            StopTypingCoroutine();

            if (_dialogueText != null)
            {
                _dialogueText.maxVisibleCharacters = _dialogueText.text.Length;
            }

            OnTypingFinished();
        }

        /// <summary>
        /// 타이핑 완료 처리 및 이벤트 발행.
        /// </summary>
        private void OnTypingFinished()
        {
            _isTyping = false;
            _typingCoroutine = null;
            EventBus.Publish(new DialogueTypingCompleted());
        }

        /// <summary>
        /// 진행 중인 타이핑 코루틴 정지.
        /// </summary>
        private void StopTypingCoroutine()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _isTyping = false;
        }
    }
}
