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
        player1 = GameObject.Find(player1Name)?.transform;
        player2 = GameObject.Find(player2Name)?.transform;
        if (player1 == null) Debug.LogError($"Spieler '{player1Name}' nicht gefunden!");
        if (player2 == null) Debug.LogError($"Spieler '{player2Name}' nicht gefunden!");
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 center = (player1.position + player2.position) * 0.5f;
        Vector3 targetPos = center + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);

        Vector3 dir = new Vector3(center.x - transform.position.x, 0f, center.z - transform.position.z);
        if (dir.sqrMagnitude > 0.0001f)
        {
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
