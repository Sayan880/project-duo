using UnityEngine;

public class SideScrollerCamera : MonoBehaviour
{
    public string player1Name = "Player1";
    public string player2Name = "Player2";
    private Transform player1;
    private Transform player2;
    public Vector3 offset = new Vector3(-10f, 5f, 0f);
    [Range(0.01f, 1f)] public float smoothSpeed = 0.1f;

    void Start()
    {
        var go1 = GameObject.Find(player1Name);
        var go2 = GameObject.Find(player2Name);
        if (go1 != null) player1 = go1.transform;
        else Debug.LogError($"Spieler '{player1Name}' nicht gefunden!");
        if (go2 != null) player2 = go2.transform;
        else Debug.LogError($"Spieler '{player2Name}' nicht gefunden!");
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 center = (player1.position + player2.position) * 0.5f;
        Vector3 target = new Vector3(center.x + offset.x, center.y + offset.y, center.z + offset.z);
        transform.position = Vector3.Lerp(transform.position, target, smoothSpeed);
        transform.LookAt(center);
    }

    void OnDrawGizmosSelected()
    {
        if (player1 != null && player2 != null)
        {
            Vector3 center = (player1.position + player2.position) * 0.5f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, 0.5f);
        }
    }
}
