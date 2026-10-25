using UnityEngine;
using UnityEngine.EventSystems;

public class TextEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Target")]
    public RectTransform target;

    [Header("Zoom")]
    public bool zoomEffect = true;

    [Tooltip("How much the object zooms in/out.")]
    public float zoomAmount = 0.03f;

    [Tooltip("How fast the zoom happens.")]
    public float zoomSpeed = 2f;

    [Header("Random Zoom")]
    [Tooltip("Gives every object its own random zoom timing.")]
    public bool randomizeZoom = true;

    [Tooltip("Random amount added to the zoom speed.")]
    public float randomSpeedVariation = 0.3f;

    [Header("Floating")]
    public bool floatEffect = false;
    public float floatAmount = 5f;
    public float floatSpeed = 1.5f;

    [Header("Wobble")]
    public bool wobbleEffect = false;
    public float wobbleAmount = 2f;
    public float wobbleSpeed = 2f;

    [Header("Intro Animation")]
    public bool playIntro = true;
    public float introDuration = 0.8f;

    [Header("Hover")]
    public bool hoverEffect = true;
    public float hoverScale = 1.08f;
    public float hoverSpeed = 10f;

    Vector3 originalScale;
    Vector2 originalPosition;
    Quaternion originalRotation;

    float introTimer;
    bool introFinished;

    bool isHovering;

    // Random zoom values for THIS object
    float zoomOffset;
    float actualZoomSpeed;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (target == null)
        {
            Debug.LogError(
                "TextEffect: No RectTransform found.",
                this
            );

            enabled = false;
            return;
        }

        originalScale = target.localScale;
        originalPosition = target.anchoredPosition;
        originalRotation = target.localRotation;

        // Give every object a different starting point.
        if (randomizeZoom)
        {
            zoomOffset = Random.Range(
                0f,
                Mathf.PI * 2f
            );

            actualZoomSpeed = Random.Range(
                zoomSpeed - randomSpeedVariation,
                zoomSpeed + randomSpeedVariation
            );

            // Prevent negative speed.
            actualZoomSpeed =
                Mathf.Max(0.1f, actualZoomSpeed);
        }
        else
        {
            zoomOffset = 0f;
            actualZoomSpeed = zoomSpeed;
        }

        if (playIntro)
        {
            target.localScale = Vector3.zero;

            introTimer = 0f;
            introFinished = false;
        }
        else
        {
            introFinished = true;
        }
    }

    void Update()
    {
        if (target == null)
            return;

        HandleIntro();

        if (!introFinished)
            return;

        HandleEffects();
    }

    // =========================================================
    // INTRO
    // =========================================================

    void HandleIntro()
    {
        if (introFinished)
            return;

        introTimer += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(
            introTimer / introDuration
        );

        // Smooth ease-out.
        float eased =
            1f - Mathf.Pow(1f - t, 3f);

        target.localScale = Vector3.Lerp(
            Vector3.zero,
            originalScale,
            eased
        );

        if (t >= 1f)
        {
            target.localScale = originalScale;
            introFinished = true;
        }
    }

    // =========================================================
    // MAIN EFFECTS
    // =========================================================

    void HandleEffects()
    {
        // -----------------------------------------------------
        // RANDOM ZOOM
        // -----------------------------------------------------

        float zoomMultiplier = 1f;

        if (zoomEffect)
        {
            float pulse = Mathf.Sin(
                Time.unscaledTime * actualZoomSpeed
                + zoomOffset
            ) * zoomAmount;

            zoomMultiplier = 1f + pulse;
        }

        // -----------------------------------------------------
        // HOVER
        // -----------------------------------------------------

        float hoverMultiplier =
            isHovering
                ? hoverScale
                : 1f;

        // Combine zoom + hover.
        float finalScale =
            zoomMultiplier * hoverMultiplier;

        Vector3 desiredScale =
            originalScale * finalScale;

        target.localScale = Vector3.Lerp(
            target.localScale,
            desiredScale,
            Time.unscaledDeltaTime * hoverSpeed
        );

        // -----------------------------------------------------
        // FLOAT
        // -----------------------------------------------------

        if (floatEffect)
        {
            float offset =
                Mathf.Sin(
                    Time.unscaledTime * floatSpeed
                ) * floatAmount;

            target.anchoredPosition =
                originalPosition +
                new Vector2(0f, offset);
        }
        else
        {
            target.anchoredPosition =
                originalPosition;
        }

        // -----------------------------------------------------
        // WOBBLE
        // -----------------------------------------------------

        if (wobbleEffect)
        {
            float angle =
                Mathf.Sin(
                    Time.unscaledTime * wobbleSpeed
                ) * wobbleAmount;

            target.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            target.localRotation =
                originalRotation;
        }
    }

    // =========================================================
    // MOUSE HOVER
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        isHovering = false;
    }

    // =========================================================
    // REPLAY INTRO
    // =========================================================

    public void PlayIntro()
    {
        introTimer = 0f;
        introFinished = false;

        target.localScale = Vector3.zero;
    }

    // =========================================================
    // PULSE
    // =========================================================

    public void Pulse()
    {
        StopCoroutine(nameof(PulseRoutine));
        StartCoroutine(PulseRoutine());
    }

    System.Collections.IEnumerator PulseRoutine()
    {
        Vector3 start =
            target.localScale;

        Vector3 bigger =
            originalScale * 1.15f;

        float t = 0f;

        while (t < 1f)
        {
            t +=
                Time.unscaledDeltaTime * 5f;

            float eased =
                Mathf.Sin(t * Mathf.PI);

            target.localScale =
                Vector3.Lerp(
                    start,
                    bigger,
                    eased
                );

            yield return null;
        }

        target.localScale =
            originalScale;
    }
}