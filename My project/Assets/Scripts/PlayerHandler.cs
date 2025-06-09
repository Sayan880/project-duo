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
            transform.Find("HumanM_BodyMesh").GetComponent<SkinnedMeshRenderer>().material = Material.Instantiate(pm);
            transform.Find("HumanM_BodyMesh").GetComponent<SkinnedMeshRenderer>().material.color = Color.green;
            //cam.SetActive(true);

        }
        else
        {
            if (move != null)
                move.enabled = false;
            //Destroy(cam);
            transform.Find("HumanM_BodyMesh").GetComponent<SkinnedMeshRenderer>().material = Material.Instantiate(pm);
            transform.Find("HumanM_BodyMesh").GetComponent<SkinnedMeshRenderer>().material.color = Color.red;

        }
    }

    void Awake()
    {
        if (move == null)
            move = GetComponent<PlayerMovement>();
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