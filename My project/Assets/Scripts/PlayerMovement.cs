using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _jumpforce = 200;
    [SerializeField] private Rigidbody _rb;
    void Update()
    {
        var vel = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * _speed;
        vel.y = _rb.angularVelocity.y;
        _rb.angularVelocity = vel;

        if (Input.GetKeyDown(KeyCode.Space)) _rb.AddForce(Vector3.up * _jumpforce);
    }
}