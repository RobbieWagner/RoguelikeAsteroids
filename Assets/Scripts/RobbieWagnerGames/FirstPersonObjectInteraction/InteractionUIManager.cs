using System;
using System.Collections.Generic;
using RobbieWagnerGames.Managers;
using RobbieWagnerGames.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.FirstPerson.Interaction
{
    public class InteractionUIManager : MonoBehaviourSingleton<InteractionUIManager>
    {
        [SerializeField] private Image cursorImage;
        private Sprite defaultCursorSprite = null;

        protected override void Awake()
        {
            base.Awake();
            if (cursorImage != null)
            {
                defaultCursorSprite = cursorImage.sprite;
            }

            InputManager.Instance.onActionMapsUpdated += UpdateCursorState;
        }

        public void UpdateCursor(Sprite newSprite)
        {
            cursorImage.sprite = newSprite != null ? newSprite : defaultCursorSprite;
        }

        public void DisableCursor()
        {
            cursorImage.enabled = false;
        }

        public void EnableCursor()
        {
            if(InputManager.Instance.CurrentActiveMaps.Contains(ActionMapName.GAME))
                cursorImage.enabled = true;
        }
        
        private void UpdateCursorState(List<ActionMapName> activeMaps)
        {
            if(!activeMaps.Contains(ActionMapName.GAME))
            {
                DisableCursor();
            }
        }
    }
}