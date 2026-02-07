using UnityEngine;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 제네릭 싱글톤 베이스 클래스. DontDestroyOnLoad 적용.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스가 이미 파괴됨. null 반환.");
                    return null;
                }

                lock (_lock)
                {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 인스턴스 존재 여부 확인. 종료 중이면 false.
        /// </summary>
        public static bool HasInstance
        {
            get
            {
                if (_isQuitting)
                {
                    return false;
                }

                lock (_lock)
                {
                    return _instance != null;
                }
            }
        }

        protected virtual void Awake()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)this;
                    DontDestroyOnLoad(gameObject);
                    OnSingletonAwake();
                }
                else if (_instance != this)
                {
                    Debug.LogWarning($"[Singleton] '{typeof(T)}' 중복 인스턴스 파괴: {gameObject.name}");
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// 싱글톤 초기화 시 호출. Awake 대신 이 메서드를 오버라이드.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            lock (_lock)
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }
        }
    }
}
