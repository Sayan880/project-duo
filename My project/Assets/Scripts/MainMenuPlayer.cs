using UnityEngine;
using Mirror;
public class MainMenuPlayer : NetworkBehaviour
{
    public static Player localPlayer;
    public void Start(){
        if (isLocalPlayer){
            localPlayer = this;
        }
    }

    public void HostGame(){
       string matchID = MatchMaker.GetRandomMatchID();
       CmdHostGame(matchID)
    }

    [Command]
    void CmdHostGame(string _matchID){
        if(MatchMaker.instance.HostGame(_matchID, gameObject)){
            Debug.Log($"Hosting success");
        }
        else{
            Debug.Log($"Hosting fail");
        }
    }
}
