using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundMask;
    public float mouseSensitivity = 2f;
    public float rotationSmoothSpeed = 10f;

    private Rigidbody rb;
    private Collider col;
    private float distToGround;
    private bool isGrounded;
    private bool jumpRequest;
    private float targetYaw;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        distToGround = col.bounds.extents.y;
        targetYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            targetYaw += mouseX;
        }

        Quaternion desiredRot = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmoothSpeed * Time.deltaTime);

        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            distToGround + groundCheckDistance,
            groundMask
        );

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequest = true;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.forward * v + transform.right * h;
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 moveVelocity = moveDir * currentSpeed;
        Vector3 newVelocity = new Vector3(
            moveVelocity.x,
            rb.linearVelocity.y,
            moveVelocity.z
        );

        if (jumpRequest)
        {
            newVelocity.y = jumpForce;
            jumpRequest = false;
        }

        rb.linearVelocity = newVelocity;
    }
}
