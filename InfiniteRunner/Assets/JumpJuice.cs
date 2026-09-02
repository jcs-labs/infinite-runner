using UnityEngine;

/// <summary>
/// Purely COSMETIC. Squashes and stretches the sprite on jump and landing.
///
/// This is a separate component on purpose: it demonstrates the pattern of
/// keeping "juice" (visual polish) out of the physics. JumpController shouts
/// "I jumped!" / "I landed!" via events; this listens and reacts. JumpController
/// has no idea sprites even exist, and you could delete this file with zero
/// effect on how the jump plays.
/// </summary>
[RequireComponent(typeof(JumpController))]
public class JumpJuice : MonoBehaviour
{
    [Tooltip("The transform to scale. Leave empty to use this object's own transform.")]
    public Transform target;

    JumpController jump;
    JumpProfile Profile => jump.profile;
    Vector3 baseScale;
    Vector3 pulseScale; // the momentary squash/stretch we spring back from

    void Awake()
    {
        jump = GetComponent<JumpController>();
        if (target == null) target = transform;
        baseScale = target.localScale;
        pulseScale = baseScale;
    }

    // Subscribe/unsubscribe to the controller's events. Always unsubscribe in
    // OnDisable to avoid leaks — a habit worth building early.
    void OnEnable()
    {
        jump.OnJump += HandleJump;
        jump.OnLand += HandleLand;
    }

    void OnDisable()
    {
        jump.OnJump -= HandleJump;
        jump.OnLand -= HandleLand;
    }

    void HandleJump()
    {
        if (!Profile.juice.squashAndStretch) return;
        pulseScale = Scaled(Profile.juice.jumpStretch);
    }

    void HandleLand()
    {
        if (!Profile.juice.squashAndStretch) return;
        pulseScale = Scaled(Profile.juice.landSquash);
    }

    void Update()
    {
        if (!Profile.juice.squashAndStretch)
        {
            target.localScale = baseScale;
            return;
        }

        // Two eases: the visible scale chases the pulse, and the pulse decays
        // back to normal. Together that reads as a quick springy pop.
        float k = Profile.juice.recoverSpeed * Time.deltaTime;
        target.localScale = Vector3.Lerp(target.localScale, pulseScale, k);
        pulseScale = Vector3.Lerp(pulseScale, baseScale, k);
    }

    Vector3 Scaled(Vector2 xy) => Vector3.Scale(baseScale, new Vector3(xy.x, xy.y, 1f));
}
