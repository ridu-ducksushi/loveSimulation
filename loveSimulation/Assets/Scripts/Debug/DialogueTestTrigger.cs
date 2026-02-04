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
        [Tooltip("Resources/Dialogues 폴더의 JSON 파일을 드래그하세요")]
        [SerializeField] private TextAsset _dialogueFile;

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

                if (_dialogueFile == null)
                {
                    Debug.LogError("[DialogueTestTrigger] 대화 파일이 설정되지 않음.");
                    return;
                }

                if (!DialogueManager.Instance.IsDialogueActive)
                {
                    string dialogueId = _dialogueFile.name;
                    Debug.Log($"[DialogueTestTrigger] 대화 시작 요청: {dialogueId}");
                    DialogueManager.Instance.StartDialogue(dialogueId);
                }
            }
        }
    }
}
