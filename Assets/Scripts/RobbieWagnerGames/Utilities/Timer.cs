using System;
using UnityEngine;

namespace RobbieWagnerGames.Utilities
{
    public class Timer : MonoBehaviour
    {
        public bool isRunning {get; private set;}
        public float duration {get; private set;}
        public float timerValue {get; private set;}
        private bool stopAtDuration = true;

        public event Action<float> OnTimerUpdate;
        public event Action OnTimerComplete;

        private void Awake()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (isRunning)
            {
                timerValue += Time.deltaTime;
                OnTimerUpdate?.Invoke(timerValue);

                if (duration <= timerValue)
                {
                    OnTimerComplete?.Invoke();
                    if (stopAtDuration)
                        isRunning = false;
                }
            }
        }

        public void StartTimer(float duration = -1, bool stopAtDuration = true)
        {
            if (duration > 0)
                this.duration = duration;

            timerValue = 0;
            OnTimerUpdate?.Invoke(timerValue);

            isRunning = true;

            this.stopAtDuration = stopAtDuration;
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        public void ResumeTimer()
        {
            isRunning = true;
        }
    }
}