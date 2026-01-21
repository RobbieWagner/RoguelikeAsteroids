using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class BossLevelHUD : PlayerHUD
    {
        [SerializeField] private Slider bossHealthSlider;
        [SerializeField] private LevelBoss boss;

        protected override void Awake()
        {
            base.Awake();
            InitializeBossHealthSlider();
            boss.OnBossDamaged += UpdateHealthSlider;
        }

        private void InitializeBossHealthSlider()
        {
            bossHealthSlider.gameObject.SetActive(true);
            
            boss.OnIntroPhaseComplete += OnBossIntroComplete;
            boss.OnBossDefeated += OnBossDefeated;
            
            bossHealthSlider.minValue = 0;
            bossHealthSlider.maxValue = boss.durability;
            bossHealthSlider.value = boss.durability;
            
            bossHealthSlider.gameObject.SetActive(false);
        }

        private void UpdateHealthSlider()
        {
            bossHealthSlider.value = boss.durability;
        }

        private void OnBossIntroComplete()
        {
            bossHealthSlider.gameObject.SetActive(true);
        }

        private void OnBossDefeated()
        {
            bossHealthSlider.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            boss.OnIntroPhaseComplete -= OnBossIntroComplete;
            boss.OnBossDefeated -= OnBossDefeated;
        }
        
    }
}