using UnityEngine;
using UnityEngine.AI;

namespace RobbieWagnerGames.AI
{
    public static class NavMeshExtensions
    {
        public static Vector3 GetRandomNavMeshPositionOnCircle(this Vector3 sourcePosition, float radius, float navMeshSampleDistance)
        {
            float randomAngle = Random.Range(0f, 360f);

            // Calculate the point on the circle
            float x = sourcePosition.x + radius * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float z = sourcePosition.z + radius * Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 randomPointOnCircle = new Vector3(x, sourcePosition.y, z);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPointOnCircle, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                return hit.position;

            return Vector3.zero;
        }
    }
}