using System.Text;
using UnityEngine;

public enum HudAnchor { TopLeft, TopCenter, TopRight, BottomLeft, BottomCenter, BottomRight }

// Add this component to your Level object (next to SceneLevel) to customise everything in the Inspector.
// Message tokens: {remaining} = actions left, {max} = action limit, {deaths} = death count, {level} = level name.
public class HUD : MonoBehaviour
{
    [Header("Layout")]
    public HudAnchor anchor = HudAnchor.TopLeft;
    public Vector2 offset = new Vector2(16f, 10f);
    public float panelWidth = 700f;
    public float padding = 10f;
    public bool showBackground = false;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);

    [Header("Fonts")]
    [SerializeField] private Font gameTitleFont;
    [SerializeField] private Font textFont;                      // leave empty for Unity's default font
    public int titleSize = 26;
    public int bodySize = 18;
    public int hintSize = 15;
    public float lineSpacing = 2f;

    [Header("Show / Hide")]
    public bool showTitle = true;
    public bool showActionPips = true;
    public bool showRecorded = true;
    public bool showStatus = true;
    public bool showDeaths = true;
    public bool showHint = true;
    public bool showControls = true;

    [Header("Colors")]
    public Color textColor = Color.white;
    public Color hintColor = new Color(0.67f, 0.67f, 0.67f);
    public Color nextActionColor = new Color(1f, 0.82f, 0.29f);   // the Shadow's next action
    public Color pipFilledColor = new Color(1f, 0.82f, 0.29f);
    public Color pipEmptyColor = new Color(0.6f, 0.6f, 0.6f);
    public Color recordingColor = Color.white;
    public Color transitionColor = new Color(0.7f, 0.6f, 1f);
    public Color shadowPhaseColor = new Color(0.75f, 0.6f, 1f);
    public Color wonColor = new Color(0.49f, 1f, 0.54f);

    [Header("Text")]
    public string gameTitle = "Just a Moment Ago";
    public string titleFormat = "{game}  -  {level}";   // {game} and {level}
    public string actionsLabel = "Actions: ";
    public string recordedLabel = "Recorded: ";
    public string emptyRecorded = "-";
    public string sequenceSeparator = "  >  ";
    public string deathsLabel = "Deaths: ";
    [TextArea] public string recordingMessage = "RECORDING - every key PRESS (A/D, Space, Shift) is one action. Left: {remaining}";
    [TextArea] public string transitionMessage = "Your Shadow is forming...";
    [TextArea] public string shadowMessage = "SHADOW PHASE - every action you press also makes the Shadow do its NEXT recorded action. Don't touch it!";
    [TextArea] public string wonMessage = "LEVEL CLEAR!  Press N for the next level.";
    [TextArea] public string controlsText = "R: restart level   T: retry shadow run   N: next (when cleared)";

    public string allLevelsDoneMessage = "ALL LEVELS DONE!  You completed the entire game!";

    [Header("Action pips")]
    public string pipFilledGlyph = "●";
    public string pipEmptyGlyph = "○";
    public Texture2D pipFilledTexture;       // optional: draw icons instead of glyphs
    public Texture2D pipEmptyTexture;
    public float pipSize = 22f;
    public float pipSpacing = 4f;

    [Header("Action names (shown in the Recorded line)")]
    public string moveLeftName = "Left";
    public string moveRightName = "Right";
    public string jumpName = "Jump";
    public string rollName = "Roll";

    GUIStyle title, body, hintStyle, panel;
    Texture2D bgTex;

    static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGBA(c);

    string NameOf(ActionType a)
    {
        switch (a)
        {
            case ActionType.MoveLeft: return moveLeftName;
            case ActionType.MoveRight: return moveRightName;
            case ActionType.Jump: return jumpName;
            default: return rollName;
        }
    }

    string Fill(string s, GameManager gm)
    {
        return s.Replace("{remaining}", gm.Remaining.ToString())
                .Replace("{max}", gm.maxActions.ToString())
                .Replace("{deaths}", gm.deaths.ToString())
                .Replace("{level}", gm.levelName);
    }

    void BuildStyles()
    {
        TextAnchor align = anchor == HudAnchor.TopCenter || anchor == HudAnchor.BottomCenter ? TextAnchor.MiddleCenter
            : (anchor == HudAnchor.TopRight || anchor == HudAnchor.BottomRight ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);

        title = new GUIStyle(GUI.skin.label) { fontSize = titleSize, fontStyle = FontStyle.Bold, richText = true, wordWrap = true, alignment = align };
        body = new GUIStyle(GUI.skin.label) { fontSize = bodySize, richText = true, wordWrap = true, alignment = align };
        hintStyle = new GUIStyle(GUI.skin.label) { fontSize = hintSize, richText = true, wordWrap = true, alignment = align };
        if (gameTitleFont != null)
            title.font = gameTitleFont;

        // Everything else uses the regular text font
        if (textFont != null)
        {
            body.font = textFont;
            hintStyle.font = textFont;
        }

        foreach (var s in new[] { title, body, hintStyle })
        {
            s.normal.textColor = textColor;
            s.margin = new RectOffset(0, 0, 0, Mathf.RoundToInt(lineSpacing));
        }
        hintStyle.normal.textColor = hintColor;

        if (bgTex == null) { bgTex = new Texture2D(1, 1); bgTex.hideFlags = HideFlags.HideAndDontSave; }
        bgTex.SetPixel(0, 0, backgroundColor);
        bgTex.Apply();
        panel = new GUIStyle { padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding) };
        if (showBackground) panel.normal.background = bgTex;
    }

    void OnDestroy() { if (bgTex != null) Destroy(bgTex); }

    void OnGUI()
    {
        var gm = GameManager.I;
        if (gm == null) return;
        BuildStyles();

        bool right = anchor == HudAnchor.TopRight || anchor == HudAnchor.BottomRight;
        bool center = anchor == HudAnchor.TopCenter || anchor == HudAnchor.BottomCenter;
        bool bottom = anchor == HudAnchor.BottomLeft || anchor == HudAnchor.BottomCenter || anchor == HudAnchor.BottomRight;

        float w = Mathf.Min(panelWidth, Screen.width - 2f * Mathf.Abs(offset.x));
        float x = right ? Screen.width - w - offset.x : (center ? (Screen.width - w) * 0.5f + offset.x : offset.x);
        var area = new Rect(x, bottom ? 0f : offset.y, w, bottom ? Screen.height - offset.y : Screen.height - offset.y);

        GUILayout.BeginArea(area);
        if (bottom) GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(panel, GUILayout.Width(w));

        if (showTitle)
            GUILayout.Label(titleFormat.Replace("{game}", gameTitle).Replace("{level}", gm.levelName), title);

        if (showActionPips) DrawPips(gm);

        if (showRecorded)
        {
            var seq = new StringBuilder(recordedLabel);
            if (gm.recorded.Count == 0) seq.Append(emptyRecorded);
            for (int i = 0; i < gm.recorded.Count; i++)
            {
                string n = NameOf(gm.recorded[i]);
                bool next = gm.phase == Phase.Shadow && i == gm.ShadowIndex;
                seq.Append(next ? "<color=" + Hex(nextActionColor) + ">[" + n + "]</color>" : n);
                if (i < gm.recorded.Count - 1) seq.Append(sequenceSeparator);
            }
            GUILayout.Label(seq.ToString(), body);
        }

        if (showStatus)
        {
            string msg;
            Color c;

            switch (gm.phase)
            {
                case Phase.Recording:
                    msg = recordingMessage;
                    c = recordingColor;
                    break;

                case Phase.Transition:
                    msg = transitionMessage;
                    c = transitionColor;
                    break;

                case Phase.Shadow:
                    msg = shadowMessage;
                    c = shadowPhaseColor;
                    break;

                case Phase.Won:
                    int currentIndex = UnityEngine.SceneManagement.SceneManager
                        .GetActiveScene().buildIndex;

                    int lastIndex = UnityEngine.SceneManagement.SceneManager
                        .sceneCountInBuildSettings - 1;

                    if (currentIndex == lastIndex)
                        msg = allLevelsDoneMessage;
                    else
                        msg = wonMessage;

                    c = wonColor;
                    break;

                default:
                    msg = "";
                    c = textColor;
                    break;
            }

            GUILayout.Label(
                "<color=" + Hex(c) + ">" + Fill(msg, gm) + "</color>",
                body
            );
        }

        if (showDeaths) GUILayout.Label(deathsLabel + gm.deaths, body);
        if (showHint && !string.IsNullOrEmpty(gm.hint)) GUILayout.Label("Hint: " + gm.hint, hintStyle);
        if (showControls) GUILayout.Label(controlsText, hintStyle);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void DrawPips(GameManager gm)
    {
        bool useTex = pipFilledTexture != null && pipEmptyTexture != null;
        GUILayout.BeginHorizontal();
        if (anchor == HudAnchor.TopCenter || anchor == HudAnchor.BottomCenter) GUILayout.FlexibleSpace();
        if (anchor == HudAnchor.TopRight || anchor == HudAnchor.BottomRight) GUILayout.FlexibleSpace();

        GUILayout.Label(actionsLabel, body, GUILayout.ExpandWidth(false));
        for (int i = 0; i < gm.maxActions; i++)
        {
            bool filled = i < gm.recorded.Count;
            if (useTex)
            {
                var r = GUILayoutUtility.GetRect(pipSize, pipSize, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(r, filled ? pipFilledTexture : pipEmptyTexture, ScaleMode.ScaleToFit);
                GUILayout.Space(pipSpacing);
            }
            else
            {
                string g = filled ? pipFilledGlyph : pipEmptyGlyph;
                Color c = filled ? pipFilledColor : pipEmptyColor;
                GUILayout.Label("<color=" + Hex(c) + ">" + g + "</color>", body, GUILayout.ExpandWidth(false));
            }
        }

        if (anchor == HudAnchor.TopCenter || anchor == HudAnchor.BottomCenter) GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    [TextArea]
    public string gameOverMessage =
    "GAME OVER - You touched your past. Press R to restart.";
}