using UnityEngine;

// Pressure plate: pressed while the Player OR the Shadow overlaps it.
// In your own scene: sprite + BoxCollider2D (tick "Is Trigger") + this script.
public class Plate : MonoBehaviour
{
    public bool IsPressed { get; private set; }

    SpriteRenderer sr;
    Collider2D col;
    ContactFilter2D filter;
    readonly Collider2D[] hits = new Collider2D[8];
    static readonly Color Off = new Color(0.95f, 0.78f, 0.2f);
    static readonly Color On = new Color(0.45f, 1f, 0.5f);

    bool wasPressed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        filter = new ContactFilter2D { useTriggers = false };
    }

    void FixedUpdate()
    {
        Vector2 center = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        Vector2 size = col != null ? (Vector2)col.bounds.size : (Vector2)transform.localScale;
        int n = Physics2D.OverlapBox(center, size, 0f, filter, hits);
        bool pressed = false;
        for (int i = 0; i < n; i++)
        {
            if (hits[i].GetComponent<Actor2D>() != null) { pressed = true; break; }
        }
        IsPressed = pressed;
        // Only tint generated plates; leave your own sprite colours alone.
        if (sr != null && tintWhenPressed) sr.color = pressed ? On : Off;

        if (IsPressed && !wasPressed)
        {
            if (AudioManager.I != null)
                AudioManager.I.PlayPlatePressed();
        }

        wasPressed = IsPressed;
    }

    public bool tintWhenPressed = false;
}
