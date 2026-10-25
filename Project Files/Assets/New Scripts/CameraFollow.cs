using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(2f, 2.5f);
    public float smooth = 6f;

    public void Snap()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10f);
    }

    void LateUpdate()
    {
        if (target == null) return;
        var goal = new Vector3(target.position.x + offset.x, Mathf.Max(target.position.y + offset.y, 2.5f), -10f);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
