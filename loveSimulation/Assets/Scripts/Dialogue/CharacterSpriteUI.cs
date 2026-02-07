using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Dialogue
{
    /// <summary>
    /// 캐릭터 스프라이트 표시 UI. Left/Center/Right 3개 슬롯 지원.
    /// </summary>
    public class CharacterSpriteUI : MonoBehaviour
    {
        [System.Serializable]
        private class CharacterSlot
        {
            public Image FrontImage;
            public Image BackImage;
            [System.NonSerialized] public string CurrentCharacterId;
            [System.NonSerialized] public string CurrentEmotion;
            [System.NonSerialized] public Coroutine FadeCoroutine;
        }

        [Header("Left Position")]
        [SerializeField] private Image _leftFront;
        [SerializeField] private Image _leftBack;

        [Header("Center Position")]
        [SerializeField] private Image _centerFront;
        [SerializeField] private Image _centerBack;

        [Header("Right Position")]
        [SerializeField] private Image _rightFront;
        [SerializeField] private Image _rightBack;

        [Header("Settings")]
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _dimmedAlpha = 0.5f;

        private Dictionary<CharacterPosition, CharacterSlot> _slots;
        private CharacterPosition? _activeSpeakerPosition;

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            _slots = new Dictionary<CharacterPosition, CharacterSlot>();

            // Left 슬롯
            if (_leftFront != null || _leftBack != null)
            {
                CharacterSlot leftSlot = new CharacterSlot
                {
                    FrontImage = _leftFront,
                    BackImage = _leftBack
                };
                _slots[CharacterPosition.Left] = leftSlot;
                InitializeSlotImages(leftSlot);
            }

            // Center 슬롯
            if (_centerFront != null || _centerBack != null)
            {
                CharacterSlot centerSlot = new CharacterSlot
                {
                    FrontImage = _centerFront,
                    BackImage = _centerBack
                };
                _slots[CharacterPosition.Center] = centerSlot;
                InitializeSlotImages(centerSlot);
            }

            // Right 슬롯
            if (_rightFront != null || _rightBack != null)
            {
                CharacterSlot rightSlot = new CharacterSlot
                {
                    FrontImage = _rightFront,
                    BackImage = _rightBack
                };
                _slots[CharacterPosition.Right] = rightSlot;
                InitializeSlotImages(rightSlot);
            }

            if (_slots.Count == 0)
            {
                Debug.LogWarning("[CharacterSpriteUI] 슬롯이 하나도 설정되지 않음. 비활성화됨.");
                enabled = false;
            }
        }

        private void InitializeSlotImages(CharacterSlot slot)
        {
            if (slot.FrontImage != null)
            {
                SetImageAlpha(slot.FrontImage, 0f);
                slot.FrontImage.raycastTarget = false;
            }

            if (slot.BackImage != null)
            {
                SetImageAlpha(slot.BackImage, 0f);
                slot.BackImage.raycastTarget = false;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueStarted>(OnDialogueStarted);
            EventBus.Subscribe<CharacterDisplayRequested>(OnCharacterDisplayRequested);
            EventBus.Subscribe<CharacterHideRequested>(OnCharacterHideRequested);
            EventBus.Subscribe<CharacterHighlightRequested>(OnCharacterHighlightRequested);
            EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueStarted>(OnDialogueStarted);
            EventBus.Unsubscribe<CharacterDisplayRequested>(OnCharacterDisplayRequested);
            EventBus.Unsubscribe<CharacterHideRequested>(OnCharacterHideRequested);
            EventBus.Unsubscribe<CharacterHighlightRequested>(OnCharacterHighlightRequested);
            EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
        }

        private void OnDialogueStarted(DialogueStarted _)
        {
            // 대화 시작 시 모든 캐릭터 슬롯 초기화
            _activeSpeakerPosition = null;
            foreach (var kvp in _slots)
            {
                ClearSlotImmediate(kvp.Value);
            }
        }

        /// <summary>
        /// 슬롯 즉시 초기화 (페이드 없이).
        /// </summary>
        private void ClearSlotImmediate(CharacterSlot slot)
        {
            if (slot.FadeCoroutine != null)
            {
                StopCoroutine(slot.FadeCoroutine);
                slot.FadeCoroutine = null;
            }

            slot.CurrentCharacterId = null;
            slot.CurrentEmotion = null;

            if (slot.FrontImage != null)
            {
                SetImageAlpha(slot.FrontImage, 0f);
                slot.FrontImage.sprite = null;
            }

            if (slot.BackImage != null)
            {
                SetImageAlpha(slot.BackImage, 0f);
                slot.BackImage.sprite = null;
            }
        }

        private void OnCharacterDisplayRequested(CharacterDisplayRequested evt)
        {
            // 해당 위치 슬롯이 없으면 무시
            if (!_slots.TryGetValue(evt.Position, out CharacterSlot slot))
            {
                return;
            }

            string characterId = evt.CharacterId;
            string emotion = string.IsNullOrEmpty(evt.Emotion) ? "neutral" : evt.Emotion;

            // 같은 캐릭터/표정이면 무시
            if (slot.CurrentCharacterId == characterId && slot.CurrentEmotion == emotion)
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

            // 기존 코루틴 중지
            if (slot.FadeCoroutine != null)
            {
                StopCoroutine(slot.FadeCoroutine);
            }

            slot.FadeCoroutine = StartCoroutine(TransitionSlot(slot, sprite, characterId, emotion, evt.FadeIn));
        }

        private void OnCharacterHideRequested(CharacterHideRequested evt)
        {
            if (evt.Position == null)
            {
                // 모든 슬롯 숨김
                foreach (var kvp in _slots)
                {
                    HideSlot(kvp.Value);
                }
            }
            else if (_slots.TryGetValue(evt.Position.Value, out CharacterSlot slot))
            {
                HideSlot(slot);
            }
        }

        private void OnCharacterHighlightRequested(CharacterHighlightRequested evt)
        {
            _activeSpeakerPosition = evt.Position;
            ApplyHighlight();
        }

        private void OnDialogueEnded(DialogueEnded _)
        {
            _activeSpeakerPosition = null;
            foreach (var kvp in _slots)
            {
                HideSlot(kvp.Value);
            }
        }

        /// <summary>
        /// 슬롯 전환 코루틴. 크로스페이드 방식.
        /// </summary>
        private IEnumerator TransitionSlot(CharacterSlot slot, Sprite newSprite, string characterId, string emotion, bool fadeIn)
        {
            float targetAlpha = GetTargetAlpha(slot);

            // BackImage가 있으면 크로스페이드, 없으면 단순 페이드
            if (slot.BackImage != null && slot.FrontImage != null)
            {
                // 현재 Front를 Back으로 복사
                if (!string.IsNullOrEmpty(slot.CurrentCharacterId))
                {
                    slot.BackImage.sprite = slot.FrontImage.sprite;
                    SetImageAlpha(slot.BackImage, GetCurrentAlpha(slot.FrontImage));
                }

                // Front에 새 스프라이트 설정
                slot.FrontImage.sprite = newSprite;

                if (fadeIn)
                {
                    SetImageAlpha(slot.FrontImage, 0f);
                    yield return CrossFade(slot.FrontImage, slot.BackImage, targetAlpha, _fadeDuration);
                }
                else
                {
                    SetImageAlpha(slot.FrontImage, targetAlpha);
                    SetImageAlpha(slot.BackImage, 0f);
                }
            }
            else if (slot.FrontImage != null)
            {
                // 단순 페이드
                if (!string.IsNullOrEmpty(slot.CurrentCharacterId) && fadeIn)
                {
                    yield return Fade(slot.FrontImage, targetAlpha, 0f, _fadeDuration / 2);
                }

                slot.FrontImage.sprite = newSprite;

                if (fadeIn)
                {
                    yield return Fade(slot.FrontImage, 0f, targetAlpha, _fadeDuration / 2);
                }
                else
                {
                    SetImageAlpha(slot.FrontImage, targetAlpha);
                }
            }

            slot.CurrentCharacterId = characterId;
            slot.CurrentEmotion = emotion;
            slot.FadeCoroutine = null;
        }

        /// <summary>
        /// 슬롯 숨김.
        /// </summary>
        private void HideSlot(CharacterSlot slot)
        {
            if (slot.FadeCoroutine != null)
            {
                StopCoroutine(slot.FadeCoroutine);
            }

            slot.FadeCoroutine = StartCoroutine(FadeOutSlot(slot));
        }

        private IEnumerator FadeOutSlot(CharacterSlot slot)
        {
            if (!string.IsNullOrEmpty(slot.CurrentCharacterId))
            {
                if (slot.FrontImage != null)
                {
                    yield return Fade(slot.FrontImage, GetCurrentAlpha(slot.FrontImage), 0f, _fadeDuration);
                }

                if (slot.BackImage != null)
                {
                    SetImageAlpha(slot.BackImage, 0f);
                }
            }

            slot.CurrentCharacterId = null;
            slot.CurrentEmotion = null;
            if (slot.FrontImage != null)
            {
                slot.FrontImage.sprite = null;
            }
            if (slot.BackImage != null)
            {
                slot.BackImage.sprite = null;
            }
            slot.FadeCoroutine = null;
        }

        /// <summary>
        /// 화자 강조 적용.
        /// </summary>
        private void ApplyHighlight()
        {
            foreach (var kvp in _slots)
            {
                CharacterSlot slot = kvp.Value;
                if (string.IsNullOrEmpty(slot.CurrentCharacterId))
                {
                    continue;
                }

                float targetAlpha = GetTargetAlpha(slot);
                if (slot.FrontImage != null)
                {
                    SetImageAlpha(slot.FrontImage, targetAlpha);
                }
            }
        }

        /// <summary>
        /// 슬롯의 목표 알파 계산 (화자 강조 고려).
        /// </summary>
        private float GetTargetAlpha(CharacterSlot slot)
        {
            // 화자 강조가 없으면 모두 1.0
            if (_activeSpeakerPosition == null)
            {
                return 1f;
            }

            // 해당 슬롯이 화자면 1.0, 아니면 dimmed
            foreach (var kvp in _slots)
            {
                if (kvp.Value == slot)
                {
                    return kvp.Key == _activeSpeakerPosition.Value ? 1f : _dimmedAlpha;
                }
            }

            return 1f;
        }

        /// <summary>
        /// 크로스페이드 코루틴.
        /// </summary>
        private IEnumerator CrossFade(Image front, Image back, float targetAlpha, float duration)
        {
            float elapsed = 0f;
            float backStart = GetCurrentAlpha(back);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                SetImageAlpha(front, Mathf.Lerp(0f, targetAlpha, t));
                SetImageAlpha(back, Mathf.Lerp(backStart, 0f, t));

                yield return null;
            }

            SetImageAlpha(front, targetAlpha);
            SetImageAlpha(back, 0f);
        }

        /// <summary>
        /// 단순 페이드 코루틴.
        /// </summary>
        private IEnumerator Fade(Image image, float from, float to, float duration)
        {
            if (image == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetImageAlpha(image, Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetImageAlpha(image, to);
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

        private float GetCurrentAlpha(Image image)
        {
            return image != null ? image.color.a : 0f;
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
