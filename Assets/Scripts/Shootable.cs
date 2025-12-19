using RobbieWagnerGames.Audio;
using RobbieWagnerGames.RoguelikeAsteroids;
using UnityEngine;

public class Shootable : MonoBehaviour
{
    private float currentSpeed;
    private Vector2 currentDirection;
    [SerializeField] private Rigidbody2D rb2d;

    public Vector2 xBounds;
    public Vector2 yBounds;

    [SerializeField] private Collider2D shootableCollider;

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("bullet"))
        {
            BasicAudioManager.Instance.Play(AudioSourceName.ShootableDestroyed);

            Destroy(gameObject);
        }
    } 

    public void SetMovement(float speed, Vector2 direction)
    {
        currentSpeed = speed;
        currentDirection = direction;

        rb2d.linearVelocity = currentSpeed * currentDirection;
    }

    private void Update() 
    {
        Vector2 pos = transform.position;
        if(pos.x < xBounds.x || pos.x > xBounds.y || pos.y < yBounds.x || pos.y > yBounds.y)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        AsteroidManager.Instance.OnDestroyAsteroid(this);
    }
}
