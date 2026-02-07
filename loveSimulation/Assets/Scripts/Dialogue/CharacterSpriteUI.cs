using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 캐릭터 스프라이트 표시 UI. 화자만 중앙에 표시하는 심플 모드.
    /// </summary>
    public class CharacterSpriteUI : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private float _fadeDuration = 0.3f;

        private Coroutine _fadeCoroutine;
        private string _currentCharacterId;
        private string _currentEmotion;

        private void Awake()
        {
            if (_characterImage == null)
            {
                _characterImage = GetComponentInChildren<Image>();
            }

            if (_characterImage == null)
            {
                Debug.LogWarning("[CharacterSpriteUI] Image 컴포넌트를 찾을 수 없음. 비활성화됨.");
                enabled = false;
                return;
            }

            // 초기 상태: 숨김, 클릭 통과
            SetImageAlpha(0f);
            _characterImage.raycastTarget = false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CharacterDisplayRequested>(OnCharacterDisplayRequested);
            EventBus.Subscribe<CharacterHideRequested>(OnCharacterHideRequested);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CharacterDisplayRequested>(OnCharacterDisplayRequested);
            EventBus.Unsubscribe<CharacterHideRequested>(OnCharacterHideRequested);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnCharacterDisplayRequested(CharacterDisplayRequested evt)
        {
            // 심플 모드: Center 위치만 사용
            if (evt.Position != CharacterPosition.Center)
            {
                return;
            }

            string characterId = evt.CharacterId;
            string emotion = string.IsNullOrEmpty(evt.Emotion) ? "neutral" : evt.Emotion;

            // 같은 캐릭터/표정이면 무시
            if (_currentCharacterId == characterId && _currentEmotion == emotion)
            {
                return;
            }

            // 스프라이트 로드
            Sprite sprite = LoadCharacterSprite(characterId, emotion);
            if (sprite == null)
            {
                Debug.LogWarning($"[CharacterSpriteUI] 스프라이트 로드 실패: {characterId}_{emotion}");
                return;
            }

            // 페이드 전환
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(TransitionCharacter(sprite, characterId, emotion, evt.FadeIn));
        }

        private void OnCharacterHideRequested(CharacterHideRequested evt)
        {
            // 모든 캐릭터 숨김 또는 Center 위치
            if (evt.Position == null || evt.Position == CharacterPosition.Center)
            {
                HideCharacter();
            }
        }

        private void OnDialogueEnded(DialogueEnded _)
        {
            HideCharacter();
        }

        /// <summary>
        /// 캐릭터 스프라이트 전환 코루틴.
        /// </summary>
        private IEnumerator TransitionCharacter(Sprite newSprite, string characterId, string emotion, bool fadeIn)
        {
            // 현재 캐릭터가 있으면 페이드 아웃
            if (!string.IsNullOrEmpty(_currentCharacterId))
            {
                yield return Fade(1f, 0f, _fadeDuration);
            }

            // 새 스프라이트 설정
            _characterImage.sprite = newSprite;
            _currentCharacterId = characterId;
            _currentEmotion = emotion;

            // 페이드 인
            if (fadeIn)
            {
                yield return Fade(0f, 1f, _fadeDuration);
            }
            else
            {
                SetImageAlpha(1f);
            }

            _fadeCoroutine = null;
        }

        /// <summary>
        /// 캐릭터 숨김.
        /// </summary>
        private void HideCharacter()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeOutAndClear());
        }

        private IEnumerator FadeOutAndClear()
        {
            if (!string.IsNullOrEmpty(_currentCharacterId))
            {
                yield return Fade(1f, 0f, _fadeDuration);
            }

            _currentCharacterId = null;
            _currentEmotion = null;
            _characterImage.sprite = null;
            _fadeCoroutine = null;
        }

        /// <summary>
        /// 페이드 코루틴.
        /// </summary>
        private IEnumerator Fade(float from, float to, float duration)
        {
            if (_characterImage == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetImageAlpha(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetImageAlpha(to);
        }

        private void SetImageAlpha(float alpha)
        {
            if (_characterImage == null)
            {
                return;
            }

            Color color = _characterImage.color;
            color.a = alpha;
            _characterImage.color = color;
        }

        /// <summary>
        /// Resources/CharacterSprites/{characterId}_{emotion} 스프라이트 로드.
        /// </summary>
        private Sprite LoadCharacterSprite(string characterId, string emotion)
        {
            string path = $"CharacterSprites/{characterId}_{emotion}";
            Sprite sprite = Resources.Load<Sprite>(path);

            // 표정 스프라이트가 없으면 neutral로 폴백
            if (sprite == null && emotion != "neutral")
            {
                path = $"CharacterSprites/{characterId}_neutral";
                sprite = Resources.Load<Sprite>(path);
            }

            return sprite;
        }
    }
}
