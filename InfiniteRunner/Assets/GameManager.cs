using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns game flow: game-over state, restart, and the score.
/// PlayerController just reports the fatal collision here; this decides what happens.
///
/// Two deliberate choices worth knowing:
///  - It self-bootstraps (see Bootstrap) so it needs no scene wiring or Inspector setup.
///  - It is NOT DontDestroyOnLoad. Restarting reloads the scene, which destroys and
///    recreates this object, so the current score resets to zero for free. The BEST
///    run has to outlive that reload, so it lives on disk in PlayerPrefs instead.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    [Tooltip("Key that restarts the run once you've lost.")]
    public KeyCode restartKey = KeyCode.R;

    [Tooltip("Score gained per second survived.")]
    public float pointsPerSecond = 10f;

    const string BestRunKey = "BestRun";

    // Accumulate in a float: each frame adds a fraction of a point, which would
    // truncate to 0 forever in an int. We only convert to a whole number to show it.
    float score;
    int bestRun;

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

    void Start()
    {
        // Best run is the only thing that survives a restart, so read it from disk.
        bestRun = PlayerPrefs.GetInt(BestRunKey, 0);
    }

    void Update()
    {
        // Time.deltaTime is 0 while frozen, so the score naturally stops climbing at
        // game over; the IsGameOver guard just makes that intent explicit.
        if (!IsGameOver)
            score += Time.deltaTime * pointsPerSecond;

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

        int finalScore = Mathf.FloorToInt(score);
        if (finalScore > bestRun)
        {
            bestRun = finalScore;
            PlayerPrefs.SetInt(BestRunKey, bestRun);
            PlayerPrefs.Save(); // flush now so a crash can't lose the record
        }
    }

    void Restart()
    {
        // CRITICAL: timeScale is a global and is NOT reset by loading a scene.
        // Reload while it's still 0 and the fresh scene is frozen too. Un-freeze first.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Rough on-screen readout. A polished version would use a Canvas + TextMeshPro;
    // OnGUI needs no scene objects or fonts, so it works the instant you press Play.
    void OnGUI()
    {
        var readout = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        readout.normal.textColor = Color.white;
        GUI.Label(new Rect(12, 8, 400, 30), $"Score: {Mathf.FloorToInt(score)}", readout);
        GUI.Label(new Rect(12, 36, 400, 30), $"Best: {bestRun}", readout);

        if (!IsGameOver) return;

        var over = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold
        };
        over.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 80),
            $"GAME OVER\nPress {restartKey} to restart", over);
    }
}
