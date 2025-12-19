using UnityEngine;

namespace RobbieWagnerGames.UnityExtensions
{
    /// <summary>
    /// Provides extension methods for Unity's Color and Color32 structs
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Converts Color to hexadecimal string
        /// </summary>
        /// <param name="color">Source color</param>
        /// <param name="includeAlpha">Whether to include alpha channel</param>
        /// <param name="includeHashPrefix">Whether to include '#' prefix</param>
        public static string ToHex(this Color color, 
            bool includeAlpha = true, 
            bool includeHashPrefix = true)
        {
            string hex = includeAlpha ? 
                ColorUtility.ToHtmlStringRGBA(color) : 
                ColorUtility.ToHtmlStringRGB(color);
            
            return includeHashPrefix ? $"#{hex}" : hex;
        }

        /// <summary>
        /// Converts Color32 to hexadecimal string
        /// </summary>
        public static string ToHex(this Color32 color32, 
            bool includeAlpha = true, 
            bool includeHashPrefix = true)
        {
            return ((Color)color32).ToHex(includeAlpha, includeHashPrefix);
        }

        /// <summary>
        /// Creates new color with modified alpha
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Creates new Color32 with modified alpha
        /// </summary>
        public static Color32 WithAlpha(this Color32 color, byte alpha)
        {
            return new Color32(color.r, color.g, color.b, alpha);
        }
    }
}