using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))] // Synchronisiert Position und Rotation im Netzwerk
public class PlayerMovement : NetworkBehaviour
{
    [Header("Referenzen")]
    [Tooltip("Main Camera (Ziehen erforderlich, z.B. durch Script automatisch).")]
    public Transform cameraTransform;

    [Tooltip("Transform am Fuß des Spielers, prüft ob Bodenkontakt besteht.")]
    public Transform groundCheck;

    [Tooltip("Layer, die als Boden erkannt werden sollen.")]
    public LayerMask groundMask;

    [Header("Bewegungseinstellungen")]
    [Tooltip("Normale Laufgeschwindigkeit.")]
    public float walkSpeed = 5f;

    [Tooltip("Sprintgeschwindigkeit bei gedrückter Shift-Taste.")]
    public float sprintSpeed = 9f;

    [Tooltip("Höhe des Sprungs in Unity-Einheiten.")]
    public float jumpHeight = 1.5f;

    [Tooltip("Wie schnell sich der Spieler in Bewegungsrichtung dreht.")]
    public float rotationSpeed = 10f;

    [Tooltip("Radius der Bodenerkennung (GroundCheck).")]
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
        StartCoroutine(TryAssignCamera()); // Versucht Kamera zu finden
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Rotation in X und Z verhindern (Spieler kippt nicht um)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (IsLocalPlayer)
            Debug.Log("Ich bin der lokale Spieler: " + OwnerClientId);
        else
            Debug.Log("Ich bin NICHT der lokale Spieler: " + OwnerClientId);
    }

    // Kamera zuweisen, nachdem alle Objekte initialisiert sind
    IEnumerator TryAssignCamera()
    {
        yield return new WaitForSeconds(0.1f);

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            Debug.Log("Kamera erfolgreich zugewiesen.");
        }
        else
        {
            Debug.LogWarning("Keine Kamera mit 'MainCamera'-Tag gefunden!");
        }
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        // Prüft, ob Spieler am Boden ist
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Richtungen basierend auf der Kameraperspektive
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = camRight * horizontalInput + camForward * verticalInput;
        moveDirection.Normalize();

        // Sprint-Check
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Spieler dreht sich zur Bewegungsrichtung
        if (moveDirection.sqrMagnitude >= 0.01f)
        {
            Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        // Springen
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

        // Bewegung umsetzen (inkl. bestehender Y-Geschwindigkeit beim Springen)
        Vector3 velocity = rb.linearVelocity;
        Vector3 targetVel = moveDirection * currentSpeed;
        targetVel.y = velocity.y;
        rb.linearVelocity = targetVel;
    }

    // Zeigt GroundCheck-Radius im Editor an (nur zur Kontrolle)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
