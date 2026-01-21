using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using System.Linq;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public enum BossState
    {
        NONE = -1,
        INTRO_1,
        INTRO_2,
        INTRO_3,
        IDLE,
        MOVE_MODE_1,
        MOVE_MODE_2,
        ATTACK_1,
        ATTACK_2,
        ATTACK_3,
        OUTRO,
        DEFEATED
    }

    [Serializable]
    public class StatePattern
    {
        public BossMovementPattern movementPattern = null;
        public BossAttackPattern attackPattern = null;
    }

    [Serializable]
    public class BossPhaseState
    {
        public BossState state;
        public float duration;
    }

    public class LevelBoss : Shootable
    {
        [Header("Boss Configuration")]
        [SerializeField] private List<BossPhaseState> introPhase;
        [SerializedDictionary("healthThreshold", "statesToActivate")][SerializeField] 
        private SerializedDictionary<float, List<BossPhaseState>> bossPhases = new SerializedDictionary<float, List<BossPhaseState>>();
        [SerializeField] private List<BossPhaseState> outroPhase;
        
        [Header("State Patterns Dictionary")]
        [SerializedDictionary("state", "patterns")]
        [SerializeField] 
        private SerializedDictionary<BossState, StatePattern> patterns = new SerializedDictionary<BossState, StatePattern>();
        
        [Header("State Machine")]
        private BossState currentState;
        private Coroutine currentPhaseRoutine;
        private int startingDurability;
        private bool isDefeated = false;
        
        private float currentPhaseThreshold = -1f;
        private float queuedPhaseThreshold = -1f;
        
        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem damageParticles;

        public event Action OnBossDamaged;
        public event Action OnBossDefeated;
        public event Action<BossState> OnStateChanged;
        public event Action OnIntroPhaseComplete;
        public event Action OnOutroPhaseComplete;
        
        protected override void Awake()
        {
            base.Awake();
            startingDurability = durability;
            InitializeBoss();
        }
        
        private void InitializeBoss()
        {
            ChangeState(BossState.IDLE);
        }

        public void StartBoss()
        {
            StartIntroPhase();
        }
        
        private void StartIntroPhase()
        {
            if (currentPhaseRoutine != null)
                StopCoroutine(currentPhaseRoutine);
                
            currentPhaseRoutine = StartCoroutine(ExecuteIntroPhase());
        }
        
        private void StartOutroPhase()
        {
            queuedPhaseThreshold = -1f;
            
            if (currentPhaseRoutine != null)
                StopCoroutine(currentPhaseRoutine);
                
            currentPhaseRoutine = StartCoroutine(ExecuteOutroPhase());
        }
        
        private void StartBossPhase(float healthThreshold)
        {
            if (!bossPhases.ContainsKey(healthThreshold) || isDefeated) return;
            
            if (Mathf.Approximately(currentPhaseThreshold, healthThreshold))
                return;
            
            if (currentPhaseRoutine != null)
                StopCoroutine(currentPhaseRoutine);
            
            currentPhaseThreshold = healthThreshold;
            currentPhaseRoutine = StartCoroutine(ExecuteBossPhase(bossPhases[healthThreshold]));
        }
        
        private void QueueBossPhaseTransition(float healthThreshold)
        {
            if (!bossPhases.ContainsKey(healthThreshold) || isDefeated) return;
            
            if (Mathf.Approximately(currentPhaseThreshold, healthThreshold) || Mathf.Approximately(queuedPhaseThreshold, healthThreshold))
                return;
            
            queuedPhaseThreshold = healthThreshold;
        }
        
        private IEnumerator ExecuteIntroPhase()
        {
            yield return ExecutePhaseSequence(introPhase);
            OnIntroPhaseComplete?.Invoke();
        }
        
        private IEnumerator ExecuteOutroPhase()
        {
            OnBossDefeated?.Invoke();
            ChangeState(BossState.DEFEATED);
            yield return ExecutePhaseSequence(outroPhase);
            OnOutroPhaseComplete?.Invoke();
        }
        
        private IEnumerator ExecuteBossPhase(List<BossPhaseState> phase)
        {
            while (!isDefeated)
                yield return ExecutePhaseSequence(phase);
        }
        
        private IEnumerator ExecutePhaseSequence(List<BossPhaseState> phase)
        {
            foreach (BossPhaseState phaseState in phase)
            {
                if (queuedPhaseThreshold >= 0)
                    yield break;
                    
                ChangeState(phaseState.state);
                yield return ExecuteStatePattern(phaseState.state, phaseState.duration);
            }
        }
        
        private IEnumerator ExecuteStatePattern(BossState state, float duration)
        {
            if (!patterns.ContainsKey(state) || patterns[state] == null)
            {
                Debug.LogWarning($"No pattern defined for state: {state}");
                yield break;
            }
            
            StatePattern pattern = patterns[state];
            BossMovementPattern movePattern = pattern.movementPattern;
            BossAttackPattern attackPattern = pattern.attackPattern;
            
            yield return ExecutePatternPair(movePattern, attackPattern, duration);
        }
        
        private IEnumerator ExecutePatternPair(BossMovementPattern movePattern, BossAttackPattern attackPattern, float duration)
        {
            if (movePattern == null && attackPattern == null)
                yield break;
            
            List<Coroutine> runningCoroutines = new List<Coroutine>();
            bool movePatternCompleted = false;
            bool attackPatternCompleted = false;
            
            if (movePattern != null)
            {
                Coroutine moveCoroutine = StartCoroutine(ExecuteSinglePattern(movePattern, duration, () => movePatternCompleted = true));
                runningCoroutines.Add(moveCoroutine);
            }
            else
                movePatternCompleted = true;
            
            if (attackPattern != null)
            {
                Coroutine attackCoroutine = StartCoroutine(ExecuteSinglePattern(attackPattern, duration, () => attackPatternCompleted = true));
                runningCoroutines.Add(attackCoroutine);
            }
            else
                attackPatternCompleted = true;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration && !movePatternCompleted && !attackPatternCompleted)
            {
                if (queuedPhaseThreshold >= 0)
                    break;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            if (queuedPhaseThreshold < 0)
                yield return new WaitUntil(() => movePatternCompleted && attackPatternCompleted);
            
            foreach (Coroutine coroutine in runningCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
        }
        
        private IEnumerator ExecuteSinglePattern(BossPattern pattern, float duration, Action onComplete = null)
        {
            if (pattern == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            bool patternCompleted = false;
            
            yield return pattern.ExecutePattern(this, duration, () => {
                patternCompleted = true;
                onComplete?.Invoke();
            });
            
            float elapsedTime = 0f;
            while (elapsedTime < duration && !patternCompleted)
            {
                if (queuedPhaseThreshold >= 0)
                    break;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        private void ChangeState(BossState newState)
        {
            if (currentState == newState || isDefeated) return;
            
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            
            animator.SetInteger("BossState", (int)newState);
        }

        public override void DecreaseDurability(int damage = 1)
        {
            if (isDefeated) return;
            
            durability -= damage;
            
            StartCoroutine(FlashDamage());
            damageParticles.Play();
            
            OnBossDamaged?.Invoke();

            if (durability <= 0 && !isDefeated)
            {
                DestroyBoss();
                return;
            }
            
            CheckHealthThresholdTransitions();
        }
        
        private IEnumerator FlashDamage()
        {
            Color originalColor = spriteRenderer.color;
            Color oppositeColor = new Color(1f - originalColor.r,1f - originalColor.g,1f - originalColor.b,originalColor.a);
            
            spriteRenderer.color = oppositeColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        
        public void CheckHealthThresholdTransitions()
        {
            if (isDefeated) return;
            
            float healthPercentage = (float)durability / startingDurability;
            
            List<float> sortedThresholds = bossPhases.Keys.OrderBy(k => k).ToList();
            
            float targetThreshold = -1f;
            foreach (float threshold in sortedThresholds)
            {
                if (healthPercentage <= threshold)
                {
                    targetThreshold = threshold;
                    break; 
                }
            }
            
            if (targetThreshold >= 0)
            {
                if (currentPhaseRoutine != null && currentPhaseThreshold >= 0)
                    QueueBossPhaseTransition(targetThreshold);
                else
                    StartBossPhase(targetThreshold);
            }
        }
        
        private void DestroyBoss()
        {
            isDefeated = true;
            StartOutroPhase();
        }
        
        protected override void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}