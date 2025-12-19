using System;
using System.Collections.Generic;

namespace RobbieWagnerGames.Utilities.SaveData
{
    [Serializable]
    public class GameSaveData
    {
        public string savePlayerName = "Player";

        public List<float> saveColorRGB = new List<float>() {.5f, .5f, .5f};
    }
}