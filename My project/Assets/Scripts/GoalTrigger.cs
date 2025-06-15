using UnityEngine;
using Unity.Netcode;

public class GoalTrigger : MonoBehaviour
{
    public int requiredPlayerId;

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        var player = other.GetComponent<PlayerHandler>();
        if (player != null && player.PlayerID.Value == requiredPlayerId)
        {
            LevelEndManager.Instance.PlayerReachedGoalServerRpc(requiredPlayerId);
        }
    }
}