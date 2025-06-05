using UnityEngine;
using Unity.Netcode;

public class PlayerHandler : NetworkBehaviour
{
    //public GameObject cam;
    public Material pm;
    public PlayerMovement move;
    public NetworkVariable<int> PlayerID = new NetworkVariable<int>();
    public static int pCount;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerID.Value = pCount;
            pCount++;
        }

        if (IsLocalPlayer)
        {
            move.enabled = true;
            GetComponent<SkinnedMeshRenderer>().material = Material.Instantiate(pm);
            GetComponent<SkinnedMeshRenderer>().material.color = Color.green;
            //cam.SetActive(true);

        }
        else
        {
            Destroy(move);
            //Destroy(cam);
            GetComponent<SkinnedMeshRenderer>().material = Material.Instantiate(pm);
            GetComponent<SkinnedMeshRenderer>().material.color = Color.red;

        }
    }

    public override void OnDestroy()
    {
        NetworkObject.Despawn(true);
        
    }
    public override void OnNetworkDespawn()
    {
        NetworkObject.Despawn();
    }
}