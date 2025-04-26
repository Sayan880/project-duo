using UnityEngine;
using Mirror;
public class MatchMaker : NetworkBehaviour
{   
    [System.Serializable]
    public class Match {
        public string matchID;
        public SyncListGameObject players = new SyncListGameObject ();
        public Match(string matchID, GameObject player){
            this.matchID = matchID;
            players.Add (player);
        }
    
        public Match(){}
    }
    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> {}
     [System.Serializable]
    public class SyncListMatch : SyncList<Match> {}
    public static MatchMaker instance;
    public SyncListMatch matches = new SyncListMatch();
    void Start(){
        instance = this;
    }
    public bool HostGame(string _matchID, GameObject _player) {
        matches.Add (new Match (_matchID, _player));
    }
    public static string GetRandomMatchID(){
        string id = string.Empty;
        for (int i = 0, i < 5, i++){
            int random = Random.Range(0, 9);
            id += random.ToString();
        }
        Debug.Log($"ID is {id}");
        return id;
    }   
}
