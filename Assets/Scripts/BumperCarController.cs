
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BumperCarController : MonoBehaviour
{   [SerializeField] private bool isPlayerControlled = true;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float grip = 4f;

    // Matches the imported model's working orientation.
    [SerializeField] private Vector3 localForward = Vector3.up;
    

    [Header("Falling")]
    [SerializeField] private float fallSpinSpeed = 250f;
    [SerializeField] private float fallDownForce = 10f;

    private bool hasFallen = false;
    private float fallDirection = 1f;
    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints |= RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        if (!isPlayerControlled)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        moveInput = -Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");
    }
     




    private void FixedUpdate()
    {   
        if (hasFallen)
        {
            // Keep spinning continuously.
            float turn = fallDirection * fallSpinSpeed
                    * Time.fixedDeltaTime;

            rb.MoveRotation(
                Quaternion.AngleAxis(turn, Vector3.up) * rb.rotation
            );

            // Push downward while gravity continues working.
            rb.AddForce(
                Vector3.down * fallDownForce,
                ForceMode.Acceleration
            );

            return;
        }
        Vector3 forward =
            transform.TransformDirection(-localForward);

        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

        // Forward and backward movement.
        rb.AddForce(
            -forward * moveInput * acceleration,
            ForceMode.Acceleration
        );

        // Cap horizontal speed.
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(
            velocity.x, 0f, velocity.z
        );

        if (horizontal.magnitude > maxSpeed)
        {
            horizontal = horizontal.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                horizontal.x,
                velocity.y,
                horizontal.z
            );
        }

        // Reduce sideways sliding.
        Vector3 forwardDirection = forward;

        Vector3 lateralVelocity = Vector3.ProjectOnPlane(
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            ),
            forwardDirection
        );

        rb.AddForce(
            -lateralVelocity * grip,
            ForceMode.Acceleration
        );

        // Steering works even while stationary.
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turn =
                turnInput * turnSpeed * Time.fixedDeltaTime;

            rb.MoveRotation(
                Quaternion.AngleAxis(turn, Vector3.up)
                * rb.rotation
            );
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform")
            && !hasFallen)
        {
            hasFallen = true;

            // Randomly spin left or right.
            fallDirection = Random.value < 0.5f ? -1f : 1f;
        }
    }
}
