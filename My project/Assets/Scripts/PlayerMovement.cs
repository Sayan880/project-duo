using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Transform der Kamera (wird fuer die Bewegungsrichtung genutzt)")]
    public Transform cameraTransform;

    [Header("Bewegung")]
    [Tooltip("Grund-Geschwindigkeit in Einheiten/Sekunde")]
    public float walkSpeed = 5f;
    [Tooltip("Multiplikator, wenn Shift gedrueckt wird")]
    public float sprintMultiplier = 2f;

    [Header("Sprung")]
    [Tooltip("Kraft des Sprungs")]
    public float jumpForce = 5f;
    [Tooltip("Wie weit nach unten fuer Bodenkontakt pruefen")]
    public float groundCheckDistance = 0.1f;
    [Tooltip("Layer, die als Boden gelten")]
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 horizontalVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance + 0.01f,
            groundMask
        );

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.zero;
        Vector3 camRight = Vector3.zero;
        if (cameraTransform != null)
        {
            camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();
        }
        else
        {
            camForward = Vector3.forward;
            camRight = Vector3.right;
        }

        Vector3 moveDir = camForward * v + camRight * h;
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= sprintMultiplier;
        }
        Vector3 desiredMove = moveDir * currentSpeed;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }

        horizontalVelocity = new Vector3(desiredMove.x, rb.linearVelocity.y, desiredMove.z);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = horizontalVelocity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position;
        Vector3 dir = Vector3.down * (groundCheckDistance + 0.01f);
        Gizmos.DrawLine(origin, origin + dir);
        Gizmos.DrawSphere(origin + dir, 0.05f);
    }
}