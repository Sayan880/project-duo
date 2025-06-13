using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimationHandler : NetworkBehaviour
{
    [Header("Ground Check")]
    [Tooltip("GroundCheck-Objekt (als Kind-Transform des Spielers), nahe den Fuesse.")]
    public Transform groundCheck;
    [Tooltip("LayerMask fuer den Boden (z.B. Default).")]
    public LayerMask groundMask;
    [Tooltip("Radius fuer die Boden-Abfrage.")]
    public float groundCheckRadius = 0.2f;

    Animator animator;
    Rigidbody rb;
    bool wasGrounded = true;

    void Start()
    {

        if (!IsLocalPlayer) return;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        bool isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask
        );

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = new Vector2(h, v).magnitude >= 0.1f;
        bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetButtonDown("Jump");

        bool jumpStart = jumpPressed && isGrounded;
        bool landed = !wasGrounded && isGrounded;
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;

        animator.SetBool("isWalking", isMoving && !isSprinting);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isJumpStart", jumpStart);
        animator.SetBool("isJumpEnd", landed);
        animator.SetBool("isFalling", isFalling);

        wasGrounded = isGrounded;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
