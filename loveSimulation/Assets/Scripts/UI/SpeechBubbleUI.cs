using System.Collections;
using UnityEngine;
using TMPro;
using LoveSimulation.Data;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 로비 캐릭터 말풍선 UI. 클릭 시 랜덤 대사를 페이드 애니메이션으로 표시.
    /// </summary>
    public class SpeechBubbleUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        [Header("애니메이션 설정")]
        [SerializeField] private float _fadeInDuration = 0.25f;
        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _fadeOutDuration = 0.4f;

        private Coroutine _bubbleCoroutine;
        private int _lastLineIndex = -1;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 랜덤 대사를 말풍선에 표시. 연속 중복 방지.
        /// </summary>
        public void ShowRandomLine()
        {
            string[] lines = LobbyDialogueDatabase.GetAvailableLines();
            if (lines == null || lines.Length == 0)
            {
                return;
            }

            int index = PickNonRepeatingIndex(lines.Length);
            _lastLineIndex = index;

            if (_dialogueText != null)
            {
                _dialogueText.text = lines[index];
            }

            // 진행 중인 코루틴 중지 후 새로 시작
            if (_bubbleCoroutine != null)
            {
                StopCoroutine(_bubbleCoroutine);
            }

            _bubbleCoroutine = StartCoroutine(BubbleCoroutine());
        }

        /// <summary>
        /// 연속 중복을 방지하는 인덱스 선택.
        /// </summary>
        private int PickNonRepeatingIndex(int count)
        {
            if (count <= 1)
            {
                return 0;
            }

            int index;
            do
            {
                index = Random.Range(0, count);
            } while (index == _lastLineIndex);

            return index;
        }

        /// <summary>
        /// FadeIn -> Hold -> FadeOut 애니메이션.
        /// </summary>
        private IEnumerator BubbleCoroutine()
        {
            if (_canvasGroup == null)
            {
                yield break;
            }

            _canvasGroup.alpha = 0f;

            // FadeIn
            yield return FadeAlpha(0f, 1f, _fadeInDuration);

            // Hold
            yield return new WaitForSeconds(_displayDuration);

            // FadeOut
            yield return FadeAlpha(1f, 0f, _fadeOutDuration);

            _canvasGroup.alpha = 0f;
            _bubbleCoroutine = null;
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
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
