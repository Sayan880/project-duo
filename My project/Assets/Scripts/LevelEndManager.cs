using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LevelEndManager : NetworkBehaviour
{
    public static LevelEndManager Instance;

    private NetworkVariable<bool> player1Reached = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> player2Reached = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReachedGoalServerRpc(int playerId)
    {
        if (playerId == 0)
            player1Reached.Value = true;
        else if (playerId == 1)
            player2Reached.Value = true;

        CheckLevelEnd();
    }
    
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
           NetworkManager.Singleton.SceneManager.LoadScene(
                "LobbyScene",
                LoadSceneMode.Single);
        }
}

    private void CheckLevelEnd()
    {
        if (player1Reached.Value && player2Reached.Value)
        {
            LoadNextLevel();
        }
    }
}
