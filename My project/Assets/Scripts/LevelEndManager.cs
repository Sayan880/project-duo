using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

// Verwalter für das Levelende – wechselt zur nächsten Szene, wenn beide Spieler das Ziel erreicht haben.

public class LevelEndManager : NetworkBehaviour
{
    public static LevelEndManager Instance;

    // Netzwerkvariablen: Merken, ob Spieler 1 und 2 das Ziel erreicht haben
    private NetworkVariable<bool> player1Reached = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> player2Reached = new NetworkVariable<bool>(false);

    private void Awake()
    {
        // Singleton-Zuweisung
        if (Instance == null) Instance = this;
    }

    // Wird vom Server aufgerufen, wenn ein Spieler das Ziel erreicht hat.
    [ServerRpc(RequireOwnership = false)]
    public void PlayerReachedGoalServerRpc(int playerId)
    {
        if (playerId == 0)
            player1Reached.Value = true;
        else if (playerId == 1)
            player2Reached.Value = true;

        CheckLevelEnd();
    }

    // Prüft, ob beide Spieler das Ziel erreicht haben und wechselt ggf. das Level.
    private void CheckLevelEnd()
    {
        if (player1Reached.Value && player2Reached.Value)
        {
            LoadNextLevel();
        }
    }

    // Lädt das nächste Level (nächste Szene im Build-Index) oder zurück zur Lobby.
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                SceneManager.GetSceneByBuildIndex(nextSceneIndex).name,
                LoadSceneMode.Single);
        }
        else
        {
            // Kein weiteres Level: Zurück zur Lobby
            NetworkManager.Singleton.SceneManager.LoadScene(
                "LobbyScene",
                LoadSceneMode.Single);
        }
    }
}
