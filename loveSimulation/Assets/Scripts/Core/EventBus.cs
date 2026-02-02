using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 정적 타입 안전 이벤트 버스. 구조체 이벤트만 허용.
    /// </summary>
    public static class EventBus
    {
        /// <summary>
        /// 타입별 이벤트 채널을 관리하는 내부 제네릭 클래스.
        /// </summary>
        private static class Channel<T> where T : struct
        {
            private static readonly List<Action<T>> _handlers = new List<Action<T>>();

            public static void Subscribe(Action<T> handler)
            {
                if (handler == null)
                {
                    Debug.LogError("[EventBus] null 핸들러 구독 시도.");
                    return;
                }

                if (!_handlers.Contains(handler))
                {
                    _handlers.Add(handler);
                }
            }

            public static void Unsubscribe(Action<T> handler)
            {
                if (handler == null)
                {
                    return;
                }

                _handlers.Remove(handler);
            }

            public static void Publish(T message)
            {
                // 역순 순회로 구독 해제 안전성 확보
                for (int i = _handlers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _handlers[i]?.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] '{typeof(T).Name}' 이벤트 처리 중 예외: {e}");
                    }
                }
            }

            public static void Clear()
            {
                _handlers.Clear();
            }
        }

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            Channel<T>.Subscribe(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            Channel<T>.Unsubscribe(handler);
        }

        public static void Publish<T>(T message) where T : struct
        {
            Channel<T>.Publish(message);
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 구독을 해제. 테스트 및 정리 용도.
        /// </summary>
        public static void Clear<T>() where T : struct
        {
            Channel<T>.Clear();
        }
    }
}
