using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using RobbieWagnerGames.Utilities;

namespace RobbieWagnerGames.AI
{
    public class AIManager : MonoBehaviourSingleton<AIManager>
    {
        [Header("Agent Management")]
        [SerializeField] private bool autoFreezeOnSceneLoad = true;
        
        private readonly List<AIAgent> activeAgents = new List<AIAgent>();
        public IReadOnlyList<AIAgent> ActiveAgents => activeAgents;

        protected override void Awake()
        {
            base.Awake();
            
            if (autoFreezeOnSceneLoad)
            {
                FreezeAllAgents();
            }
        }

        #region Agent Management
        /// <summary>
        /// Instantiates a new AI agent in the scene
        /// </summary>
        /// <param name="agentPrefab">Prefab to instantiate</param>
        /// <param name="startingPos">Initial position</param>
        /// <param name="initialTargets">Initial targets for the agent</param>
        /// <returns>The created agent instance</returns>
        public AIAgent CreateAgent(AIAgent agentPrefab, Vector3 startingPos, List<AITarget> initialTargets = null)
        {
            if (agentPrefab == null)
            {
                Debug.LogError("Cannot create agent from null prefab!");
                return null;
            }

            AIAgent agent = Instantiate(agentPrefab, startingPos, Quaternion.identity);
            RegisterAgent(agent, initialTargets);
            return agent;
        }

        /// <summary>
        /// Registers an existing agent with the manager
        /// </summary>
        public void RegisterAgent(AIAgent agent, List<AITarget> initialTargets = null)
        {
            if (agent == null) return;

            if (!activeAgents.Contains(agent))
            {
                activeAgents.Add(agent);
            }

            if (initialTargets != null && initialTargets.Any())
            {
                agent.SetTargets(initialTargets);
            }
        }

        /// <summary>
        /// Removes and destroys an agent
        /// </summary>
        public void DestroyAgent(AIAgent agent)
        {
            if (agent == null) return;

            if (activeAgents.Contains(agent))
            {
                activeAgents.Remove(agent);
            }

            Destroy(agent.gameObject);
        }

        /// <summary>
        /// Removes an agent from management without destroying it
        /// </summary>
        public void UnregisterAgent(AIAgent agent)
        {
            if (agent != null)
            {
                activeAgents.Remove(agent);
            }
        }
        #endregion

        #region Agent Control
        /// <summary>
        /// Commands all agents to go idle
        /// </summary>
        public void FreezeAllAgents()
        {
            foreach (AIAgent agent in activeAgents.Where(a => a != null))
            {
                agent.GoIdle();
            }
        }

        /// <summary>
        /// Resumes normal behavior for all agents
        /// </summary>
        public void ResumeAllAgents()
        {
            foreach (AIAgent agent in activeAgents.Where(a => a != null))
            {
                agent.MoveToRandomSpot();
            }
        }
        #endregion

        #region Path Utilities
        /// <summary>
        /// Calculates the length of a NavMesh path
        /// </summary>
        public static float CalculatePathLength(NavMeshPath path)
        {
            if (path == null || path.corners.Length < 2)
                return 0f;

            float length = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return length;
        }
        #endregion
    }
}