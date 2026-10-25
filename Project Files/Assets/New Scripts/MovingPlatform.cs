using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement")]
    public float speed = 2f;
    public bool startAtPointA = true;

    Rigidbody2D rb;

    Vector2 positionA;
    Vector2 positionB;

    float progress;
    int direction = 1;
    float distance;

    Vector2 lastPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (pointA == null || pointB == null)
        {
            Debug.LogError("MovingPlatform: Assign Point A and Point B.");
            enabled = false;
            return;
        }

        // Save fixed world positions.
        positionA = pointA.position;
        positionB = pointB.position;

        distance = Vector2.Distance(positionA, positionB);

        if (distance < 0.01f)
            distance = 1f;

        if (startAtPointA)
        {
            progress = 0f;
            direction = 1;
        }
        else
        {
            progress = 1f;
            direction = -1;
        }

        Vector2 startPosition = Vector2.Lerp(
            positionA,
            positionB,
            progress
        );

        rb.position = startPosition;
        lastPosition = startPosition;
    }

    void FixedUpdate()
    {
        Vector2 oldPosition = rb.position;

        float step =
            (Mathf.Abs(speed) / distance) *
            Time.fixedDeltaTime;

        progress += step * direction;

        if (progress >= 1f)
        {
            progress = 1f;
            direction = -1;
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            direction = 1;
        }

        Vector2 newPosition = Vector2.Lerp(
            positionA,
            positionB,
            progress
        );

        rb.MovePosition(newPosition);

        Vector2 platformMovement = newPosition - oldPosition;

        // Move actors standing on the platform with it.
        MoveActorsOnPlatform(platformMovement);

        lastPosition = newPosition;
    }

    void MoveActorsOnPlatform(Vector2 movement)
    {
        Collider2D platformCollider = GetComponent<Collider2D>();

        if (platformCollider == null)
            return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            platformCollider.bounds.center,
            platformCollider.bounds.size,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Actor2D actor = hit.GetComponent<Actor2D>();

            if (actor == null)
                continue;

            // Only move the actor if they are standing above
            // the platform.
            Bounds actorBounds = actor.BodyBounds;
            Bounds platformBounds = platformCollider.bounds;

            bool standingOnPlatform =
                actorBounds.min.y >= platformBounds.max.y - 0.15f &&
                actorBounds.min.y <= platformBounds.max.y + 0.2f;

            if (standingOnPlatform)
            {
                actor.transform.position += (Vector3)movement;
            }
        }
    }
}