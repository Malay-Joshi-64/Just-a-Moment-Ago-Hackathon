using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public List<Plate> plates = new List<Plate>();

    [Header("Door Trigger")]
    public GameObject doorTrigger;
    [Header("Door Sprites")]

    public Sprite closedSprite;
    public Sprite openSprite;

    public bool IsOpen { get; private set; }

    Collider2D col;
    SpriteRenderer sr;

    bool wasOpen;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        bool open = plates.Count > 0;

        foreach (var p in plates)
        {
            if (p == null || !p.IsPressed)
            {
                open = false;
                break;
            }
        }

        if (open && !wasOpen)
        {
            if (AudioManager.I != null)
                AudioManager.I.PlayDoorOpen();
        }

        wasOpen = open;

        IsOpen = open;

        // Door collision
        if (col != null)
            col.enabled = !open;

        // Door sprite
        if (sr != null)
        {
            sr.sprite = open ? openSprite : closedSprite;
        }
        IsOpen = open;

        if (doorTrigger != null)
            doorTrigger.SetActive(open);
    }
}