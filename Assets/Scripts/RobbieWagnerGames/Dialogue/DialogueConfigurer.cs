using UnityEngine;
using Ink.Runtime;

namespace RobbieWagnerGames.Dialogue
{
    /// <summary>
    /// Utility class for configuring Ink stories
    /// </summary>
    public static class DialogueConfigurer
    {
        /// <summary>
        /// Creates a new Ink story from a text asset
        /// </summary>
        public static Story CreateStory(TextAsset inkJsonAsset)
        {
            if (inkJsonAsset == null)
            {
                Debug.LogError("Cannot create story from null text asset");
                return null;
            }

            return new Story(inkJsonAsset.text);
        }

        /// <summary>
        /// Creates a new Ink story from JSON text
        /// </summary>
        public static Story CreateStory(string inkJsonText)
        {
            if (string.IsNullOrEmpty(inkJsonText))
            {
                Debug.LogError("Cannot create story from empty JSON text");
                return null;
            }

            return new Story(inkJsonText);
        }
    }
}