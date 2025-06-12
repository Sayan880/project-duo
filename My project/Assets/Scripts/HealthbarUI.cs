using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class HealthbarUI : MonoBehaviour
{
    public Slider P1HPBar;
    public Slider P2HPBar;
    void Start()
    {
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();

        foreach (var player in allPlayers)
        {
            if (player.IsOwner)
            {
                
            }
            else
            {
                
            }
        }
    }
}
