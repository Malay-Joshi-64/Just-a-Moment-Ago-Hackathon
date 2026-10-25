using UnityEngine;

// Put this on an empty GameObject in YOUR scene. It replaces the generated levels.
// Add the HUD component to the same object to customise the on-screen UI.
public class SceneLevel : MonoBehaviour
{
    public string levelName = "My Level";
    [TextArea] public string hint = "";
    public int maxActions = 3;

    [Header("Scene references")]
    public PlayerController player;      // your player object (has PlayerController)
    public Transform spawnPoint;         // empty object where Player/Shadow start
    public GameObject shadowPrefab;      // your shadow sprite prefab (optional; needs no extra scripts)

    [Header("Rules")]
    public bool shadowKillsPlayer = true;
    public float transitionDelay = 0.9f;

    void Awake()
    {
        if (GetComponent<GameManager>() == null) gameObject.AddComponent<GameManager>();
        if (GetComponent<HUD>() == null) gameObject.AddComponent<HUD>();
    }

    void Start()
    {
        if (player == null || spawnPoint == null)
        {
            Debug.LogError("SceneLevel: assign Player and Spawn Point in the Inspector.");
            return;
        }
        var gm = GameManager.I;
        gm.shadowKillsPlayer = shadowKillsPlayer;
        gm.transitionDelay = transitionDelay;
        gm.Init(levelName, hint, maxActions, spawnPoint.position, player, null, shadowPrefab);

        var follow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (follow != null) { follow.target = player.transform; follow.Snap(); }
    }
}