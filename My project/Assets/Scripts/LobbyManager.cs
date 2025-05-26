using UnityEngine;
using System.Collections;
using Unity.Services.Lobbies;
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public string lobbyID;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void StartHeartbeat(string lobbyID) {
        this.lobbyID = lobbyID;
        StartCoroutine(SendHeartbeatC());
    }

    public void StopHeartbeat() {
        StopAllCoroutines();
        lobbyID = "";
    }
    private IEnumerator SendHeartbeatC() {
        while (true)
        {
            yield return new WaitForSeconds(15);
        }
    }

    private async void SendHeartbeatPing() {
        if (!string.IsNullOrEmpty(lobbyID))
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Fehler heartbeat: {e.Message}");
            }
        }   
    }
}
