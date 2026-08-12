using System;
using UnityEngine;

/// <summary>
/// Owns vertical movement and jump feel. Reads its numbers from a JumpProfile
/// (the data) and keeps only runtime state here (grounded flag, mid-jump flag).
///
/// It runs a custom gravity model instead of Unity's built-in gravity: a good jump
/// needs the rise and the fall to obey different rules (you fall faster than you
/// rose), which one global gravityScale can't do. So Awake sets gravityScale = 0
/// and ApplyGravity adds our own each physics step, heavier on the way down.
///
/// Fires OnJump / OnLand so cosmetic systems (JumpJuice) can react without this
/// file knowing sprites exist.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JumpController : MonoBehaviour
{
    [Tooltip("The tuning asset. If left empty, a default is created at runtime so this still runs.")]
    public JumpProfile profile;

    [Header("Ground Check (a box probe under the feet)")]
    [Tooltip("Size of the box we test for ground.")]
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.15f);
    [Tooltip("Where the feet are, relative to this object's center.")]
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    [Tooltip("Which physics layers can be ground. Default = Everything; we also require the 'Ground' tag.")]
    public LayerMask groundLayers = ~0;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;

    // ---- events other systems can subscribe to (decoupling) ----
    public event Action OnJump;
    public event Action OnLand;

    // ---- runtime STATE only (never tuning values) ----
    Rigidbody2D rb;
    bool isGrounded;
    bool wasGrounded;
    bool isJumping;

    // ---- values DERIVED from the designer-friendly arc settings ----
    // With constant gravity g, a launch velocity v reaches height h = v^2 / (2g)
    // after time t = v / g. Solving for g and v given the height and time the
    // designer typed:
    //     g = 2h / t^2      v = 2h / t
    float ApexTime    => Mathf.Max(0.01f, profile.arc.timeToApex); // guard divide-by-zero
    float BaseGravity => 2f * profile.arc.jumpHeight / (ApexTime * ApexTime);
    float JumpVelocity => 2f * profile.arc.jumpHeight / ApexTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Fallback so the component always works even before you make an asset.
        if (profile == null)
            profile = ScriptableObject.CreateInstance<JumpProfile>();

        rb.gravityScale = 0f;   // WE do gravity now, not Unity.
        rb.freezeRotation = true;
    }

    // Input is read in Update (runs every rendered frame = most responsive).
    void Update()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
            DoJump();

        if (Input.GetKeyUp(jumpKey))
            ApplyJumpCut(); // releasing early shortens the hop (variable jump height)
    }

    // Physics is done in FixedUpdate (runs on a fixed clock = stable simulation).
    void FixedUpdate()
    {
        UpdateGrounded();
        ApplyGravity();
    }

    // ------------------------------------------------------------------ ground
    void UpdateGrounded()
    {
        Vector2 center = (Vector2)transform.position + groundCheckOffset;
        Vector2 size = groundCheckSize + Vector2.up * profile.assist.groundedTolerance;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, groundLayers);

        bool touchingGround = false;
        foreach (var h in hits)
        {
            if (h.attachedRigidbody == rb) continue; // ignore our own collider
            if (h.CompareTag("Ground")) { touchingGround = true; break; }
        }

        // Only count as grounded when we're NOT moving upward. Otherwise the box
        // still overlaps the floor for a frame right after jumping and would
        // instantly cancel the jump.
        isGrounded = touchingGround && rb.linearVelocity.y <= 0.01f;

        if (isGrounded)
        {
            isJumping = false;
            if (rb.linearVelocity.y < 0f) // stop custom gravity building up while resting
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        if (isGrounded && !wasGrounded) // the moment of touchdown
            OnLand?.Invoke();
        wasGrounded = isGrounded;
    }

    // ---------------------------------------------------------------- gravity
    void ApplyGravity()
    {
        if (isGrounded) return; // resting on the floor: no gravity needed

        float vy = rb.linearVelocity.y;
        // Rise at base gravity, fall heavier. This asymmetry IS the jump's feel.
        float mult = vy > 0f ? 1f : profile.gravity.fallGravityMult;
        vy -= BaseGravity * mult * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, vy);
    }

    // ------------------------------------------------------------------- jump
    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpVelocity);
        isJumping = true;
        OnJump?.Invoke();
    }

    // Releasing jump early clips the upward velocity for a shorter hop. It must only
    // ever REDUCE the rise: the minJumpHeight floor guarantees a tap still clears a
    // real hop, but if we applied that floor while already rising slower than it, the
    // "cut" would BOOST us — and mashing jump in mid-air would let you fly away.
    void ApplyJumpCut()
    {
        if (!isJumping || rb.linearVelocity.y <= 0f) return;

        float minCutVelocity = Mathf.Sqrt(2f * BaseGravity * profile.arc.minJumpHeight);
        if (rb.linearVelocity.y <= minCutVelocity) return; // already at/under the min hop — nothing to cut

        float cut = Mathf.Max(rb.linearVelocity.y * profile.gravity.jumpCutMultiplier, minCutVelocity);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, cut);
    }

    // Draws the ground-check box in the Scene view when this object is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 center = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(center, groundCheckSize);
    }
}
