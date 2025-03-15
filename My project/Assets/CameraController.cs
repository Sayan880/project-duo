using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;



public class CameraController : NetworkBehaviour
{
    public GameObject camera;
    public override void OnStartAuthority()
    {
        camera.SetActive(true);
    }
}
