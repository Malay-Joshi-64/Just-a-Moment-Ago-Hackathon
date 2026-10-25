using UnityEngine;

// Player can clear the level only when the door is open.
public class Goal : MonoBehaviour
{
    [Header("Door Requirement")]
    public Door door;

    ContactFilter2D filter;
    Collider2D col;

    readonly Collider2D[] hits = new Collider2D[8];

    void Awake()
    {
        col = GetComponent<Collider2D>();

        filter = new ContactFilter2D
        {
            useTriggers = false
        };
    }

    void FixedUpdate()
    {
        var gm = GameManager.I;

        if (gm == null)
            return;

        // Only the Shadow phase can be won.
        if (gm.phase != Phase.Shadow)
            return;

        // Door MUST be open.
        if (door == null || !door.IsOpen)
            return;

        Vector2 center = col != null
            ? (Vector2)col.bounds.center
            : (Vector2)transform.position;

        Vector2 size = col != null
            ? (Vector2)col.bounds.size
            : (Vector2)transform.localScale;

        int n = Physics2D.OverlapBox(
            center,
            size,
            0f,
            filter,
            hits
        );

        for (int i = 0; i < n; i++)
        {
            if (hits[i].GetComponent<PlayerController>() != null)
            {
                gm.Win();
                return;
            }
        }
    }
}