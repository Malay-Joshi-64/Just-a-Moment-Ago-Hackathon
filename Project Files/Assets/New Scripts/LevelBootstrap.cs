using System.Collections.Generic;
using UnityEngine;

// Zero-setup entry point: press Play in ANY scene and the game builds itself from primitives.
public class LevelBootstrap : MonoBehaviour
{
    const int LevelCount = 4;

    int index;
    Transform root;
    GameManager gm;
    Camera cam;

    static readonly Color GroundColor = new Color(0.28f, 0.3f, 0.36f);
    static readonly Color PlayerColor = new Color(0.3f, 0.85f, 0.95f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    /*static void AutoStart()
    {
        if (FindFirstObjectByType<LevelBootstrap>() != null) return;
        if (FindFirstObjectByType<SceneLevel>() != null) return; // you built your own scene
        new GameObject("JustAMomentAgo").AddComponent<LevelBootstrap>();
    }*/

    void Awake()
    {
        gm = gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<HUD>();

        cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.07f, 0.1f);
        if (cam.GetComponent<CameraFollow>() == null) cam.gameObject.AddComponent<CameraFollow>();
        Physics2D.gravity = new Vector2(0f, -9.81f);

        Build(0);
    }

    void Update()
    {
        if (gm.phase == Phase.Won && Input.GetKeyDown(KeyCode.N))
        {
            index = (index + 1) % LevelCount;
            Build(index);
        }
    }

    void Build(int i)
    {
        if (root != null) Destroy(root.gameObject);
        root = new GameObject("Level").transform;

        Solid("Ground", new Vector2(8f, -0.5f), new Vector2(36f, 1f));
        Solid("WallL", new Vector2(-10.5f, 5f), new Vector2(1f, 12f));
        Solid("WallR", new Vector2(26.5f, 5f), new Vector2(1f, 12f));

        var plates = new List<Plate>();
        string name, hint;
        int max;
        float doorX, goalX;

        switch (i)
        {
            default:
            case 0:
                name = "1. First Echo"; max = 2;
                hint = "Record Right, Right. Then press D twice and the Shadow walks onto the plate while you run for the goal.";
                plates.Add(AddPlate(3.5f, 0f));
                doorX = 7f; goalX = 10f;
                break;
            case 1:
                name = "2. Wrong Way"; max = 2;
                hint = "The plate is behind you. The keys that move the Shadow move YOU too, so plan the way back.";
                plates.Add(AddPlate(-3.5f, 0f));
                doorX = 4f; goalX = 9f;
                break;
            case 2:
                name = "3. Higher Ground"; max = 3;
                hint = "The plate sits on a ledge. Try Right, Jump, Right. The timing of your presses matters!";
                Solid("Ledge", new Vector2(3.5f, 0.75f), new Vector2(3f, 1.5f));
                plates.Add(AddPlate(3.5f, 1.5f));
                doorX = 8f; goalX = 11f;
                break;
            case 3:
                name = "4. Roll With It"; max = 2;
                hint = "A Roll is a long dash. Combine it with a step to reach the far plate.";
                plates.Add(AddPlate(5.4f, 0f));
                doorX = 9f; goalX = 12f;
                break;
        }

        AddDoor(doorX, plates);
        AddGoal(goalX);

        var spawn = new Vector2(0f, 0.5f);
        var playerGo = SpriteFactory.Make("Player", root, spawn, new Vector2(0.8f, 1f), PlayerColor, 5);
        var player = playerGo.AddComponent<PlayerController>();

        gm.Init(name, hint, max, spawn, player, root);

        var follow = cam.GetComponent<CameraFollow>();
        follow.target = player.transform;
        follow.Snap();
    }

    void Solid(string n, Vector2 pos, Vector2 size)
    {
        var go = SpriteFactory.Make(n, root, pos, size, GroundColor, 0);
        go.AddComponent<BoxCollider2D>();
    }

    Plate AddPlate(float x, float surfaceY)
    {
        var go = SpriteFactory.Make("Plate", root, new Vector2(x, surfaceY + 0.1f), new Vector2(1.5f, 0.2f),
            new Color(0.95f, 0.78f, 0.2f), 1);
        var plate = go.AddComponent<Plate>();
        plate.tintWhenPressed = true;
        return plate;
    }

    Door AddDoor(float x, List<Plate> plates)
    {
        var go = SpriteFactory.Make("Door", root, new Vector2(x, 2.5f), new Vector2(0.6f, 5f),
            new Color(0.85f, 0.25f, 0.25f), 3);
        go.AddComponent<BoxCollider2D>();
        var d = go.AddComponent<Door>();
        d.plates = plates;
        return d;
    }

    void AddGoal(float x)
    {
        var go = SpriteFactory.Make("Goal", root, new Vector2(x, 1f), new Vector2(1.2f, 2f),
            new Color(0.3f, 0.95f, 0.45f, 0.5f), 1);
        go.AddComponent<Goal>();
    }
}
