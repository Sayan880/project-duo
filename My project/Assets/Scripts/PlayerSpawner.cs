using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    private Transform spawnPointPlayer1;
    private Transform spawnPointPlayer2;

    private List<ulong> playersIds = new List<ulong>();

    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
    }

    public void SetPlayers(List<ulong> playerIds)
    {
        playersIds = playerIds;
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            spawnPointPlayer1 = GameObject.Find("StartPlayer1")?.transform;
            spawnPointPlayer2 = GameObject.Find("StartPlayer2")?.transform;

            if (spawnPointPlayer1 != null && spawnPointPlayer2 != null)
            {
                SpawnPlayers();
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
            else
            {
                Debug.LogError("Spawnpunkte nicht gefunden! 'StartPlayer1' und 'StartPlayer2' müssen in der Szene existieren.");
            }
        }
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < playersIds.Count; i++)
        {
            GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);

            if (i == 0) player.transform.position = spawnPointPlayer1.position + Vector3.up * 2f;
            else if (i == 1) player.transform.position = spawnPointPlayer2.position + Vector3.up * 2f;
            else player.transform.position = Vector3.zero; // fallback Position

            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playersIds[i]);
        }
    }
}
