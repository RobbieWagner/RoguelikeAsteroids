using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RobbieWagnerGames.AI
{
    public enum AIState
    {
        None = -1,
        Idle = 0,
        Moving = 1,
        Chasing = 2
    }

    /// <summary>
    /// Defines the base AI agent and helpful methods for pathfinding.
    /// Useful for simple AI agents or when controlling many agents.
    /// Can be inherited for custom behaviors.
    /// </summary>
    public class AIAgent : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] protected float idleWaitTime = 3f;
        [SerializeField] protected float movementRange = 100f;
        
        public NavMeshAgent Agent { get; protected set; }
        public AIState CurrentState { get; protected set; } = AIState.None;
        public AITarget ChasingTarget { get; protected set; }

        protected List<AITarget> currentTargets = new List<AITarget>();
        protected float currentWaitTime;

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            if (Agent == null)
            {
                Debug.LogError("NavMeshAgent component is missing!", this);
            }
        }

        protected virtual void Update()
        {
            UpdateState();
        }

        #region State Management
        protected virtual void UpdateState()
        {
            switch (CurrentState)
            {
                case AIState.Idle:
                    UpdateIdleState();
                    break;
                case AIState.Moving:
                    UpdateMovingState();
                    break;
                case AIState.Chasing:
                    UpdateChaseState();
                    break;
            }
        }

        protected virtual void ChangeState(AIState newState)
        {
            if (newState == CurrentState) return;
            CurrentState = newState;
            OnStateChanged(newState);
        }

        protected virtual void OnStateChanged(AIState newState)
        {
            // Can be overridden for custom state change behavior
        }
        #endregion

        #region State Behaviors
        public virtual void GoIdle()
        {
            Agent.isStopped = true;
            ChangeState(AIState.Idle);
        }

        protected virtual void UpdateIdleState()
        {
            currentWaitTime += Time.deltaTime;
            if (currentWaitTime >= idleWaitTime)
            {
                currentWaitTime = 0;
                MoveToRandomSpot(movementRange);
            }
        }

        protected virtual void UpdateMovingState()
        {
            if (HasReachedDestination())
            {
                GoIdle();
            }
        }

        protected virtual void UpdateChaseState()
        {
            if (ChasingTarget == null)
            {
                GoIdle();
                return;
            }

            SetDestination(ChasingTarget.transform.position);

            if (HasReachedDestination())
            {
                OnReachTarget(ChasingTarget);
            }
        }
        #endregion

        #region Navigation
        public virtual bool SetDestination(Vector3 destination)
        {
            Agent.isStopped = false;
            return Agent.SetDestination(destination);
        }

        public virtual bool MoveAgent(Vector3 destination)
        {
            ChangeState(AIState.Moving);
            bool success = SetDestination(destination);

            if (!success)
            {
                GoIdle();
                Debug.LogWarning("Failed to move agent to destination");
            }

            return success;
        }

        public virtual void MoveToRandomSpot(float range = 100f)
        {
            StartCoroutine(MoveToRandomSpotCoroutine(transform.position, range));
        }

        protected virtual IEnumerator MoveToRandomSpotCoroutine(Vector3 center, float range, int maxAttempts = 100, int attemptsBeforeYield = 10)
        {
            int attempts = 0;
            bool success = false;
            
            while (attempts < maxAttempts && !success)
            {
                attempts++;
                
                if (attempts % attemptsBeforeYield == 0)
                    yield return null;
                
                Vector3 randomDirection = Random.insideUnitSphere * range;
                randomDirection += center;
                
                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, range, NavMesh.AllAreas))
                {
                    success = MoveAgent(hit.position);
                }
            }
            
            if (!success)
            {
                Debug.LogWarning($"Failed to find valid navigation position after {maxAttempts} attempts");
            }
        }

        protected virtual bool HasReachedDestination()
        {
            return !Agent.pathPending 
                   && Agent.remainingDistance <= Agent.stoppingDistance 
                   && (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f);
        }
        #endregion

        #region Target Handling
        public virtual bool ChaseNearestTarget()
        {
            if (currentTargets == null || currentTargets.Count == 0)
            {
                Debug.LogWarning("No targets available to chase");
                GoIdle();
                return false;
            }

            AITarget closestTarget = FindClosestReachableTarget();
            
            if (closestTarget != null)
            {
                ChangeState(AIState.Chasing);
                ChasingTarget = closestTarget;
                return true;
            }

            return false;
        }

        protected virtual AITarget FindClosestReachableTarget()
        {
            AITarget closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (AITarget target in currentTargets.Where(t => t != null))
            {
                NavMeshPath path = new NavMeshPath();
                if (Agent.CalculatePath(target.transform.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = AIManager.CalculatePathLength(path);

                    if (pathLength < closestDistance)
                    {
                        closestDistance = pathLength;
                        closestTarget = target;
                    }
                }
            }

            return closestTarget;
        }

        public virtual void SetTargets(List<AITarget> targets, bool clearExisting = false, bool chaseNearest = true)
        {
            if (clearExisting)
            {
                currentTargets.Clear();
            }
            
            currentTargets.AddRange(targets.Where(t => t != null));

            if (chaseNearest)
            {
                ChaseNearestTarget();
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != AIState.Chasing) return;

            AITarget target = collision.gameObject.GetComponent<AITarget>();
            if (target != null && ChasingTarget == target)
            {
                OnReachTarget(ChasingTarget);
            }
        }

        protected virtual void OnReachTarget(AITarget target)
        {
            target?.OnCaught(this);
            currentTargets.Remove(target);
            ChaseNearestTarget();
        }
        #endregion
    }
}