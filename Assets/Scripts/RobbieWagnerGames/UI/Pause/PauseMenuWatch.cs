using DG.Tweening.Core;
using RobbieWagnerGames.UI;
using RobbieWagnerGames.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.Managers
{
    public class PauseMenuWatch : MonoBehaviourSingleton<PauseMenuWatch>
    {
        [SerializeField] private PauseMenu pauseMenu;
        private List<InputActionMap> pausedActionMaps;

        protected override void Awake()
        {
            base.Awake();
            InputManager.Instance.Controls.PAUSE.PauseGame.performed += EnablePauseMenu;
            InputManager.Instance.EnableActionMap(ActionMapName.PAUSE);
            pausedActionMaps = new List<InputActionMap>();
        }

        private void EnablePauseMenu(InputAction.CallbackContext context)
        {
            if (!pauseMenu.enabled)
            {
                pauseMenu.enabled = true;
                foreach (InputActionMap actionMap in InputManager.Instance.Controls.asset.actionMaps)
                {
                    if (actionMap.enabled)
                        pausedActionMaps.Add(actionMap);
                }
            }
        }

        public void DisablePauseMenu(InputAction.CallbackContext context)
        {
            if (pauseMenu.enabled && pauseMenu.canvas.enabled)
            {
                pauseMenu.paused = false;
                pauseMenu.enabled = false;
                foreach (InputActionMap map in pausedActionMaps)
                    map.Enable();
                pausedActionMaps.Clear();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputManager.Instance.Controls.PAUSE.PauseGame.performed -= EnablePauseMenu;
        }
    }
}