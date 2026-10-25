using UnityEngine;

public class DoorWinTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the real player can win
        if (other.GetComponent<PlayerController>() != null)
        {
            if (GameManager.I != null)
                GameManager.I.Win();
        }
    }
}