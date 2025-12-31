using System.Collections.Generic;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class LevelNode
	{
		public Level level;
		public int tier;
		public int positionInTier; // 0-based position within tier
		public List<LevelNode> connections = new List<LevelNode>();
		
		public override string ToString()
		{
			return $"Tier {tier}, Pos {positionInTier}: {level.levelType}";
		}
	}
}