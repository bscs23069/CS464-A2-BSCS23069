using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 6f;            // metres per second
    [SerializeField] float jumpHeight = 1.8f;     // metres
    [SerializeField] float fallMultiplier = 2.2f; // extra gravity while falling

    [Header("Jump windows")]
    [SerializeField] float coyoteTime = 0.1f;     // seconds after leaving a ledge
    [SerializeField] float jumpBuffer = 0.12f;    // seconds before landing

    [Header("Landing squash")]
    [SerializeField] Transform body;
    [SerializeField] float squashY = 0.7f;
    [SerializeField] float squashDuration = 0.25f;
    [SerializeField] float minAirTime = 0.1f;

    Rigidbody rb;
    InputAction moveAction;
    InputAction jumpAction;

    Vector3 move;
    bool isGrounded, wasGrounded, jumpQueued, rising;
    float lastGroundedTime = -1f;
    float lastPressedTime = -1f;
    float leftGroundTime;
    Vector3 groundPoint, groundNormal = Vector3.up;
    Coroutine squash;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.sleepThreshold = 0f; // a sleeping body stops sending OnCollisionStay

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        move = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

        // coyote time and jump buffer, both as timestamps
        if (isGrounded && !jumpQueued && !rising)
            lastGroundedTime = Time.time;
        if (jumpAction.WasPressedThisFrame())
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;
        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;
            lastPressedTime = -1f;
        }

        // squash once per real landing
        if (!isGrounded && wasGrounded)
            leftGroundTime = Time.time;
        if (isGrounded && !wasGrounded && Time.time - leftGroundTime > minAirTime)
        {
            if (squash != null) StopCoroutine(squash);
            squash = StartCoroutine(Squash());
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;
            rising = true;

            // zero the fall first, or a coyote jump comes out short
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }
        else if (rising && rb.linearVelocity.y <= 0f)
        {
            rising = false;
        }

        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);

        // the collision callbacks after this step set it again
        isGrounded = false;
    }

    void OnCollisionEnter(Collision collision) => CheckGround(collision);
    void OnCollisionStay(Collision collision) => CheckGround(collision);

    void CheckGround(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            // only a surface facing up counts, so a wall isn't ground
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                groundPoint = contact.point;
                groundNormal = contact.normal;
            }
        }
    }

    IEnumerator Squash()
    {
        // keep the volume: x and z grow when y shrinks
        float side = 1f / Mathf.Sqrt(squashY);
        Vector3 squashed = new Vector3(side, squashY, side);

        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);
            SetBodyScale(Vector3.LerpUnclamped(squashed, Vector3.one, p));
            yield return null;
        }

        SetBodyScale(Vector3.one); // exactly at rest
        squash = null;
    }

    void SetBodyScale(Vector3 scale)
    {
        // the capsule is 2 m tall, so move it down to keep the feet on the ground
        body.localScale = scale;
        body.localPosition = new Vector3(0f, scale.y - 1f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(groundPoint, groundNormal);
        Gizmos.DrawWireSphere(groundPoint, 0.1f);
    }
}
