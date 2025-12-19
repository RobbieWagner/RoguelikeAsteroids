using System;
using UnityEngine;

namespace RobbieWagnerGames.Utilities
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        public static event Action<MonoBehaviourSingleton<T>> OnInstanceSet;
        private static T instance;
        public static T Instance
        {
            get 
            { 
                return instance; 
            }
            protected set
            {
                if (instance == value)
                    return;
                instance = value;
                OnInstanceSet?.Invoke(instance);
            }
        }

        public static bool hasInstance => instance != null;

        protected virtual void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            else
            {
                instance = (T) this;
                OnInstanceSet?.Invoke(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
