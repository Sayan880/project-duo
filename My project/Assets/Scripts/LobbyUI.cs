using UnityEngine;
using UnityEngine.UI;
public class LobbyUI : MonoBehaviour
{
    [SerializeField] InputField joinInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButton;

    public void Host(){
        joinInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
        MainMenuPlayer.localPlayer.HostGame();
    }

    public void Join(){
        joinInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;
    }
}
