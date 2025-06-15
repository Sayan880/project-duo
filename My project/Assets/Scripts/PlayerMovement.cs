using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))] // fuer Position/Rotation-Sync
public class PlayerMovement : NetworkBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Ziehen: Hauptkamera (Main Camera) hierher.")]
    public Transform cameraTransform;

    [Tooltip("Ziehen: GroundCheck-Objekt (als Kind-Transform des Spielers), positioniert nahe der Fuesse.")]
    public Transform groundCheck;

    [Tooltip("Legen: Auf welche Layer der Boden gehoert (z.B. Default).")]
    public LayerMask groundMask;

    [Header("Bewegungs-Einstellungen")]
    [Tooltip("Normale Gehgeschwindigkeit (Einheiten pro Sekunde).")]
    public float walkSpeed = 5f;

    [Tooltip("Sprintgeschwindigkeit (Einheiten pro Sekunde).")]
    public float sprintSpeed = 9f;

    [Tooltip("Sprunghoehe (ungefuehr, in Unity-Einheiten).")]
    public float jumpHeight = 1.5f;

    [Tooltip("Wie schnell sich der Spieler zur Bewegungsrichtung dreht.")]
    public float rotationSpeed = 10f;

    [Tooltip("Radius fuer die Boden-Abfrage (GroundCheck).")]
    public float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        StartCoroutine(TryAssignCamera());
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        if (IsLocalPlayer)
            Debug.Log("Ich bin der lokale Spieler: " + OwnerClientId);
        else
            Debug.Log("Ich bin NICHT der lokale Spieler: " + OwnerClientId);
    }

    IEnumerator TryAssignCamera()
    {   
        yield return new WaitForSeconds(0.1f);  
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            Debug.Log("CameraTransform gesetzt");
        }
        else
        {
            Debug.LogWarning("Keine Kamera mit MainCamera-Tag gefunden!");
        }
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = camRight * horizontalInput + camForward * verticalInput;
        moveDirection.Normalize();

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (moveDirection.sqrMagnitude >= 0.01f)
        {
            Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(Physics.gravity.y));
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpVelocity;
            rb.linearVelocity = vel;
        }
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer) return;

        Vector3 velocity = rb.linearVelocity;
        Vector3 targetVel = moveDirection * currentSpeed;
        targetVel.y = velocity.y;
        rb.linearVelocity = targetVel;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
