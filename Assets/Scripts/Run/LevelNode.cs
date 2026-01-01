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
		public string nodeID {get; set;}
		public List<string> connectionIDs = new List<string>();

		public LevelNode()
		{
			nodeID = Guid.NewGuid().ToString();
			connectionIDs = new List<string>();
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
			
			return nodeID == other.nodeID;
		}
		
		public override int GetHashCode()
		{
			return nodeID.GetHashCode();
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