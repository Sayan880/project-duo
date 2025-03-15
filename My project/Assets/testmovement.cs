using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;

 [RequireComponent (typeof(Rigidbody))]
 public class PlayerController : MonoBehaviour {

     public float movementSpeed = 5.0f;

     void Start () {
         
     }
     
     void Update () {

        Rigidbody rigidbody = GetComponent<Rigidbody>();

         if(Input.GetKey(KeyCode.W)) {
             transform.position += Vector3.forward * Time.deltaTime * movementSpeed;
         }
         else if(Input.GetKey(KeyCode.S)) {
             rigidbody.position += Vector3.back * Time.deltaTime * movementSpeed;
         }
         else if(Input.GetKey(KeyCode.A)) {
             rigidbody.position += Vector3.left * Time.deltaTime * movementSpeed;
         }
         else if(Input.GetKey(KeyCode.D)) {
             rigidbody.position += Vector3.right * Time.deltaTime * movementSpeed;
         }
     }
 }

