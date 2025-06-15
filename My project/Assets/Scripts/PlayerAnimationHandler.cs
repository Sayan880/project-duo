using UnityEngine;
using Unity.Netcode;

// Stellt sicher, dass das GameObject einen Animator und Rigidbody hat
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimationHandler : NetworkBehaviour
{

    public Transform groundCheck; // Position zur Überprüfung, ob der Spieler auf dem Boden ist

    
    public LayerMask groundMask;  // Welche Layer als Boden gelten

    public float groundCheckRadius = 0.2f; // Radius der Kugel, mit der Bodenberührung geprüft wird

    private Animator animator; // Referenz auf Animator-Komponente, steuert Animationen
    private Rigidbody rb;      // Referenz auf Rigidbody, liefert Physik-Daten wie Geschwindigkeit

    private bool wasGrounded = true; // Speichert, ob Spieler im letzten Frame auf dem Boden war

    // Start wird beim ersten Frame aufgerufen, sobald das Objekt aktiv ist
    void Start()
    {
        // Animationen nur für den lokalen Spieler steuern (keine Steuerung für andere Spieler)
        if (!IsLocalPlayer) return;

        // Holen des Animator-Components vom GameObject
        animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
    }

//einmal pro frame
    void Update()
    {
        // Nur für den lokalen Spieler ausführen, wenn Animator und Rigidbody gesetzt sind
        if (!IsLocalPlayer || animator == null || rb == null) return;

        // Prüfen, ob der Spieler auf dem Boden steht, indem eine kleine Kugel am Fuß ausgesendet wird
        bool isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask
        );

        // Eingaben für Bewegung auslesen
        float h = Input.GetAxis("Horizontal"); // Links/Rechts
        float v = Input.GetAxis("Vertical");   // Vorwärts/Rückwärts

        // Prüfen, ob sich der Spieler überhaupt bewegt (Magnitude der Eingabe >= 0.1)
        bool isMoving = new Vector2(h, v).magnitude >= 0.1f;

        // Sprinten, wenn Bewegung und Shift gedrückt wird
        bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);

        // Prüfen, ob die Sprungtaste gerade gedrückt wurde
        bool jumpPressed = Input.GetButtonDown("Jump");

        // Spring-Start: Sprungtaste gedrückt + Spieler steht am Boden
        bool jumpStart = jumpPressed && isGrounded;

        // Landung: Spieler war vorher in der Luft, jetzt aber auf dem Boden
        bool landed = !wasGrounded && isGrounded;

        // Fallen: Spieler ist nicht auf dem Boden und fällt mit negativer Y-Geschwindigkeit
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;

        // Animationsparameter setzen, um passende Animationen abzuspielen
        animator.SetBool("isWalking", isMoving && !isSprinting);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isJumpStart", jumpStart);
        animator.SetBool("isJumpEnd", landed);
        animator.SetBool("isFalling", isFalling);

        // Status fürs nächste Frame speichern
        wasGrounded = isGrounded;
    }

    // Zeichnet im Editor eine gelbe Kugel um das GroundCheck-Objekt, um den Prüf-Radius sichtbar zu machen
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
