using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoveSimulation.Core;
using LoveSimulation.Data;
using LoveSimulation.Events;

namespace LoveSimulation.UI
{
    /// <summary>
    /// 호감도 변경 시 화면 상단에 잠시 표시되는 팝업 알림.
    /// </summary>
    public class AffectionNotificationUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _deltaText;
        [SerializeField] private Image _heartIcon;

        [Header("애니메이션 설정")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _displayDuration = 1.5f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _slideUpDistance = 30f;

        [Header("색상")]
        [SerializeField] private Color _positiveColor = new Color(1f, 0.4f, 0.6f);
        [SerializeField] private Color _negativeColor = new Color(0.5f, 0.5f, 0.7f);

        private RectTransform _rectTransform;
        private Coroutine _notificationCoroutine;
        private float _baseY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _baseY = _rectTransform.anchoredPosition.y;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AffectionChanged>(OnAffectionChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AffectionChanged>(OnAffectionChanged);
        }

        private void OnAffectionChanged(AffectionChanged evt)
        {
            if (evt.Delta == 0) return;

            CharacterData character = CharacterDatabase.GetCharacter(evt.CharacterId);
            string displayName = character != null ? character.DisplayName : evt.CharacterId;

            ShowNotification(displayName, evt.Delta);
        }

        /// <summary>
        /// 호감도 변경 알림 표시.
        /// </summary>
        private void ShowNotification(string characterName, int delta)
        {
            // 텍스트 설정
            if (_characterNameText != null)
            {
                _characterNameText.text = characterName;
            }

            bool isPositive = delta > 0;
            Color color = isPositive ? _positiveColor : _negativeColor;

            if (_deltaText != null)
            {
                _deltaText.text = isPositive ? $"+{delta}" : delta.ToString();
                _deltaText.color = color;
            }

            if (_heartIcon != null)
            {
                _heartIcon.color = color;
            }

            // 진행 중인 코루틴 중지 후 새로 시작
            if (_notificationCoroutine != null)
            {
                StopCoroutine(_notificationCoroutine);
            }

            _notificationCoroutine = StartCoroutine(NotificationCoroutine());
        }

        /// <summary>
        /// FadeIn → Hold → FadeOut + 슬라이드 업 애니메이션.
        /// </summary>
        private IEnumerator NotificationCoroutine()
        {
            if (_canvasGroup == null || _rectTransform == null)
            {
                yield break;
            }

            // 초기 위치 및 알파 리셋
            ResetPosition();
            _canvasGroup.alpha = 0f;

            // FadeIn
            yield return FadeAlpha(0f, 1f, _fadeInDuration);

            // Hold
            yield return new WaitForSeconds(_displayDuration);

            // FadeOut + 슬라이드 업
            yield return FadeOutWithSlide();

            _canvasGroup.alpha = 0f;
            _notificationCoroutine = null;
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

        private IEnumerator FadeOutWithSlide()
        {
            float elapsed = 0f;
            float startY = _rectTransform.anchoredPosition.y;
            float targetY = startY + _slideUpDistance;

            while (elapsed < _fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeOutDuration);

                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                Vector2 pos = _rectTransform.anchoredPosition;
                pos.y = Mathf.Lerp(startY, targetY, t);
                _rectTransform.anchoredPosition = pos;

                yield return null;
            }
        }

        private void ResetPosition()
        {
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.y = _baseY;
            _rectTransform.anchoredPosition = pos;
        }
    }
}
