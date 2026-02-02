using UnityEngine;
using UnityEngine.InputSystem;
using LoveSimulation.Dialogue;

namespace LoveSimulation.Testing
{
    /// <summary>
    /// 대화 시스템 테스트용 트리거. 빌드 시 제거할 것.
    /// </summary>
    public class DialogueTestTrigger : MonoBehaviour
    {
        [SerializeField] private string _dialogueId = "sample_dialogue";

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (DialogueManager.Instance == null)
                {
                    Debug.LogError("[DialogueTestTrigger] DialogueManager 인스턴스 없음.");
                    return;
                }

                if (!DialogueManager.Instance.IsDialogueActive)
                {
                    Debug.Log($"[DialogueTestTrigger] 대화 시작 요청: {_dialogueId}");
                    DialogueManager.Instance.StartDialogue(_dialogueId);
                }
            }
        }
    }
}
