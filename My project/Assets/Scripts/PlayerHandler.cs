using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerHandler : NetworkBehaviour
{
    // Referenz auf das Standard-Material, das für den Spieler verwendet wird
    public Material pm;

    // Referenz auf das PlayerMovement-Skript (Steuerung)
    public PlayerMovement move;

    public Vector3 spawnPosition;

    // Netzwerkvariable für die Spieler-ID (wird vom Server vergeben)
    public NetworkVariable<int> PlayerID = new NetworkVariable<int>();

    // Zählt die Anzahl der Spieler, um IDs zu vergeben (statisch, damit alle Instanzen darauf zugreifen)
    public static int pCount;

    // Netzwerkvariable für die Spielerfarbe, die vom Server gesetzt und an alle Clients synchronisiert wird
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white, 
        NetworkVariableReadPermission.Everyone, // Jeder darf lesen
        NetworkVariableWritePermission.Server // Nur Server darf schreiben
    );

    // Wird aufgerufen, wenn das Netzwerk-Objekt gespawnt wird (Instanz wird aktiv)
    public override void OnNetworkSpawn()
    {
        // Registrierung eines Event-Handlers, damit Farbänderungen bei Clients aktualisiert werden
        playerColor.OnValueChanged += OnColorChanged;

        // Nur der Server darf Spielern eine ID und Farbe zuweisen sowie Position bestimmen
        if (IsServer)
        {
            // Vergibt die nächste freie Spieler-ID
            PlayerID.Value = pCount++;

            // Weist Spieler 0 die Farbe Grün, Spieler 1 die Farbe Rot zu
            playerColor.Value = (PlayerID.Value == 0) ? Color.green : Color.red;

            // Setzt die Startposition je nach Spieler-ID
            transform.position = (PlayerID.Value == 0)
                ? new Vector3(-9.0f, -2.0f, -22.0f)
                : new Vector3(-14.0f, -2.0f, -22.0f);
        }

        // Umbenennen der einzelnen Spieler
        gameObject.name = $"Player{PlayerID.Value + 1}";

        // Aktiviert die Steuerung nur für den lokalen Spieler (der das Spielobjekt besitzt)
        if (IsOwner)
        {
            move.enabled = true;

            // Falls noch keine Kamera referenziert wurde, versucht die MainCamera zu holen(war später nicht mehr nötig ist aber aus Sicherheitsgründen drin)
            if (move.cameraTransform == null && Camera.main != null)
            {
                move.cameraTransform = Camera.main.transform;
            }
        }
        else
        {
            // Deaktiviert Steuerung bei allen anderen Spielern um das kontrollieren des anderen zu verhindern
            move.enabled = false;
        }

        // Wendet die Farbe(rot oder grün) an
        ApplyColor(playerColor.Value);
    }

    // Event-Handler, der bei Änderung der Farbe aufgerufen wird
    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    // Wendet eine Farbe auf das Material des Spieler-Modells an
    private void ApplyColor(Color color)
    {
        // Findet das Mesh-Renderer-Objekt des Spielers
        var renderer = transform.Find("HumanM_BodyMesh")?.GetComponent<SkinnedMeshRenderer>();
        if (renderer == null) return; // Falls nicht gefunden, abbrechen

        // Klont das Standard-Material und setzt die Farbe
        var mat = Material.Instantiate(pm);
        mat.color = color;

        // Setzt das Material auf den Renderer
        renderer.material = mat;
    }

    // Wird aufgerufen, wenn das Spielobjekt zerstört wird (z.B. beim Szenenwechsel)
    public override void OnDestroy()
    {
        // Entfernt den Event-Handler, um Speicherlecks zu vermeiden
        if (playerColor != null)
        {
            playerColor.OnValueChanged -= OnColorChanged;
        }
        base.OnDestroy();
    }
}
