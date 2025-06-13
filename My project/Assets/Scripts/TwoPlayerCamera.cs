using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TwoPlayerCamera : MonoBehaviour
{
    public string player1Name = "Player1";
    public string player2Name = "Player2";

    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    public Vector3 offset = new Vector3(0, 0, -10);

    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomLimiter = 10f;

    private Camera cam;
    private Transform player1;
    private Transform player2;

    void Start()
    {
        cam = GetComponent<Camera>();
        GameObject go1 = GameObject.Find(player1Name);
        GameObject go2 = GameObject.Find(player2Name);

        if (go1 != null) player1 = go1.transform;
        else Debug.LogWarning($"TwoPlayerCamera: '{player1Name}' not found in scene.");

        if (go2 != null) player2 = go2.transform;
        else Debug.LogWarning($"TwoPlayerCamera: '{player2Name}' not found in scene.");

        if (!cam.orthographic)
            Debug.LogWarning("TwoPlayerCamera works best with an orthographic camera for 2D.");
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;
        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 midpoint = (player1.position + player2.position) * 0.5f;
        Vector3 targetPos = midpoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float distance = Vector3.Distance(player1.position, player2.position);
        float targetSize = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (player1 != null && player2 != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 mid = (player1.position + player2.position) * 0.5f;
            Gizmos.DrawSphere(mid, 0.2f);
            Gizmos.DrawLine(player1.position, player2.position);
        }
    }
#endif
}