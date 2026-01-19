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
        [SerializeField] private int maxHealth = 30;
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
        private int currentHealth;
        private bool isDefeated = false;
        
        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem damageParticles;
        
        public event Action OnBossDestroyed;
        public event Action<BossState> OnStateChanged;
        public event Action OnIntroPhaseComplete;
        public event Action OnOutroPhaseComplete;
        
        protected override void Awake()
        {
            base.Awake();
            currentHealth = maxHealth;
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
            if (currentPhaseRoutine != null)
                StopCoroutine(currentPhaseRoutine);
                
            currentPhaseRoutine = StartCoroutine(ExecuteOutroPhase());
        }
        
        private void StartBossPhase(float healthThreshold)
        {
            if (!bossPhases.ContainsKey(healthThreshold) || isDefeated) return;
            
            if (currentPhaseRoutine != null)
                StopCoroutine(currentPhaseRoutine);
                
            currentPhaseRoutine = StartCoroutine(ExecuteBossPhase(bossPhases[healthThreshold]));
        }
        
        private IEnumerator ExecuteIntroPhase()
        {
            Debug.Log("Starting intro phase execution");
            yield return ExecutePhaseSequence(introPhase);
            Debug.Log("Intro phase sequence complete, invoking callback");
            OnIntroPhaseComplete?.Invoke();
        }
        
        private IEnumerator ExecuteOutroPhase()
        {
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
                ChangeState(phaseState.state);
                
                yield return ExecuteStatePattern(phaseState.state);
                
                if (phaseState.duration > 0)
                    yield return new WaitForSeconds(phaseState.duration);
            }
        }
        
        private IEnumerator ExecuteStatePattern(BossState state)
        {
            if (!patterns.ContainsKey(state) || patterns[state] == null)
            {
                Debug.LogWarning($"No pattern defined for state: {state}");
                yield break;
            }
            
            StatePattern pattern = patterns[state];
            BossMovementPattern movePattern = pattern.movementPattern;
            BossAttackPattern attackPattern = pattern.attackPattern;
            
            yield return ExecutePatternPair(movePattern, attackPattern);
        }
        
        private IEnumerator ExecutePatternPair(BossMovementPattern movePattern, BossAttackPattern attackPattern)
        {
            if (movePattern == null && attackPattern == null)
                yield break;
            
            List<Coroutine> runningCoroutines = new List<Coroutine>();
            bool movePatternCompleted = false;
            bool attackPatternCompleted = false;
            
            if (movePattern != null)
            {
                Debug.Log("move");
                Coroutine moveCoroutine = StartCoroutine(ExecuteSinglePattern(movePattern, () => movePatternCompleted = true));
                runningCoroutines.Add(moveCoroutine);
            }
            else
                movePatternCompleted = true;
            
            if (attackPattern != null)
            {
                Coroutine attackCoroutine = StartCoroutine(ExecuteSinglePattern(attackPattern, () => attackPatternCompleted = true));
                runningCoroutines.Add(attackCoroutine);
            }
            else
                attackPatternCompleted = true;
            
            yield return new WaitUntil(() => movePatternCompleted && attackPatternCompleted);
            
            foreach (Coroutine coroutine in runningCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
        }
        
        private IEnumerator ExecuteSinglePattern(BossPattern pattern, Action onComplete = null)
        {
            if (pattern == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            bool patternCompleted = false;
            
            yield return pattern.ExecutePattern(this, () => {
                patternCompleted = true;
                onComplete?.Invoke();
            });
            
            yield return new WaitUntil(() => patternCompleted);
        }
        
        private void ChangeState(BossState newState)
        {
            if (currentState == newState || isDefeated) return;
            
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            
            animator.SetInteger("BossState", (int)newState);
            
            Debug.Log($"Boss state changed to: {newState}");
        }
        
        public void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            currentHealth -= damage;
            
            StartCoroutine(FlashDamage());
            damageParticles.Play();
            
            CheckHealthThresholdTransitions();

            if (currentHealth <= 0 && !isDefeated)
                DestroyBoss();
        }
        
        private IEnumerator FlashDamage()
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
        
        private void CheckHealthThresholdTransitions()
        {
            if (isDefeated) return;
            
            float healthPercentage = (float) currentHealth / maxHealth;
            
            List<float> sortedThresholds = bossPhases.Keys.OrderByDescending(k => k).ToList();
            
            foreach (float threshold in sortedThresholds)
            {
                if (healthPercentage <= threshold && bossPhases.ContainsKey(threshold))
                    StartBossPhase(threshold);
            }
        }
        
        private void DestroyBoss()
        {
            Debug.Log($"DestroyBoss called. isDefeated: {isDefeated}, currentHealth: {currentHealth}");
            isDefeated = true;
            StartOutroPhase();
        }
        
        protected override void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}