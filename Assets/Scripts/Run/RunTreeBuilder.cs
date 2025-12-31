using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public static class RunTreeBuilder
    {
        public static void ConnectLevelNodes(List<List<LevelNode>> levelTree)
		{
			foreach (List<LevelNode> tier in levelTree)
				foreach (LevelNode node in tier)
					node.connections.Clear();
			
			for (int tierIndex = 0; tierIndex < levelTree.Count - 1; tierIndex++)
				BuildTierConnections(levelTree, tierIndex);
			
			ValidateAllConnections(levelTree);
		}

		private static void BuildTierConnections(List<List<LevelNode>> levelTree, int fromTierIndex)
        {
            List<LevelNode> fromTier = levelTree[fromTierIndex];
            List<LevelNode> toTier = levelTree[fromTierIndex + 1];

            if (fromTier.Count == 0 || toTier.Count == 0)
                return;

            bool[,] connectionMatrix = new bool[fromTier.Count, toTier.Count];

            for (int i = 0; i < fromTier.Count; i++)
            {
                for (int j = 0; j < toTier.Count; j++)
                    connectionMatrix[i, j] = false;
                fromTier[i].connections.Clear();
            }

            List<int> indexOptions = new List<int>();
            for (int i = 0; i < toTier.Count; i++)
            {
                for(int j = 0; j < toTier.Count - i; j++)
                    indexOptions.Add(i);
            }

            for (int i = 0; i < fromTier.Count; i++)
            {
                int toIndex = indexOptions[UnityEngine.Random.Range(0, indexOptions.Count)];

                if (!WouldCreateIntersection(connectionMatrix, fromTier, toTier, i, toIndex))
                    connectionMatrix[i, toIndex] = true;
                else
                {
                    int fallbackIndex = Mathf.Clamp(
                        Mathf.RoundToInt((float)i / fromTier.Count * toTier.Count),
                        0, toTier.Count - 1
                    );

                    for (int adjust = 0; adjust < toTier.Count; adjust++)
                    {
                        int tryIndex = (fallbackIndex + adjust) % toTier.Count;
                        if (!WouldCreateIntersection(connectionMatrix, fromTier, toTier, i, tryIndex))
                        {
                            connectionMatrix[i, tryIndex] = true;
                            break;
                        }

                        tryIndex = (fallbackIndex - adjust + toTier.Count) % toTier.Count;
                        if (!WouldCreateIntersection(connectionMatrix, fromTier, toTier, i, tryIndex))
                        {
                            connectionMatrix[i, tryIndex] = true;
                            break;
                        }
                    }
                }
            }

            int maxAdditionalConnections = UnityEngine.Random.Range(0, fromTier.Count * toTier.Count / 2);
            int additionalConnectionsAdded = 0;

            for (int attempt = 0; attempt < maxAdditionalConnections * 3 && additionalConnectionsAdded < maxAdditionalConnections; attempt++)
            {
                int fromIndex = UnityEngine.Random.Range(0, fromTier.Count);
                int toIndex = UnityEngine.Random.Range(0, toTier.Count);

                if (connectionMatrix[fromIndex, toIndex])
                    continue;

                if (!WouldCreateIntersection(connectionMatrix, fromTier, toTier, fromIndex, toIndex))
                {
                    connectionMatrix[fromIndex, toIndex] = true;
                    additionalConnectionsAdded++;
                }
            }

            ConnectOrphanedNodes(fromTier, toTier, connectionMatrix);
            ConnectChildlessParents(fromTier, toTier, connectionMatrix);
        }

        private static void ConnectOrphanedNodes(List<LevelNode> fromTier, List<LevelNode> toTier, bool[,] connectionMatrix)
        {
            for (int toIndex = 0; toIndex < toTier.Count; toIndex++)
            {
                bool hasIncoming = false;
                for (int fromIndex = 0; fromIndex < fromTier.Count; fromIndex++)
                {
                    if (connectionMatrix[fromIndex, toIndex])
                    {
                        hasIncoming = true;
                        break;
                    }
                }

                if (!hasIncoming)
                {
                    bool connectionMade = false;

                    for (int fromIndex = 0; fromIndex < fromTier.Count && !connectionMade; fromIndex++)
                    {
                        if (!WouldCreateIntersection(connectionMatrix, fromTier, toTier, fromIndex, toIndex))
                        {
                            connectionMatrix[fromIndex, toIndex] = true;
                            connectionMade = true;
                        }
                    }

                    if (!connectionMade)
                    {
                        int bestFromIndex = 0;
                        int bestIntersectionSeverity = int.MaxValue;

                        for (int fromIndex = 0; fromIndex < fromTier.Count; fromIndex++)
                        {
                            int intersectionSeverity = 0;

                            for (int leftIndex = 0; leftIndex < fromIndex; leftIndex++)
                            {
                                int maxLeft = -1;
                                for (int j = toTier.Count - 1; j >= 0; j--)
                                {
                                    if (connectionMatrix[leftIndex, j])
                                    {
                                        maxLeft = j;
                                        break;
                                    }
                                }

                                if (maxLeft > toIndex)
                                    intersectionSeverity += maxLeft - toIndex;
                            }

                            for (int rightIndex = fromIndex + 1; rightIndex < fromTier.Count; rightIndex++)
                            {
                                int minRight = toTier.Count;
                                for (int j = 0; j < toTier.Count; j++)
                                {
                                    if (connectionMatrix[rightIndex, j])
                                    {
                                        minRight = j;
                                        break;
                                    }
                                }

                                if (minRight < toIndex)
                                    intersectionSeverity += toIndex - minRight;
                            }

                            if (intersectionSeverity < bestIntersectionSeverity)
                            {
                                bestIntersectionSeverity = intersectionSeverity;
                                bestFromIndex = fromIndex;
                            }
                        }

                        connectionMatrix[bestFromIndex, toIndex] = true;
                    }
                }
            }
        }

        private static void ConnectChildlessParents(List<LevelNode> fromTier, List<LevelNode> toTier, bool[,] connectionMatrix)
        {
            for (int i = 0; i < fromTier.Count; i++)
            {
                fromTier[i].connections.Clear();
                for (int j = 0; j < toTier.Count; j++)
                {
                    if (connectionMatrix[i, j])
                        fromTier[i].connections.Add(toTier[j]);
                }

                fromTier[i].connections.Sort((a, b) => a.positionInTier.CompareTo(b.positionInTier));
            }
        }

        private static bool WouldCreateIntersection(bool[,] connectionMatrix, List<LevelNode> fromTier, List<LevelNode> toTier, int fromIndex, int toIndex)
		{
			for (int i = 0; i < fromIndex; i++)
			{
				int maxConnectionForI = -1;
				for (int j = toTier.Count - 1; j >= 0; j--)
				{
					if (connectionMatrix[i, j])
					{
						maxConnectionForI = j;
						break;
					}
				}
				
				if (maxConnectionForI > toIndex)
					return true;
			}
			
			for (int i = fromIndex + 1; i < fromTier.Count; i++)
			{
				int minConnectionForI = toTier.Count;
				for (int j = 0; j < toTier.Count; j++)
				{
					if (connectionMatrix[i, j])
					{
						minConnectionForI = j;
						break;
					}
				}
				
				if (minConnectionForI < toIndex)
					return true;
			}
			
			return false;
		}

		private static void ValidateAllConnections(List<List<LevelNode>> levelTree)
		{
			for (int tier = 0; tier < levelTree.Count; tier++)
			{
				List<LevelNode> tierNodes = levelTree[tier];
				
				if (tier < levelTree.Count - 1)
				{
					foreach (LevelNode node in tierNodes)
					{
						if (node.connections.Count == 0)
							throw new InvalidOperationException($"VALIDATION ERROR: Node at Tier {tier}, Position {node.positionInTier} has NO forward connections!");
					}

                    List<LevelNode> nextTier = levelTree[tier + 1];
					foreach (LevelNode nextNode in nextTier)
					{
						if (nextNode.connections.Count == 0 && tier == levelTree.Count - 2)
							continue;
						
						bool hasIncoming = false;
						foreach (LevelNode prevNode in tierNodes)
						{
							if (prevNode.connections.Contains(nextNode))
							{
								hasIncoming = true;
								break;
							}
						}
						
						if (!hasIncoming)
							throw new InvalidOperationException($"VALIDATION ERROR: Node at Tier {tier + 1}, Position {nextNode.positionInTier} has NO incoming connections!");
					}
					
					for (int i = 0; i < tierNodes.Count - 1; i++)
					{
						for (int j = i + 1; j < tierNodes.Count; j++)
						{
							LevelNode nodeA = tierNodes[i];
							LevelNode nodeB = tierNodes[j];
							
							if (nodeA.connections.Count > 0 && nodeB.connections.Count > 0)
							{
								int aMin = nodeA.connections[0].positionInTier;
								int aMax = nodeA.connections[nodeA.connections.Count - 1].positionInTier;
								int bMin = nodeB.connections[0].positionInTier;
								int bMax = nodeB.connections[nodeB.connections.Count - 1].positionInTier;
								
								if (aMin < bMin && aMax > bMin)
									throw new InvalidOperationException($"VALIDATION ERROR: Intersection between node {i} (range {aMin}-{aMax}) and node {j} (range {bMin}-{bMax}) in tier {tier}");
								else if (bMin < aMin && bMax > aMin)
									throw new InvalidOperationException($"VALIDATION ERROR: Intersection between node {i} (range {aMin}-{aMax}) and node {j} (range {bMin}-{bMax}) in tier {tier}");
							}
						}
					}
				}
			}
		}
    }
}