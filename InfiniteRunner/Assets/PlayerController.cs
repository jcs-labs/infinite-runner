using UnityEngine;

// Owns only the lose condition: detect the fatal hit and report it to the GameManager,
// which owns what actually happens next (freeze, restart, and later score).
public class PlayerController : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
