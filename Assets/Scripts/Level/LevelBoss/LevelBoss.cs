using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public enum BossState
	{
		NONE = -1,
		IDLE,
		MOVE_MODE_1,
		MOVE_MODE_2,
		ATTACK_1,
		ATTACK_2,
		ATTACK_3
	}

	[Serializable]
    public class BossPhase
    {
        public string phaseName;
        public BossState[] stateSequence;
        public float[] stateDurations;
        public float healthThreshold = 0.5f;
    }

    public class LevelBoss : Shootable
    {
        [Header("Boss Configuration")]
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private BossPhase[] bossPhases;
        
        [Header("Patterns")]
        [SerializeField] private BossMovementPattern idlePattern;
        [SerializeField] private BossMovementPattern[] movementPatterns;
        [SerializeField] private BossAttackPattern[] attackPatterns;
        
        [Header("State Machine")]
        private BossState currentState;
        private Coroutine currentStateRoutine;
        private int currentPhaseIndex = 0;
        private int currentHealth;
        
        [Header("Visuals")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem damageParticles;
        
        public event Action OnBossDestroyed;
        public event Action<BossState> OnStateChanged;
        
        protected override void Awake()
        {
			base.Awake();
            currentHealth = maxHealth;
            InitializeBoss();
        }
        
        private void InitializeBoss()
        {
            ChangeState(BossState.IDLE);
            StartPhase(0);
        }
        
        private void StartPhase(int phaseIndex)
        {
            if (phaseIndex >= bossPhases.Length) return;
            
            currentPhaseIndex = phaseIndex;
            var phase = bossPhases[phaseIndex];
            
            StartCoroutine(ExecutePhaseSequence(phase));
        }
        
        private IEnumerator ExecutePhaseSequence(BossPhase phase)
        {
            for (int i = 0; i < phase.stateSequence.Length; i++)
            {
                ChangeState(phase.stateSequence[i]);
                
                yield return ExecuteStatePattern(phase.stateSequence[i]);
                
                if (i < phase.stateDurations.Length && phase.stateDurations[i] > 0)
                    yield return new WaitForSeconds(phase.stateDurations[i]);
            }
            
            StartPhase(currentPhaseIndex);
        }
        
        private IEnumerator ExecuteStatePattern(BossState state)
        {
            BossPattern pattern = null;
            
            switch (state)
            {
                case BossState.IDLE:
                    pattern = idlePattern;
                    break;
                case BossState.MOVE_MODE_1:
                    pattern = movementPatterns[0];
                    break;
                case BossState.MOVE_MODE_2:
                    pattern = movementPatterns.Length > 1 ? movementPatterns[1] : movementPatterns[0];
                    break;
                case BossState.ATTACK_1:
                    pattern = attackPatterns[0];
                    break;
                case BossState.ATTACK_2:
                    pattern = attackPatterns.Length > 1 ? attackPatterns[1] : attackPatterns[0];
                    break;
                case BossState.ATTACK_3:
                    pattern = attackPatterns.Length > 2 ? attackPatterns[2] : attackPatterns[0];
                    break;
            }
            
            if (pattern != null)
                yield return pattern.ExecutePattern(this);
        }
        
        private void ChangeState(BossState newState)
        {
            if (currentState == newState) return;
            
			StopCoroutine(currentStateRoutine);
            
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            
			animator.SetInteger("BossState", (int)newState);
            
            Debug.Log($"Boss state changed to: {newState}");
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            
			StartCoroutine(FlashDamage());
			damageParticles.Play();
            
            CheckPhaseTransition();

            if (currentHealth <= 0)
                DestroyBoss();
        }
        
        private IEnumerator FlashDamage()
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
        
        private void CheckPhaseTransition()
        {
            float healthPercentage = (float) currentHealth / maxHealth;
            
            for (int i = bossPhases.Length - 1; i >= 0; i--)
            {
                if (healthPercentage <= bossPhases[i].healthThreshold && currentPhaseIndex < i)
                {
                    StartPhase(i);
                    break;
                }
            }
        }
        
        private void DestroyBoss()
        {
            animator.SetTrigger("Destroy");
            OnBossDestroyed?.Invoke();
            Destroy(gameObject, 2f);
        }
        
        protected override void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}