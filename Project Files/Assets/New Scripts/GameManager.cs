using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Phase
{
    Recording,
    Transition,
    Shadow,
    GameOver,
    Won
}

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public Phase phase = Phase.Recording;

    public string levelName = "";
    public string hint = "";

    [Header("Game")]
    public int maxActions = 3;
    public float transitionDelay = 0.9f;

    [Header("Shadow Collision")]
    public bool shadowKillsPlayer = true;

    [Header("Stats")]
    public int deaths;

    public event System.Action OnPlayerDied;

    public readonly List<ActionType> recorded =
        new List<ActionType>();

    public int ShadowIndex { get; private set; }

    [Header("Win Screen")]

    public int Remaining
    {
        get
        {
            return Mathf.Max(
                0,
                maxActions - recorded.Count
            );
        }
    }

    PlayerController player;
    ShadowController shadow;

    Transform levelRoot;
    Vector2 spawn;

    GameObject shadowPrefab;

    // =========================================================
    // AWAKE
    // =========================================================

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    // =========================================================
    // INIT
    // =========================================================

    public void Init(
        string name,
        string levelHint,
        int max,
        Vector2 spawnPos,
        PlayerController p,
        Transform root,
        GameObject shadowPrefab = null
    )
    {
        CancelInvoke();

        levelName = name;
        hint = levelHint;
        maxActions = max;

        spawn = spawnPos;

        player = p;
        levelRoot = root;

        this.shadowPrefab = shadowPrefab;

        shadow = null;

        deaths = 0;

        recorded.Clear();

        ShadowIndex = 0;

        phase = Phase.Recording;

        if (player != null)
        {
            player.TeleportTo(spawn);
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (player == null)
            return;

        // Restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
            return;
        }

        // Retry Shadow
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (phase == Phase.Shadow ||
                phase == Phase.Won)
            {
                BeginShadowPhase();
            }
        }
    }

    // =========================================================
    // COLLISION CHECK
    // =========================================================

    void FixedUpdate()
    {
        if (!shadowKillsPlayer)
            return;

        if (phase != Phase.Shadow)
            return;

        if (player == null || shadow == null)
            return;

        CheckShadowCollision();
    }

    void CheckShadowCollision()
    {
        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        Collider2D shadowCollider =
            shadow.GetComponent<Collider2D>();

        if (playerCollider == null ||
            shadowCollider == null)
            return;

        if (playerCollider.bounds.Intersects(
            shadowCollider.bounds))
        {
            GameOver();
        }
    }

    // =========================================================
    // RECORDING
    // =========================================================

    public bool TryRecord(ActionType action)
    {
        if (phase != Phase.Recording)
            return false;

        if (recorded.Count >= maxActions)
            return false;

        recorded.Add(action);

        if (recorded.Count >= maxActions)
        {
            phase = Phase.Transition;

            Invoke(
                nameof(BeginShadowPhase),
                transitionDelay
            );
        }

        return true;
    }

    // =========================================================
    // SHADOW PHASE
    // =========================================================

    public void BeginShadowPhase()
    {
        CancelInvoke();

        if (player == null)
            return;

        if (recorded.Count == 0)
            return;

        // Reset player velocity.
        Rigidbody2D playerRb =
            player.GetComponent<Rigidbody2D>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = true;
        }

        // Create Shadow if necessary.
        if (shadow == null)
        {
            SpawnShadow();
        }
        else
        {
            shadow.TeleportTo(spawn);

            Rigidbody2D shadowRb =
                shadow.GetComponent<Rigidbody2D>();

            if (shadowRb != null)
            {
                shadowRb.linearVelocity = Vector2.zero;
                shadowRb.angularVelocity = 0f;
                shadowRb.simulated = true;
            }
        }

        ShadowIndex = 0;

        phase = Phase.Shadow;
    }

    // =========================================================
    // SPAWN SHADOW
    // =========================================================

    void SpawnShadow()
    {
        GameObject go;

        InfoPopup.Show("YOUR PAST HAS ARRIVED");

        if (shadowPrefab != null)
        {
            go = Instantiate(
                shadowPrefab,
                spawn,
                Quaternion.identity
            );

            go.SetActive(true);
        }
        else
        {
            go = SpriteFactory.Make(
                "Shadow",
                levelRoot,
                spawn,
                new Vector2(0.8f, 1f),
                new Color(
                    0.55f,
                    0.35f,
                    0.95f,
                    0.65f
                ),
                4
            );
        }

        shadow =
            go.GetComponent<ShadowController>();

        if (shadow == null)
        {
            shadow =
                go.AddComponent<ShadowController>();
        }

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        Collider2D shadowCollider =
            shadow.GetComponent<Collider2D>();

        if (playerCollider != null &&
            shadowCollider != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                shadowCollider,
                true
            );
        }
    }

    // =========================================================
    // TRIGGER SHADOW ACTION
    // =========================================================

    // Called whenever the player presses an action
    // during Shadow phase.
    public void TriggerShadow()
    {
        if (phase != Phase.Shadow)
            return;

        if (shadow == null)
            return;

        if (ShadowIndex >= recorded.Count)
            return;

        ActionType action = recorded[ShadowIndex];

        shadow.Perform(action);

        // =========================
        // SHADOW AUDIO
        // =========================

        if (AudioManager.I != null)
        {
            switch (action)
            {
                case ActionType.MoveLeft:
                case ActionType.MoveRight:

                    AudioManager.I.PlayWalk();

                    break;

                case ActionType.Jump:
                case ActionType.MoveLeftJump:
                case ActionType.MoveRightJump:

                    AudioManager.I.PlayJump();

                    break;
            }
        }

        ShadowIndex++;
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    public void GameOver()
    {

        if (phase == Phase.Won)
            return;

        Debug.Log("GAME OVER TRIGGERED");

        if (PauseManager.I != null)
        {
            PauseManager.I.ShowGameOver();
        }
        else
        {
            Debug.LogError("PauseManager not found!");
        }
    }

    void StopActor(Actor2D actor)
    {
        if (actor == null)
            return;

        Rigidbody2D rb =
            actor.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
    }

    // =========================================================
    // FALLING
    // =========================================================

    public void OnActorFell(Actor2D a)
    {
        // PLAYER falls
        if (a is PlayerController)
        {
            deaths++;

            OnPlayerDied?.Invoke();

            player.TeleportTo(spawn);

            return;
        }

        // SHADOW falls
        if (a is ShadowController)
        {
            BeginShadowPhase();

            return;
        }

        // Safety fallback
        a.TeleportTo(spawn);
    }

    // =========================================================
    // WIN
    // =========================================================

    public void Win()
    {
        if (phase != Phase.Shadow)
            return;

        Debug.Log("WIN!");

        if (AudioManager.I != null)
            AudioManager.I.PlayWin();

        int currentScene =
            SceneManager.GetActiveScene().buildIndex;

        int lastScene =
            SceneManager.sceneCountInBuildSettings - 1;

        if (currentScene < lastScene)
        {
            SceneManager.LoadScene(currentScene + 1);
        }
        else
        {
            PauseManager.I.ShowWinScreen();
            phase = Phase.Won;
        }
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartLevel()
    {
        CancelInvoke();

        recorded.Clear();

        ShadowIndex = 0;

        // Destroy Shadow
        if (shadow != null)
        {
            Destroy(shadow.gameObject);
            shadow = null;
        }

        // Reset Player
        if (player != null)
        {
            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            player.TeleportTo(spawn);
        }

        phase = Phase.Recording;

        Debug.Log("LEVEL RESTARTED");
    }
}