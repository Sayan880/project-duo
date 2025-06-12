using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<float> Health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone);
    public float MaxHealth = 100f;
    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        Health.Value = Mathf.Max(0, Health.Value - amount);
    }
}
