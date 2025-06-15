using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using Unity.Services.Lobbies;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

/*
 Dieses Script verwaltet die Multiplayer-Lobby und Netzwerkverbindungen.
  
 Aufgaben:
 - Spieler authentifizieren und verbinden (Unity Services)
 - Erstellen und Verwalten von Spiel-Lobbies (Host-Seite)
 - Beitreten von Lobbies (Client-Seite)
 - Überwachung und Verwaltung der Spieler-Verbindungen
 - Starten des Spiels und Laden der Spielszene
  
 Verwendet Unity Services (Lobbies, Relay, Authentication) und Unity Netcode für Multiplayer.
 Die Debug Logs und System Exceptions waren nötig um Fehler während des Codens einfach herauszulesen.
 */
public class Matchmaker : NetworkBehaviour
{
    [Header("UI Sachen")]
    public TMP_InputField JoinKeyInput, ShowJoinKey;
    public GameObject WaitHost, WaitClient;
    public TextMeshProUGUI playerCount;
    public TMP_InputField lobbyNameInput, DebugLog;

    private static UnityTransport trans;

    [Header("Einstellungen")]
    public string PlayerId { get; private set; }
    public int maxPlayer = 2;
    public string LobbyId;
    public string LobbyName = "LobbyName";
    public string JoinKey = "jointKey";
    public string mapId = "Level1";

    private List<ulong> playersIds = new List<ulong>();

    private async void Start()
    {
        await Login(); // Automatischer Login bei Spielstart
    }

    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines(); // Beendet alle laufenden Prozesse
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    // Meldet den Spieler anonym bei Unity-Services an, speichert deren ID, schaut das nur angemeldete Spieler interagieren
    public async Task Login()
    { //Initialisiere UnityServices
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var Options = new InitializationOptions();
            Options.SetProfile(UnityEngine.Random.Range(0, 100000) + "profile");
            await UnityServices.InitializeAsync(Options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            PlayerId = AuthenticationService.Instance.PlayerId;
        }

        Debug.Log("Player Id" + PlayerId);
        DebugLog.text = "PlayerId: " + PlayerId;
    }

    // Wird aufgerufen, wenn sich ein Client verbindet, Client wird zur Spielerliste hinzugefügt
    private void OnClientConnect(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.Log("Client joined, ID:" + clientId);
            playersIds.Add(clientId);
        }
    }

    // Wird aufgerufen, wenn ein Client getrennt wird, gegenteil des verbindens logischerweise
    private async void OnClientDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                Debug.Log($"Player{client.ClientId} is Disconnect");
                Destroy(client.PlayerObject);
            }
        }

        await LobbyService.Instance.RemovePlayerAsync(LobbyId, AuthenticationService.Instance.PlayerId);
    }

    // UI-Button: Lobby hosten und Events registrieren
    public void HostCreateLobby()
    {
        CreateLobby();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    // Startet das Spiel aus der Lobby heraus
    public void GoInPlayModeLobby()
    {
        foreach (ulong pid in playersIds)
        {
            GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList[0].Prefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(pid);//Spieler spawnen
        }

        NetworkManager.Singleton.SceneManager.LoadScene(mapId, UnityEngine.SceneManagement.LoadSceneMode.Single);//In die Scene laden des ersten levels
        StopAllCoroutines(); // Stoppt Lobby-Monitoring
    }

    // UI-Anzeige Spieleranzahl
    void ChangePlayerCount(string count, string max)
    {
        playerCount.text = "Players:" + count + " MaxSlots:" + max;
    }

    // Lobby erstellen, Relay erstellen, UI Sachen
    private async Task CreateLobby()
    {
        DebugLog.text = "Try to create lobby";
        playerCount.text = "Wait for conn";

        try
        {
            WaitHost.SetActive(true);
            LobbyName = lobbyNameInput.text;
            DebugLog.text = "Create Lobby: " + LobbyName + " as host";

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "ID", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, maxPlayer, options);
            LobbyId = lobby.Id;

            if (trans == null)
                trans = NetworkManager.Singleton.GetComponent<UnityTransport>();
            trans.SetHostRelayData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData);

            NetworkManager.Singleton.StartHost();
            ChangePlayerCount(lobby.Players.Count.ToString(), lobby.MaxPlayers.ToString());

            LobbyManager.instance.StartHeartbeat(LobbyId);
            ShowJoinKey.text = LobbyId;
            StartCoroutine(MLC()); // Startet Monitoring-Schleife
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            DebugLog.text = "failed creating lobby";
        }
    }

    // Coroutine: Alle 5 Sekunden Lobby-Status checken
    IEnumerator MLC()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            MonitorLobby();
        }
    }

    // Lobby-Status von Unity-Lobby-API abrufen
    async void MonitorLobby()
    {
        try
        {
            var lobby = await LobbyService.Instance.GetLobbyAsync(LobbyId);
            ChangePlayerCount(lobby.Players.Count.ToString(), lobby.MaxPlayers.ToString());
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("LSE Reason: " + e.Reason + " EXCEPTION: " + e.Message);
        }
    }

    // Lobby schließen, Spieler entfernen, Netzwerk zurücksetzen
    public async void CloseLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
            WaitHost.SetActive(false);
            StopAllCoroutines();
            LobbyManager.instance.StopHeartbeat();
            playersIds.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.Shutdown();
            DebugLog.text = "close lobby as host";
            Debug.Log($"Lobby {LobbyName} closed");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error closing Lobby: {e.Message}");
        }
    }

    // Zeigt den JoinKey in der UI an
    public void onChangeJoinKey()
    {
        ShowJoinKey.text = LobbyId;
    }

    // Tritt einer bestehenden Lobby mit JoinKey bei
    public async void JoinLobby()
    {
        DebugLog.text = "Try Join: " + JoinKeyInput.text;

        try
        {
            if (string.IsNullOrEmpty(JoinKeyInput.text))
            {
                DebugLog.text += "\nNo Lobby Key Input";
                return;
            }

            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(JoinKeyInput.text);

            if (joinedLobby == null)
            {
                DebugLog.text += "No Lobby found for Key: " + JoinKeyInput.text;
                return;
            }

            LobbyId = joinedLobby.Id;
            JoinKey = joinedLobby.Data["ID"].Value;
            DebugLog.text = "connected to Lobby = " + JoinKey;

            var a = await RelayService.Instance.JoinAllocationAsync(JoinKey);

            if (trans == null)
                trans = NetworkManager.Singleton.GetComponent<UnityTransport>();
            trans.SetClientRelayData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData,
                a.HostConnectionData);

            NetworkManager.Singleton.StartClient();

            WaitClient.SetActive(true);
            StartCoroutine(CLSCo()); // Client Lobby State Coroutine

        }
        catch (System.Exception e)
        {
            WaitClient.SetActive(false);
            DebugLog.text += "\nException " + e.Message + " Stacktrace = " + e.StackTrace;
        }
    }

    // die vorher erwähnte coroutine, ein cooldown
    IEnumerator CLSCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
        }
    }

    // Überprüft, ob Lobby noch existiert
    private async void CheckLobbyState()
    {
        try
        {
            await LobbyService.Instance.GetLobbyAsync(LobbyId);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                WaitClient.SetActive(false);
                DebugLog.text += "Lobby closed";
                Debug.Log("Lobby closed " + AuthenticationService.Instance.PlayerId);
                StopAllCoroutines();
            }
        }
    }
}
