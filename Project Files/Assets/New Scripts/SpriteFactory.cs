using UnityEngine;

// Generates a 1x1 world-unit white square so the whole prototype needs zero art assets.
public static class SpriteFactory
{
    static Sprite sprite;

    public static Sprite Square()
    {
        if (sprite == null)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color32[16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            // 4 pixels at 4 pixels-per-unit = exactly 1 world unit.
            sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
        return sprite;
    }

    public static GameObject Make(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square();
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }
}
