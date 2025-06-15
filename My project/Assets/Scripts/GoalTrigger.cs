using UnityEngine;
using Unity.Netcode;

// Überprüft, ob ein bestimmter Spieler das Ziel erreicht.
public class GoalTrigger : MonoBehaviour
{
    [Tooltip("Spieler-ID, die dieses Ziel erreichen muss (z.B. 0 oder 1).")]
    public int requiredPlayerId;

    private void OnTriggerEnter(Collider other)
    {
        // Nur der Server verarbeitet Zielerreichung
        if (!NetworkManager.Singleton.IsServer) return;

        // Prüft, ob das kollidierende Objekt ein Spieler ist und ob es die nötige Player ID hat
        var player = other.GetComponent<PlayerHandler>();
        if (player != null && player.PlayerID.Value == requiredPlayerId)
        {
            // Benachrichtigt den LevelEndManager, dass ein Spieler mit der benötigten ID das Ziel erreicht hat
            LevelEndManager.Instance.PlayerReachedGoalServerRpc(requiredPlayerId);
        }
    }
}
