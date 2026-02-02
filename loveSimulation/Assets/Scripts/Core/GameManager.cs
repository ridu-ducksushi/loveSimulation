using UnityEngine;
using LoveSimulation.Events;

namespace LoveSimulation.Core
{
    /// <summary>
    /// 게임 상태 머신 싱글톤. 전체 게임 상태를 관리.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState _initialState = GameState.Title;

        private GameState _currentState;
        private float _playTime;

        public GameState CurrentState => _currentState;
        public float PlayTime => _playTime;

        protected override void OnSingletonAwake()
        {
            _currentState = _initialState;
            Debug.Log($"[GameManager] 초기화 완료. 상태: {_currentState}");
        }

        private void Update()
        {
            if (_currentState == GameState.Playing || _currentState == GameState.Dialogue)
            {
                _playTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// 게임 상태를 변경하고 이벤트를 발행.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            GameState previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] 상태 변경: {previousState} → {newState}");

            EventBus.Publish(new GameStateChanged
            {
                PreviousState = previousState,
                NewState = newState
            });
        }

        /// <summary>
        /// 플레이 시간을 로드된 데이터로 복원.
        /// </summary>
        public void SetPlayTime(float time)
        {
            _playTime = time;
        }
    }
}
