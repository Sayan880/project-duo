using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float rotationSpeed = 5f;
    public float minYAngle = -20f;
    public float maxYAngle = 80f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 position = target.position + rotation * new Vector3(0f, 0f, -distance);
            transform.rotation = rotation;
            transform.position = position;
        }
    }
}
