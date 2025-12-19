namespace RobbieWagnerGames.Utilities
{
    public class MultiSceneSingleton<T> : MonoBehaviourSingleton<T> where T : MonoBehaviourSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
