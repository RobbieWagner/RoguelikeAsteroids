using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    public void Fire(Vector2 direction, float speed, float time)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Vector2 finalPos = (Vector2)transform.position + (direction * speed * time);

        Sequence shootSequence = DOTween.Sequence();
        shootSequence.Append(sprite.DOColor(Color.clear, time).SetEase(Ease.InCubic));
        shootSequence.Join(transform.DOMove(finalPos, time).SetEase(Ease.Linear));
        shootSequence.AppendCallback(() => {Destroy(gameObject);});
    }
}