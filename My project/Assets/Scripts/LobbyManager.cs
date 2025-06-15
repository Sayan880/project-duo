using UnityEngine;
using System.Collections;
using Unity.Services.Lobbies;

// Verwalter für die Lobby-Funktionalität.
// Diese Klasse sendet regelmäßig ein "Heartbeat"-Signal, damit die Lobby aktiv bleibt.

public class LobbyManager : MonoBehaviour
{
    // Singleton-Instanz, damit nur ein LobbyManager existiert
    public static LobbyManager instance;

    // Die aktuelle Lobby-ID
    public string lobbyID;

    // Stellt sicher, dass nur ein LobbyManager existiert.
    // Objekt bleibt über Szenen hinweg erhalten.
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Falls schon eine Instanz existiert -> zerstören
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Objekt bleibt beim Szenenwechsel erhalten
    }

    // Setzt Singleton-Instanz zurück, wenn dieses Objekt zerstört wird.
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // Startet die regelmäßige Übermittlung von Heartbeat-Pings für die übergebene Lobby.
    public void StartHeartbeat(string lobbyID)
    {
        this.lobbyID = lobbyID;
        StartCoroutine(SendHeartbeatC());
    }

    
    // Stoppt alle Heartbeat-Übertragungen.
    public void StopHeartbeat()
    {
        StopAllCoroutines();
        lobbyID = "";
    }

    // Coroutine: Wartet 15 Sekunden, dann sendet einen Heartbeat.
    // Endlosschleife, um kontinuierlich Heartbeats  alle 15s zu senden.
    private IEnumerator SendHeartbeatC()
    {
        while (true)
        {
            yield return new WaitForSeconds(15);
            SendHeartbeatPing(); // Manuell alle 15 Sekunden Ping senden
        }
    }

    // Sendet asynchron einen Heartbeat-Ping an die Lobby, um sie aktiv zu halten.
    private async void SendHeartbeatPing()
    {
        if (!string.IsNullOrEmpty(lobbyID))
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Fehler beim Heartbeat: {e.Message}");
            }
        }
    }
}
