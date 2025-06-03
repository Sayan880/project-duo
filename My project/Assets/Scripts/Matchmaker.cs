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


public class Matchmaker : NetworkBehaviour
{

    public TMP_InputField JoinKeyInput, ShowJoinKey;
    public GameObject WaitHost, WaitClient;
    public TextMeshProUGUI playerCount;
    public TMP_InputField lobbyNameInput, DebugLog;

    private static UnityTransport trans;

    public string PlayerId { get; private set; }
    public int maxPlayer = 2;
    public string LobbyId;
    public string LobbyName = "LobbyName";
    public string JoinKey = "jointKey";
    public string mapId = "MainScene";

    private List<ulong> playersIds = new List<ulong>();


    private async void Start()
    {
        await Login();

    }

    private void OnDestroy() {
        try
        {
            StopAllCoroutines();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            
        }
    }
    public async Task Login()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var Options = new InitializationOptions();

            Options.SetProfile(UnityEngine.Random.Range(0, 1000000) + "profile");

            await UnityServices.InitializeAsync(Options);

        }
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            PlayerId = AuthenticationService.Instance.PlayerId;

        }
        Debug.Log("Player Id" + PlayerId);
        DebugLog.text = "PlayerId: " + PlayerId;

    }

    private void OnClientConnect(ulong clientId) {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) {
            Debug.Log("Client joined, ID:" + clientId);
            playersIds.Add(clientId);
        }
    }

    private async void OnClientDisconnect(ulong clientId) {
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

    public void HostCreateLobby() {
        CreateLobby();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public void GoInPlayModeLobby() {
        foreach (ulong pid in playersIds)
        {
            GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList[0].Prefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(pid);

        }

        NetworkManager.Singleton.SceneManager.LoadScene(mapId, UnityEngine.SceneManagement.LoadSceneMode.Single);

        StopAllCoroutines();
    }

    void ChangePlayerCount(string count, string max) {
        playerCount.text = "Players:" + count + "MaxSlots:" + max;
    }

    private async Task CreateLobby() {
        DebugLog.text = "Try to create lobby";
        playerCount.text = "Wait for conn";
        try
        {
            WaitHost.SetActive(true);
            DebugLog.text = "Create Lobby:" + lobbyNameInput.text + "as host";
            LobbyName = lobbyNameInput.text;

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayer);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"ID" , new DataObject(DataObject.VisibilityOptions.Public, joinCode)}
                }
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, maxPlayer, options);
            LobbyId = lobby.Id;

            if (trans == null)
                trans = NetworkManager.Singleton.GetComponent<UnityTransport>();
            trans.SetHostRelayData(a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData);

            NetworkManager.Singleton.StartHost();

            ChangePlayerCount(lobby.Players.Count.ToString(), lobby.MaxPlayers.ToString());

            LobbyManager.instance.StartHeartbeat(LobbyId);
            ShowJoinKey.text = LobbyId;
            StartCoroutine(MLC());


        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            DebugLog.text = "failed creatinglobby";

        }
    }

    IEnumerator MLC() {
        while (true)
        {
            yield return new WaitForSeconds(5);
            MonitorLobby();
        }
    }

    async void MonitorLobby() {
        try
        {
            var lobby = await LobbyService.Instance.GetLobbyAsync(LobbyId);

            ChangePlayerCount(lobby.Players.Count.ToString(), lobby.MaxPlayers.ToString());
        }
        catch (LobbyServiceException e)
        {

            Debug.LogError("LSE Reason: " + e.Reason + "EXCEPTION" + e.Message);
        }
    }

    public async void CloseLobby() {
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

    public void onChangeJoinKey() {
        ShowJoinKey.text = LobbyId;
    }

    public async void JoinLobby() {
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
            DebugLog.text = "connected to Lobby = " + JoinKeyInput.text;

            LobbyId = joinedLobby.Id;

            JoinKey = joinedLobby.Data["ID"].Value;

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
            StartCoroutine(CLSCo());

            DebugLog.text = "Joined Lobby = " + JoinKeyInput.text;


        }
        catch (System.Exception e)
        {
            WaitClient.SetActive(false);
            DebugLog.text += "\nException " + e.Message + " Stacktrace = " + e.StackTrace;

        }
    }
    IEnumerator CLSCo() {
        while (true)
        {
            yield return new WaitForSeconds(5);

        }
    }

    private async void CheckLobbyState() {
        try
        {
            await LobbyService.Instance.GetLobbyAsync(LobbyId);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
            WaitClient.SetActive(false);
                DebugLog.text += "Lobby closeddd";
                Debug.Log("Lobby closed " + AuthenticationService.Instance.PlayerId);

                StopAllCoroutines();
            }
            
        }
     }
}
