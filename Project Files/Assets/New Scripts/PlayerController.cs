using UnityEngine;

public class PlayerController : Actor2D
{
    float movementRecordTimer;

    protected override float HeldDirection
    {
        get
        {
            var gm = GameManager.I;

            if (gm != null && gm.phase == Phase.Won)
                return 0f;

            float d = 0f;

            if (Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.LeftArrow))
            {
                d -= 1f;
            }

            if (Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.RightArrow))
            {
                d += 1f;
            }

            return d;
        }
    }

    protected override void Update()
    {
        // Completely disable player input while paused or on Game Over
        if (PauseManager.I != null &&
            PauseManager.I.InputLocked)
        {
            base.Update();
            return;
        }

        base.Update();

        var gm = GameManager.I;
        if (gm == null)
            return;

        if (gm.phase == Phase.Recording)
        {
            RecordInput();
            return;
        }

        if (gm.phase == Phase.Shadow)
        {
            ActionType? pressed = ReadActionPress();

            if (pressed.HasValue)
            {
                Perform(pressed.Value);
                gm.TriggerShadow();
            }
        }
    }

    // =========================
    // RECORD INPUT
    // =========================

    void RecordInput()
    {
        var gm = GameManager.I;

        bool left =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        bool right =
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space);

        // Both directions
        if (left && right)
        {
            movementRecordTimer = 0f;
            return;
        }

        // =========================
        // MOVE + JUMP
        // =========================

        if (jumpPressed && (left || right))
        {
            ActionType action =
                left
                    ? ActionType.MoveLeftJump
                    : ActionType.MoveRightJump;

            if (gm.TryRecord(action))
            {
                Perform(action);

                PlayJumpSound();

                movementRecordTimer = stepTime;
            }

            return;
        }

        // =========================
        // NORMAL JUMP
        // =========================

        if (jumpPressed)
        {
            if (gm.TryRecord(ActionType.Jump))
            {
                Perform(ActionType.Jump);

                PlayJumpSound();
            }

            return;
        }

        // =========================
        // ROLL
        // =========================

        if (Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.RightShift))
        {
            RecordAndPerform(ActionType.Roll);
            return;
        }

        // =========================
        // NO MOVEMENT
        // =========================

        if (!left && !right)
        {
            movementRecordTimer = 0f;
            return;
        }

        // =========================
        // MOVEMENT
        // =========================

        movementRecordTimer -= Time.deltaTime;

        if (movementRecordTimer <= 0f)
        {
            ActionType action =
                left
                    ? ActionType.MoveLeft
                    : ActionType.MoveRight;

            if (gm.TryRecord(action))
            {
                Perform(action);

                PlayWalkSound();

                movementRecordTimer = stepTime;
            }
        }
    }

    // =========================
    // RECORD + PERFORM
    // =========================

    void RecordAndPerform(ActionType action)
    {
        var gm = GameManager.I;

        if (gm.TryRecord(action))
        {
            Perform(action);
        }
    }

    // =========================
    // READ PLAYER INPUT
    // =========================

    static ActionType? ReadActionPress()
    {
        bool left =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        bool right =
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        bool jump =
            Input.GetKeyDown(KeyCode.Space);

        // Move + Jump
        if (right && jump)
            return ActionType.MoveRightJump;

        if (left && jump)
            return ActionType.MoveLeftJump;

        // Jump
        if (jump)
            return ActionType.Jump;

        // Roll
        if (Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.RightShift))
        {
            return ActionType.Roll;
        }

        // Move left
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return ActionType.MoveLeft;
        }

        // Move right
        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            return ActionType.MoveRight;
        }

        return null;
    }

    // =========================
    // AUDIO
    // =========================

    void PlayJumpSound()
    {
        if (AudioManager.I != null)
            AudioManager.I.PlayJump();
    }

    void PlayWalkSound()
    {
        if (AudioManager.I != null)
            AudioManager.I.PlayWalk();
    }

    bool IsJumpAction(ActionType action)
    {
        return action == ActionType.Jump ||
               action == ActionType.MoveLeftJump ||
               action == ActionType.MoveRightJump;
    }

    bool IsWalkAction(ActionType action)
    {
        return action == ActionType.MoveLeft ||
               action == ActionType.MoveRight;
    }
}