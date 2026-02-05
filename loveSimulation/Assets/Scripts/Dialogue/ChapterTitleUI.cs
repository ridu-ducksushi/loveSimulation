using System.Collections;
using UnityEngine;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 챕터 제목 연출 UI. 암전 후 제목 페이드 인/아웃.
    /// </summary>
    public class ChapterTitleUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("애니메이션 설정")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _displayDuration = 2.0f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        private Coroutine _titleCoroutine;

        private void Awake()
        {
            // Inspector에서 할당되지 않은 경우 자동 탐색
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (_titleText == null)
            {
                _titleText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (_canvasGroup == null)
            {
                Debug.LogError("[ChapterTitleUI] CanvasGroup을 찾을 수 없음.");
                return;
            }

            // 초기 상태: 숨김
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ChapterTitleRequested>(OnChapterTitleRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChapterTitleRequested>(OnChapterTitleRequested);
        }

        private void OnChapterTitleRequested(ChapterTitleRequested evt)
        {
            if (string.IsNullOrEmpty(evt.Title))
            {
                EventBus.Publish(new ChapterTitleCompleted());
                return;
            }

            if (_titleCoroutine != null)
            {
                StopCoroutine(_titleCoroutine);
            }

            _titleCoroutine = StartCoroutine(ShowTitleSequence(evt.Title));
        }

        private IEnumerator ShowTitleSequence(string title)
        {
            // 제목 텍스트 설정
            if (_titleText != null)
            {
                _titleText.text = title;
                _titleText.alpha = 0f;
            }

            // 배경 즉시 표시 (검은 화면)
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            // 제목 페이드 인
            yield return FadeText(0f, 1f, _fadeInDuration);

            // 제목 표시 유지
            yield return new WaitForSeconds(_displayDuration);

            // 전체 페이드 아웃 (배경 + 텍스트)
            yield return FadeAll(1f, 0f, _fadeOutDuration);

            // 완료 후 숨김
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _titleCoroutine = null;

            // 완료 이벤트 발행
            EventBus.Publish(new ChapterTitleCompleted());
            Debug.Log($"[ChapterTitleUI] 챕터 제목 연출 완료: {title}");
        }

        private IEnumerator FadeText(float from, float to, float duration)
        {
            if (_titleText == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _titleText.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _titleText.alpha = to;
        }

        private IEnumerator FadeAll(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _canvasGroup.alpha = to;
        }
    }
}
