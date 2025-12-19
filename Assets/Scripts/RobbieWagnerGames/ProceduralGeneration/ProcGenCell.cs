using System.Collections.Generic;
using System.Text;

namespace RobbieWagnerGames.ProcGen
{
    public enum CellType
    {
        Blank,
        Road
    }

    [System.Serializable]
    public class ProcGenCell
    {
        public int Value { get; set; } = -1;
        public int X { get; private set; }
        public int Y { get; private set; }
        public List<int> Options { get; set; }

        public ProcGenCell(int x, int y)
        {
            X = x;
            Y = y;
            Options = new List<int>();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"({X},{Y}):");
            
            stringBuilder.Append("{");
            foreach (int option in Options)
            {
                stringBuilder.Append(option.ToString() + ",");
            }
            stringBuilder.Append("}");

            return stringBuilder.ToString();
        }
    }
}