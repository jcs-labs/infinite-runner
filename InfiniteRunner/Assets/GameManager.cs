using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns game flow: whether the run is over, ending it, and restarting it.
/// PlayerController just reports the fatal collision here; this decides what happens.
///
/// Two deliberate choices worth knowing:
///  - It self-bootstraps (see Bootstrap) so it needs no scene wiring or Inspector setup.
///  - It is NOT DontDestroyOnLoad. Restarting reloads the scene, which destroys and
///    recreates this object, so every run starts from a clean slate for free.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    [Tooltip("Key that restarts the run once you've lost.")]
    public KeyCode restartKey = KeyCode.R;

    // Spawn a GameManager after every scene load if one isn't already present.
    // Runs on first launch AND after each Restart() reload, with zero scene setup.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<GameManager>() == null)
            new GameObject(nameof(GameManager)).AddComponent<GameManager>();
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Input still fires while frozen: timeScale only halts time-based motion,
        // not Update or key polling. So we can watch for restart during the freeze.
        if (IsGameOver && Input.GetKeyDown(restartKey))
            Restart();
    }

    // Called by PlayerController when the player hits an obstacle.
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f; // freeze everything (all time-based motion stops)
    }

    void Restart()
    {
        // CRITICAL: timeScale is a global and is NOT reset by loading a scene.
        // Reload while it's still 0 and the fresh scene is frozen too. Un-freeze first.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Rough on-screen prompt. A polished version would use a Canvas + TextMeshPro;
    // OnGUI needs no scene objects or fonts, so it works the instant you press Play.
    void OnGUI()
    {
        if (!IsGameOver) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 80),
            $"GAME OVER\nPress {restartKey} to restart", style);
    }
}
