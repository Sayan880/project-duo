using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlatformMover : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.forward;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = startPosition + moveDirection.normalized * offset;
    }
}

