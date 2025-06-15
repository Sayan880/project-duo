using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    public Transform cameraTransform;
    public Transform groundCheck;
    public LayerMask groundMask;

    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.5f;
    public float rotationSpeed = 10f;
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
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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
        }
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        if (Physics.SphereCast(groundCheck.position, groundCheckRadius, Vector3.down, out RaycastHit hitInfo, 0.1f, groundMask))
        {
            isGrounded = true;

            if (hitInfo.collider.gameObject.name == "Cloud Layer")
            {
                ReloadSceneIfOwner();
            }
        }
        else
        {
            isGrounded = false;
        }

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

    private void ReloadSceneIfOwner()
    {
        if (IsOwner)
        {
            ReloadSceneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReloadSceneServerRpc()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
