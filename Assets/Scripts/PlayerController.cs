using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 6f;            // metres per second

    Rigidbody rb;
    InputAction moveAction;

    Vector3 move;
    bool isGrounded;
    Vector3 groundPoint, groundNormal = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.sleepThreshold = 0f; // a sleeping body stops sending OnCollisionStay

        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        move = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(groundPoint, groundNormal);
        Gizmos.DrawWireSphere(groundPoint, 0.1f);
    }
}
