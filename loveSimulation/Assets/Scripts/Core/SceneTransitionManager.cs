using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LoveSimulation.Events;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 씬 전환 + 페이드 인/아웃 매니저. Coroutine 기반.
    /// </summary>
    public class SceneTransitionManager : Singleton<SceneTransitionManager>
    {
        private const float DefaultFadeDuration = 0.5f;

        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        private bool _isTransitioning;
        private Canvas _fadeCanvas;

        protected override void OnSingletonAwake()
        {
            EnsureFadeCanvas();
            Debug.Log("[SceneTransitionManager] 초기화 완료.");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SceneTransitionRequested>(OnSceneTransitionRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneTransitionRequested>(OnSceneTransitionRequested);
        }

        private void OnSceneTransitionRequested(SceneTransitionRequested e)
        {
            float duration = e.FadeDuration > 0f ? e.FadeDuration : DefaultFadeDuration;
            TransitionTo(e.SceneName, duration);
        }

        /// <summary>
        /// 지정 씬으로 페이드 전환.
        /// </summary>
        public void TransitionTo(string sceneName, float fadeDuration = DefaultFadeDuration)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] 이미 씬 전환 중.");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneTransitionManager] 씬 이름이 비어있음.");
                return;
            }

            StartCoroutine(TransitionCoroutine(sceneName, fadeDuration));
        }

        private IEnumerator TransitionCoroutine(string sceneName, float fadeDuration)
        {
            _isTransitioning = true;

            // 페이드 아웃 (화면 어두워짐)
            yield return FadeCoroutine(0f, 1f, fadeDuration);

            // 씬 비동기 로드
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneTransitionManager] 씬 로드 실패: {sceneName}");
                yield return FadeCoroutine(1f, 0f, fadeDuration);
                _isTransitioning = false;
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 페이드 인 (화면 밝아짐)
            yield return FadeCoroutine(1f, 0f, fadeDuration);

            _isTransitioning = false;

            EventBus.Publish(new SceneTransitionCompleted
            {
                SceneName = sceneName
            });

            Debug.Log($"[SceneTransitionManager] 씬 전환 완료: {sceneName}");
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration)
        {
            if (_fadeCanvasGroup == null)
            {
                yield break;
            }

            _fadeCanvasGroup.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _fadeCanvasGroup.alpha = to;

            // 완전히 투명해지면 비활성화
            if (Mathf.Approximately(to, 0f))
            {
                _fadeCanvasGroup.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 페이드용 Canvas와 CanvasGroup을 자동 생성 (없을 경우).
        /// </summary>
        private void EnsureFadeCanvas()
        {
            if (_fadeCanvasGroup != null)
            {
                return;
            }

            // 페이드 캔버스 생성
            var fadeObj = new GameObject("FadeCanvas");
            fadeObj.transform.SetParent(transform);

            _fadeCanvas = fadeObj.AddComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.sortingOrder = 999;

            fadeObj.AddComponent<CanvasScaler>();

            var raycaster = fadeObj.AddComponent<GraphicRaycaster>();

            _fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();

            // 검은색 이미지 생성
            var imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(fadeObj.transform, false);

            var image = imageObj.AddComponent<Image>();
            image.color = Color.black;

            var rectTransform = image.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            _fadeCanvasGroup.alpha = 0f;
            fadeObj.SetActive(false);

            Debug.Log("[SceneTransitionManager] 페이드 캔버스 자동 생성 완료.");
        }
    }
}
