using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 배경 이미지 크로스페이드 전환 매니저.
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage1;
        [SerializeField] private Image _backgroundImage2;
        [SerializeField] private float _defaultFadeDuration = 0.5f;

        private Coroutine _fadeCoroutine;
        private int _activeImageIndex;
        private string _currentBackgroundId;

        private void Awake()
        {
            // Inspector에서 할당되지 않은 경우 자동 탐색
            if (_backgroundImage1 == null || _backgroundImage2 == null)
            {
                Image[] images = GetComponentsInChildren<Image>();
                if (images.Length >= 2)
                {
                    _backgroundImage1 = images[0];
                    _backgroundImage2 = images[1];
                }
                else
                {
                    Debug.LogWarning("[BackgroundManager] 배경 이미지 컴포넌트를 찾을 수 없음. 비활성화됨.");
                    enabled = false;
                    return;
                }
            }

            // 초기 상태: 두 이미지 모두 투명, 클릭 통과
            SetImageAlpha(_backgroundImage1, 0f);
            SetImageAlpha(_backgroundImage2, 0f);
            _backgroundImage1.raycastTarget = false;
            _backgroundImage2.raycastTarget = false;
            _activeImageIndex = 0;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BackgroundChangeRequested>(OnBackgroundChangeRequested);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BackgroundChangeRequested>(OnBackgroundChangeRequested);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnBackgroundChangeRequested(BackgroundChangeRequested evt)
        {
            // 같은 배경이면 무시
            if (_currentBackgroundId == evt.BackgroundId)
            {
                return;
            }

            Sprite sprite = LoadBackgroundSprite(evt.BackgroundId);
            if (sprite == null)
            {
                Debug.LogWarning($"[BackgroundManager] 배경 스프라이트 로드 실패: {evt.BackgroundId}");
                return;
            }

            float duration = evt.Duration > 0f ? evt.Duration : _defaultFadeDuration;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(CrossFadeBackground(sprite, evt.BackgroundId, duration));
        }

        private void OnDialogueEnded(DialogueEnded _)
        {
            // 대화 종료 시 배경 유지 (필요시 페이드 아웃 가능)
            // 현재는 배경을 유지하고 상태만 리셋
            _currentBackgroundId = null;
        }

        /// <summary>
        /// 크로스페이드 배경 전환 코루틴.
        /// </summary>
        private IEnumerator CrossFadeBackground(Sprite newSprite, string backgroundId, float duration)
        {
            Image fadeInImage = _activeImageIndex == 0 ? _backgroundImage2 : _backgroundImage1;
            Image fadeOutImage = _activeImageIndex == 0 ? _backgroundImage1 : _backgroundImage2;

            // 새 이미지 설정
            fadeInImage.sprite = newSprite;
            SetImageAlpha(fadeInImage, 0f);

            // 크로스페이드
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                SetImageAlpha(fadeInImage, t);
                SetImageAlpha(fadeOutImage, 1f - t);

                yield return null;
            }

            SetImageAlpha(fadeInImage, 1f);
            SetImageAlpha(fadeOutImage, 0f);

            // 활성 이미지 전환
            _activeImageIndex = _activeImageIndex == 0 ? 1 : 0;
            _currentBackgroundId = backgroundId;
            _fadeCoroutine = null;
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        /// <summary>
        /// Resources/Backgrounds/{backgroundId} 스프라이트 로드.
        /// </summary>
        private Sprite LoadBackgroundSprite(string backgroundId)
        {
            string path = $"Backgrounds/{backgroundId}";
            return Resources.Load<Sprite>(path);
        }
    }
}
