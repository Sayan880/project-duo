using UnityEngine;
using Unity.Netcode;

public class PlayerHandler : NetworkBehaviour
{
    public Material pm;
    public PlayerMovement move;

    public Vector3 spawnPosition;

    public NetworkVariable<int> PlayerID = new NetworkVariable<int>();
    public static int pCount;

    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        playerColor.OnValueChanged += OnColorChanged;

        if (IsServer)
        {
            PlayerID.Value = pCount++;
            playerColor.Value = (PlayerID.Value == 0) ? Color.green : Color.red;
        }

        gameObject.name = $"Player{PlayerID.Value + 1}";

        transform.position = spawnPosition;

        if (IsOwner)
        {
            move.enabled = true;

            if (move.cameraTransform == null && Camera.main != null)
            {
                move.cameraTransform = Camera.main.transform;
            }
        }
        else
        {
            move.enabled = false;
        }

        ApplyColor(playerColor.Value);
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color color)
    {
        var renderer = transform.Find("HumanM_BodyMesh")?.GetComponent<SkinnedMeshRenderer>();
        if (renderer == null) return;

        var mat = Material.Instantiate(pm);
        mat.color = color;
        renderer.material = mat;
    }

    public override void OnDestroy()
    {
        if (playerColor != null)
        {
            playerColor.OnValueChanged -= OnColorChanged;
        }
        base.OnDestroy();
    }
}
