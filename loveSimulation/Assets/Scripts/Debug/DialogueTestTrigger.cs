using System.Collections;
using UnityEngine;
using LoveSimulation.Dialogue;

namespace LoveSimulation.Testing
{
    /// <summary>
    /// 앱 시작 시 지정된 대화를 자동 실행하는 트리거.
    /// </summary>
    public class DialogueTestTrigger : MonoBehaviour
    {
        [Tooltip("Resources/Dialogues 폴더 내 대화 ID (확장자 제외)")]
        [SerializeField] private string _dialogueId = "chapter01";

        private IEnumerator Start()
        {
            // 모든 컴포넌트 초기화 완료 대기
            yield return null;

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueTestTrigger] DialogueManager 인스턴스 없음.");
                yield break;
            }

            Debug.Log($"[DialogueTestTrigger] 대화 자동 시작: {_dialogueId}");
            DialogueManager.Instance.StartDialogue(_dialogueId);
        }
    }
}
