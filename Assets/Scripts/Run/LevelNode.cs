using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class LevelNode
	{
		public Level level;
		public int tier;
		public int positionInTier;
		[JsonIgnore] public List<LevelNode> connections = new List<LevelNode>();
		public string nodeID {get; private set;}
		[JsonProperty] public List<string> connectionIDs {get; private set;}

		public LevelNode()
		{
			nodeID = Guid.NewGuid().ToString();
		}

		public void PrepForSerialization()
		{
			connectionIDs = new List<string>();
			foreach (LevelNode connection in connections)
				connectionIDs.Add(connection.nodeID);
		}

		public void DeserializeConnections(Dictionary<string, LevelNode> nodes)
		{
			connections.Clear();
            foreach (string connectionId in connectionIDs)
            {
                if (nodes.TryGetValue(connectionId, out LevelNode connectedNode))
                    connections.Add(connectedNode);
            }
		}
		
		public override string ToString()
		{
			return $"{JsonConvert.SerializeObject(this, Formatting.Indented)}";
		}
		
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
				
			LevelNode other = (LevelNode)obj;
			
			return tier == other.tier && 
			       positionInTier == other.positionInTier && 
			       level == other.level;
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + tier.GetHashCode();
				hash = hash * 23 + positionInTier.GetHashCode();
				hash = hash * 23 + (level?.GetHashCode() ?? 0);
				return hash;
			}
		}
		
		public static bool operator ==(LevelNode left, LevelNode right)
		{
			if (ReferenceEquals(left, right))
				return true;
				
			if (left is null || right is null)
				return false;
				
			return left.Equals(right);
		}
		
		public static bool operator !=(LevelNode left, LevelNode right)
		{
			return !(left == right);
		}
	}
}