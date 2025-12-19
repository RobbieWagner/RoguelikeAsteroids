using UnityEngine;

namespace RobbieWagnerGames
{
    [CreateAssetMenu(fileName = "CombatSoundEffects", menuName = "Audio/Combat Sound Effects")]
    public class CombatSoundEffects : ScriptableObject
    {
        [Header("Action Sounds")]
        [SerializeField] private AudioClip _damageSound;
        [SerializeField] private AudioClip _healingSound;
        [SerializeField] private AudioClip _missSound;
        
        [Header("Stat Modification Sounds")]
        [SerializeField] private AudioClip _statUpSound;
        [SerializeField] private AudioClip _statDownSound;
        
        [Header("Status Sounds")]
        [SerializeField] private AudioClip _conditionSound;
        
        [Header("UI Sounds")]
        [SerializeField] private AudioClip _actionSelectionSound;
        [SerializeField] private AudioClip _combatMenuNavSound;

        public AudioClip DamageSound => _damageSound;
        public AudioClip HealingSound => _healingSound;
        public AudioClip MissSound => _missSound;
        public AudioClip StatUpSound => _statUpSound;
        public AudioClip StatDownSound => _statDownSound;
        public AudioClip ConditionSound => _conditionSound;
        public AudioClip ActionSelectionSound => _actionSelectionSound;
        public AudioClip CombatMenuNavSound => _combatMenuNavSound;
    }
}