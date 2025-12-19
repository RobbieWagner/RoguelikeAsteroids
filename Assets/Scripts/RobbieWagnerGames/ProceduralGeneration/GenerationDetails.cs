using System.Collections.Generic;

namespace RobbieWagnerGames.ProcGen
{
    public class GenerationDetails
    {
        public int Possibilities { get; set; }
        public int Seed { get; set; } = -1;
        public Dictionary<int, List<int>> AboveAllowList { get; set; }
        public Dictionary<int, List<int>> BelowAllowList { get; set; }
        public Dictionary<int, List<int>> LeftAllowList { get; set; }
        public Dictionary<int, List<int>> RightAllowList { get; set; }

        // key = possibility, value = weight (default 1 spawn chance)
        public Dictionary<int, int> Weights { get; set; }
    }
}