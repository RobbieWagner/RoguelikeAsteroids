using DG.Tweening;
using RobbieWagnerGames.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class ScreenCover : MonoBehaviourSingleton<ScreenCover>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image screenCover;

        public void ToggleScreenCover(bool on)
        {
            if(on)
            {
                canvas.enabled = true;
                screenCover.color = Color.black;
            }
            else
            {
                canvas.enabled = false;
            }
        }

        public IEnumerator FadeCoverIn(float time = 1f)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = false;
                screenCover.color = Color.clear;
                canvas.enabled = true;
                yield return screenCover.DOColor(Color.black, time).SetEase(Ease.Linear).WaitForCompletion();
            }
        }

        public IEnumerator FadeCoverOut(float time = 1f)
        {
            if (canvas.enabled)
            {
                screenCover.color = Color.black;
                canvas.enabled = true;
                yield return screenCover.DOColor(Color.clear, time).SetEase(Ease.Linear).WaitForCompletion();
                canvas.enabled = false;
            }
        }
    }
}