using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        dir *= Time.deltaTime * speed;
        transform.position += dir;
    }
}
