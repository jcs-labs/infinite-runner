using UnityEngine;

/// <summary>
/// Draws the build version in the bottom-right corner. This is pure presentation,
/// not game state: it reads the version straight from Application.version (set in
/// Project Settings > Player), so there's a single source of truth and nothing to
/// bump by hand here.
///
/// Self-bootstraps so it needs no scene wiring. Uses OnGUI so it needs no Canvas or
/// font asset and shows up the instant you press Play.
/// </summary>
public class VersionLabel : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<VersionLabel>() == null)
            new GameObject(nameof(VersionLabel)).AddComponent<VersionLabel>();
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerRight };
        style.normal.textColor = new Color(1f, 1f, 1f, 0.5f); // dim, stays out of the way

        string version = string.IsNullOrEmpty(Application.version) ? "dev" : Application.version;
        GUI.Label(new Rect(0, Screen.height - 24, Screen.width - 8, 20), $"v{version}", style);
    }
}
