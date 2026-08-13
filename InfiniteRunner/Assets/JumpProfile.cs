using UnityEngine;

/// <summary>
/// Data only — every jump tuning number, no logic. It's a ScriptableObject, so
/// the values live in an .asset you create via
/// Assets > Create > Infinite Runner > Jump Profile, separate from any GameObject.
/// Make several and hot-swap them; JumpController holds the logic that reads one.
///
/// The whole "Canabalt core" is four feel knobs: jumpHeight + timeToApex shape the
/// arc ("how high, how long"), fallGravityMult makes the fall heavier than the
/// rise, and jumpCutMultiplier gives "hold longer = jump higher". The rest is a
/// floor (minJumpHeight), one set-once correctness value (groundedTolerance), and
/// optional squash & stretch polish (juice, off by default).
/// </summary>
[CreateAssetMenu(fileName = "JumpProfile", menuName = "Infinite Runner/Jump Profile")]
public class JumpProfile : ScriptableObject
{
    public ArcSettings arc = new ArcSettings();
    public GravitySettings gravity = new GravitySettings();
    public AssistSettings assist = new AssistSettings();
    public JuiceSettings juice = new JuiceSettings();

    // ---------------------------------------------------------------------
    // The SHAPE of the jump, described the way a designer thinks about it:
    // "how high" and "how long to get there" — not raw gravity numbers.
    // JumpController converts these into gravity + launch velocity for you.
    // ---------------------------------------------------------------------
    [System.Serializable]
    public class ArcSettings
    {
        [Tooltip("How high a FULL jump reaches, in world units.")]
        public float jumpHeight = 3.5f;

        [Tooltip("Seconds from leaving the ground to the top of the jump. Smaller = snappier.")]
        public float timeToApex = 0.4f;

        [Tooltip("Even a quick tap guarantees at least this height (world units).")]
        public float minJumpHeight = 1f;
    }

    // ---------------------------------------------------------------------
    // What makes the arc feel good: a heavier fall than rise, and the ability
    // to cut a jump short by releasing early.
    // ---------------------------------------------------------------------
    [System.Serializable]
    public class GravitySettings
    {
        [Tooltip("Gravity multiplier while falling. Above 1 = heavier, snappier fall than the rise.")]
        public float fallGravityMult = 1.8f;

        [Range(0f, 1f)]
        [Tooltip("On early release while rising, upward speed is multiplied by this. Lower = shorter hop.")]
        public float jumpCutMultiplier = 0.5f;
    }

    // ---------------------------------------------------------------------
    // Not a feel knob — a correctness value. Set once and leave it.
    // ---------------------------------------------------------------------
    [System.Serializable]
    public class AssistSettings
    {
        [Tooltip("How far below the feet we probe for ground. Set once (~0.1); it's correctness, not feel.")]
        public float groundedTolerance = 0.1f;
    }

    // ---------------------------------------------------------------------
    // Cosmetic only — read by JumpJuice, never by the physics. Off by default
    // for a stark Canabalt look; flip squashAndStretch on to enable it.
    // ---------------------------------------------------------------------
    [System.Serializable]
    public class JuiceSettings
    {
        [Tooltip("Master on/off switch for sprite squash & stretch. Off by default.")]
        public bool squashAndStretch = false;

        [Tooltip("Scale when launching (x squeezed, y stretched).")]
        public Vector2 jumpStretch = new Vector2(0.85f, 1.15f);

        [Tooltip("Scale on landing (x widened, y squashed).")]
        public Vector2 landSquash = new Vector2(1.15f, 0.85f);

        [Tooltip("How quickly the sprite springs back to its normal scale.")]
        public float recoverSpeed = 8f;
    }
}
