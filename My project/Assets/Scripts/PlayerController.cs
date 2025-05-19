using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] private float _speed = 1;

    private void Update() {
        var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        transform.Translate(dir * _speed * Time.deltaTime);
    }
}
