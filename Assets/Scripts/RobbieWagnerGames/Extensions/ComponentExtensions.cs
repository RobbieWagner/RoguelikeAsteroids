using UnityEngine;

namespace RobbieWagnerGames.UnityExtensions
{
    /// <summary>
    /// Provides extension methods for Unity's Component class
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Adds component of type T
        /// </summary>
        public static T AddComponent<T>(this Component component) where T : Component
        {
            return component != null ? component.gameObject.AddComponent<T>() : null;
        }

        /// <summary>
        /// Gets existing component or adds new one
        /// </summary>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (component == null) return null;
            
            T existing = component.GetComponent<T>();
            return existing != null ? existing : component.AddComponent<T>();
        }

        /// <summary>
        /// Gets component in parent or children
        /// </summary>
        public static T GetComponentInHierarchy<T>(this Component component, bool includeInactive = false) where T : Component
        {
            if (component == null) return null;
            
            T comp = component.GetComponentInParent<T>();
            if (comp == null)
            {
                comp = component.GetComponentInChildren<T>(includeInactive);
            }
            return comp;
        }
    }
}