using RobbieWagnerGames.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class Menu : MonoBehaviour
    {
        [FormerlySerializedAs("thisCanvas")]
        [SerializeField] public Canvas canvas;

        [SerializeField] protected Button backButton;
        //[HideInInspector] public Canvas lastMenu;
        [HideInInspector] public Menu lastMenu;

        [SerializeField] private GameObject defaultSelection;

        public Coroutine menuCoroutine = null;

        protected virtual void Awake()
        {
            
        }
        
        protected virtual void OnEnable()
        {
            ToggleButtonInteractibility(true);

            if (backButton != null)
            {
                backButton.onClick.AddListener(BackToLastMenu);
                InputManager.Instance.Controls.UI.Cancel.performed += BackToLastMenu;
            }
            EventSystemManager.Instance.eventSystem.SetSelectedGameObject(defaultSelection);
            InputManager.Instance.EnableActionMap(ActionMapName.UI);
        }

        protected virtual void OnDisable()
        {
            ToggleButtonInteractibility(false);

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(BackToLastMenu);
                InputManager.Instance.Controls.UI.Cancel.performed -= BackToLastMenu;
            }
            if(EventSystemManager.Instance != null)
                EventSystemManager.Instance.eventSystem.SetSelectedGameObject(null);
        }

        protected virtual void ToggleButtonInteractibility(bool toggleOn)
        {
            if(backButton != null) backButton.interactable = toggleOn;
        }

        protected virtual void BackToLastMenu(InputAction.CallbackContext context)
        {
            BackToLastMenu();
        }

        protected virtual void BackToLastMenu()
        {
            if(lastMenu != null)
            {
                StartCoroutine(SwapMenu(this, lastMenu));
            }
        }

        protected virtual IEnumerator SwapMenu(Menu active, Menu next, bool setAsLastMenu = true)
        {
            yield return new WaitForSecondsRealtime(.1f);

            next.canvas.enabled = true;
            next.ToggleButtonInteractibility(true);
            if(setAsLastMenu)
                next.lastMenu = active;
            active.canvas.enabled = false;

            active.enabled = false;
            next.enabled = true;

            StopCoroutine(SwapMenu(active, next));
        }
    }
}
