using System;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    [Serializable]
    public struct PromptData
    {
        public string title;
        [TextArea(3, 5)] public string description;
        public string confirmButtonText;
        public string cancelButtonText;
        public Color confirmButtonColor;
        public Color cancelButtonColor;
        public Action OnConfirm;
        public Action OnCancel;
    }
}