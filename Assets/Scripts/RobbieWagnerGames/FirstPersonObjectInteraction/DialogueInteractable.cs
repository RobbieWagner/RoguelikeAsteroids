using System.Collections;
using Ink.Runtime;
using RobbieWagnerGames.Managers;
using UnityEngine;

namespace RobbieWagnerGames.FirstPerson.Interaction
{ 
    public class DialogueInteractable : Interactable
    {

        [SerializeField] private TextAsset dialogueText;
        
        public override void OnCursorEnter()
        {
            base.OnCursorEnter();
            // Debug.Log($"Cursor entered {gameObject.name}");
        }

        public override void OnCursorStay()
        {
            base.OnCursorStay();
            // Debug.Log($"Cursor staying on {gameObject.name}");
        }

        public override void OnCursorExit()
        {
            base.OnCursorExit();
            // Debug.Log($"Cursor exited {gameObject.name}");
        }


        public override IEnumerator Interact()
        {
            InputManager.Instance.DisableActionMap(ActionMapName.EXPLORATION);

            if (dialogueText != null)
            {
                yield return Dialogue.DialogueManager.Instance.StartDialogueCo(dialogueText);
            }
            else
            {
                Debug.LogWarning($"No dialogue story assigned to {gameObject.name}");
                yield return null;
            }
            InputManager.Instance.EnableActionMap(ActionMapName.EXPLORATION);
        }
    }
}