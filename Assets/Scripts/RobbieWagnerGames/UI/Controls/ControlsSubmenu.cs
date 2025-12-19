using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class ControlsSubmenu : MenuTab
    {
        //[SerializeField] private TextMeshProUGUI sectionName;
        [SerializeField] public VerticalLayoutGroup actions;
        [SerializeField] public VerticalLayoutGroup keyBinds;

        [SerializeField] private ControlsLibrary controlsLibrary;

        [SerializeField] private List<TextMeshProUGUI> actionTexts;
        [SerializeField] private List<TextMeshProUGUI> keybindTexts;

        public override void BuildTab()
        {
            base.BuildTab();

            //sectionName.text = tabName;

            Dictionary<InputType, ActionMapIconData> dict = controlsLibrary.dict;

            int action = 0;
            foreach(KeyValuePair<InputType, ActionMapIconData> keyValuePair in dict)
            {
                if (action >= actionTexts.Count)
                    break;

                TextMeshProUGUI actionText = actionTexts[action];
                TextMeshProUGUI keyBind = keybindTexts[action];

                actionText.text = AddSpacesToString(keyValuePair.Key.ToString(), false);
                keyBind.text = keyValuePair.Value.mkbInputString;

                action++;
            }
        }

    }
}