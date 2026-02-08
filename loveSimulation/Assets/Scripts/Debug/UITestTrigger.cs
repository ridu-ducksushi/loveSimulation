using UnityEngine;
using UnityEngine.InputSystem;
using LoveSimulation.Core;
using LoveSimulation.Events;

namespace LoveSimulation.Testing
{
    /// <summary>
    /// 세이브/로드 UI 및 호감도 알림 테스트용 키보드 트리거.
    /// F5: 저장 UI, F9: 로드 UI, F6: 호감도 +5, F7: 호감도 -3
    /// </summary>
    public class UITestTrigger : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Tooltip("호감도 테스트 대상 캐릭터 ID")]
        [SerializeField] private string _targetCharacterId = "duke";

        [Tooltip("F6 호감도 증가량")]
        [SerializeField] private int _affectionUpAmount = 5;

        [Tooltip("F7 호감도 감소량")]
        [SerializeField] private int _affectionDownAmount = -3;

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                OpenSaveUI();
            }

            if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                OpenLoadUI();
            }

            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                AddAffection(_affectionUpAmount);
            }

            if (Keyboard.current.f7Key.wasPressedThisFrame)
            {
                AddAffection(_affectionDownAmount);
            }
        }

        private void OpenSaveUI()
        {
            Debug.Log("[UITestTrigger] F5 → 저장 UI 열기");
            EventBus.Publish(new SaveLoadUIRequested { IsSaveMode = true });
        }

        private void OpenLoadUI()
        {
            Debug.Log("[UITestTrigger] F9 → 불러오기 UI 열기");
            EventBus.Publish(new SaveLoadUIRequested { IsSaveMode = false });
        }

        private void AddAffection(int amount)
        {
            Debug.Log($"[UITestTrigger] 호감도 변경: {_targetCharacterId} {(amount >= 0 ? "+" : "")}{amount}");
            GameData.AddAffection(_targetCharacterId, amount);
        }
#endif
    }
}
