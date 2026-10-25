using UnityEngine;

// Shared body for the Player and the Shadow.
// Handles movement, jumping, grounding and animation parameters.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class Actor2D : MonoBehaviour
{
    [Header("Tuning")]
    public float moveSpeed = 5f;
    public float stepTime = 0.35f;
    public float jumpVelocity = 12f;
    public float rollSpeed = 9f;
    public float rollTime = 0.4f;
    public float gravityScale = 3f;
    public float killY = -12f;

    protected Rigidbody2D rb;
    protected BoxCollider2D box;
    protected int facing = 1;

    protected Animator animator;

    float timer;
    float timedVx;

    static readonly Collider2D[] groundHits = new Collider2D[8];

    protected virtual float HeldDirection => 0f;

    public Bounds BodyBounds => box.bounds;

    Vector2 Vel
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
        set
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#else
            rb.velocity = value;
#endif
        }
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        animator = GetComponent<Animator>();

        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        box.sharedMaterial =
            new PhysicsMaterial2D("NoFriction")
            {
                friction = 0f,
                bounciness = 0f
            };
    }

    public bool IsGrounded
    {
        get
        {
            var b = box.bounds;

            var point = new Vector2(
                b.center.x,
                b.min.y - 0.05f
            );

            var size = new Vector2(
                b.size.x * 0.9f,
                0.1f
            );

            var filter = new ContactFilter2D
            {
                useTriggers = false
            };

            int n = Physics2D.OverlapBox(
                point,
                size,
                0f,
                filter,
                groundHits
            );

            for (int i = 0; i < n; i++)
            {
                var c = groundHits[i];

                if (c != box &&
                    c.GetComponent<Actor2D>() == null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // Performs one recorded action.
    public void Perform(ActionType action)
    {
        switch (action)
        {
            // -----------------------------------------
            // LEFT
            // -----------------------------------------

            case ActionType.MoveLeft:

                facing = -1;

                timedVx = -moveSpeed;
                timer = stepTime;

                break;


            // -----------------------------------------
            // RIGHT
            // -----------------------------------------

            case ActionType.MoveRight:

                facing = 1;

                timedVx = moveSpeed;
                timer = stepTime;

                break;


            // -----------------------------------------
            // LEFT + JUMP
            // -----------------------------------------

            case ActionType.MoveLeftJump:

                facing = -1;

                // Keep moving left while jumping.
                timedVx = -moveSpeed;
                timer = stepTime;

                if (IsGrounded)
                {
                    var v = Vel;
                    v.y = jumpVelocity;
                    Vel = v;

                    if (animator != null)
                        animator.SetTrigger("Jump");
                }

                break;


            // -----------------------------------------
            // RIGHT + JUMP
            // -----------------------------------------

            case ActionType.MoveRightJump:

                facing = 1;

                // Keep moving right while jumping.
                timedVx = moveSpeed;
                timer = stepTime;

                if (IsGrounded)
                {
                    var v = Vel;
                    v.y = jumpVelocity;
                    Vel = v;

                    if (animator != null)
                        animator.SetTrigger("Jump");
                }

                break;


            // -----------------------------------------
            // JUMP
            // -----------------------------------------

            case ActionType.Jump:

                if (IsGrounded)
                {
                    var v = Vel;
                    v.y = jumpVelocity;
                    Vel = v;

                    if (animator != null)
                        animator.SetTrigger("Jump");
                }

                break;


            // -----------------------------------------
            // ROLL
            // -----------------------------------------

            case ActionType.Roll:

                timedVx = facing * rollSpeed;
                timer = rollTime;

                break;
        }
    }

    public void TeleportTo(Vector2 p)
    {
        timer = 0f;
        timedVx = 0f;

        transform.position = p;
        rb.position = p;

        Vel = Vector2.zero;
    }

    protected virtual void FixedUpdate()
    {
        var v = Vel;

        float held = HeldDirection;

        if (held != 0f)
        {
            facing = held > 0f ? 1 : -1;
        }

        // -----------------------------------------
        // TIMED HORIZONTAL MOVEMENT
        // -----------------------------------------

        if (timer > 0f)
        {
            v.x = timedVx;

            timer -= Time.fixedDeltaTime;

            if (timer <= 0f)
            {
                timer = 0f;
                v.x = 0f;
            }
        }
        else
        {
            v.x = 0f;
        }

        Vel = v;

        // -----------------------------------------
        // FALL CHECK
        // -----------------------------------------

        if (transform.position.y < killY &&
            GameManager.I != null)
        {
            GameManager.I.OnActorFell(this);
        }
    }

    // --------------------------------------------------
    // ANIMATION
    // --------------------------------------------------

    protected virtual void Update()
    {
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (animator == null)
            return;

        bool grounded = IsGrounded;

        float horizontalSpeed = Mathf.Abs(Vel.x);
        float verticalSpeed = Vel.y;

        animator.SetBool("Grounded", grounded);
        animator.SetFloat("AirSpeedY", verticalSpeed);

        // IDLE / RUN
        if (grounded)
        {
            if (horizontalSpeed > 0.1f)
            {
                animator.SetInteger("AnimState", 1);
            }
            else
            {
                animator.SetInteger("AnimState", 0);
            }
        }

        // Flip sprite
        if (facing < 0)
        {
            transform.localScale =
                new Vector3(-1f, 1f, 1f);
        }
        else
        {
            transform.localScale =
                new Vector3(1f, 1f, 1f);
        }
    }
}