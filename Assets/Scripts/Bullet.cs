using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public IEnumerator Fire(Vector2 direction, float speed, float time)
    {
        Vector2 finalPos = (Vector2) transform.position + (direction * speed * time);

        yield return transform.DOMove(finalPos, time).WaitForCompletion();

        Destroy(this.gameObject);
    }
}