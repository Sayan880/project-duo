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
        NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneEventHandler;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneEventHandler;
    }

    public void SetPlayers(List<ulong> playerIds)
    {
        playersIds = playerIds;
    }

    private void SceneEventHandler(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            Debug.Log("Scene loaded: " + sceneEvent.SceneName);

            GameObject sp1 = GameObject.Find("StartPlayer1");
            GameObject sp2 = GameObject.Find("StartPlayer2");

            if (sp1 != null && sp2 != null)
            {
                spawnPointPlayer1 = sp1.transform;
                spawnPointPlayer2 = sp2.transform;
                Debug.Log("Spawnpoints found.");
                SpawnPlayers();
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneEventHandler;
            }
            else
            {
                Debug.LogError("Spawnpoints not found in scene! Make sure objects 'StartPlayer1' and 'StartPlayer2' exist.");
            }
        }
    }

    private void SpawnPlayers()
    {
        for (int i = 0; i < playersIds.Count; i++)
        {
            GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);

            if (i == 0 && spawnPointPlayer1 != null)
                player.transform.position = spawnPointPlayer1.position + Vector3.up * 2f;
            else if (i == 1 && spawnPointPlayer2 != null)
                player.transform.position = spawnPointPlayer2.position + Vector3.up * 2f;
            else
                Debug.LogWarning($"No spawnpoint assigned for player index {i}");

            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playersIds[i]);
        }
    }
}
