using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
	public class LevelButton : MonoBehaviour
	{
		[field:SerializeField] public Button button {get; private set;}
		public Level level {get; set;}
		public LevelNode levelNode {get; set;}

        private void Awake()
        {
            button.onClick.AddListener(SelectLevel);
        }

        private void SelectLevel()
        {
            RunManager.Instance.CurrentRun.currentNode = levelNode;
			StartCoroutine(RunManager.Instance.StartCurrentLevelCo());
			DisableButton();
        }

        public void EnableButton()
		{
			button.enabled = true;
		}

		public void DisableButton()
		{
			button.enabled = false;
		}
	}
}