using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(7f, 4f, 0f);
    [SerializeField] float smoothTime = 0.3f; // seconds
    [SerializeField] float lookAhead = 0.35f; // seconds of the player's velocity

    Rigidbody targetBody;
    Vector3 aim;
    Vector3 velocity;

    void Awake()
    {
        if (target == null)
            target = FindAnyObjectByType<PlayerController>().transform;
        targetBody = target.GetComponent<Rigidbody>();
        aim = target.position;
    }

    void LateUpdate()
    {
        // aim at where the player will be, not where they are
        Vector3 flatVelocity = targetBody.linearVelocity;
        flatVelocity.y = 0f;
        Vector3 wantedAim = target.position + flatVelocity * lookAhead;

        // smooth the aim point itself, so a sudden turn doesn't snap the camera round
        aim = Vector3.SmoothDamp(aim, wantedAim, ref velocity, smoothTime);
        transform.position = aim + offset;
        transform.LookAt(aim);
    }
}
