using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.AI
{
    /// <summary>
    /// Test script for demonstrating AI agent functionality
    /// </summary>
    public class AITest : MonoBehaviour
    {
        [Header("Agent Settings")]
        [SerializeField] private AIAgent testAgentPrefab = null;
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        [SerializeField] private Vector3 initialMovePosition = new Vector3(3, 1, 3);

        [Header("Target Settings")]
        [SerializeField] private List<AITarget> agentTargets = new List<AITarget>();
        [SerializeField] private float initialDelay = 1f;
        [SerializeField] private float testDuration = 30f;

        private AIAgent testAgent;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(initialDelay);
            yield return StartCoroutine(RunTestSequence());
        }

        private IEnumerator RunTestSequence()
        {
            // Create and initialize agent
            testAgent = AIManager.Instance.CreateAgent(testAgentPrefab, spawnPosition, agentTargets);
            
            if (testAgent == null)
            {
                Debug.LogError("Test failed - could not create agent");
                yield break;
            }

            // Initial movement test
            testAgent.MoveAgent(initialMovePosition);
            Debug.Log($"Agent spawned at {spawnPosition} and commanded to move to {initialMovePosition}");

            // Wait for test duration
            yield return new WaitForSeconds(testDuration);

            // Target behavior test
            if (agentTargets.Count > 0)
            {
                testAgent.SetTargets(agentTargets);
                Debug.Log($"Agent assigned {agentTargets.Count} targets");
            }
            else
            {
                Debug.LogWarning("No targets assigned for second test phase");
            }
        }

        private void OnDestroy()
        {
            if (testAgent != null)
            {
                AIManager.Instance.DestroyAgent(testAgent);
            }
        }
    }
}