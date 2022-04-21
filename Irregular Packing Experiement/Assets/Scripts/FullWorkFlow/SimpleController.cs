using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleController : MonoBehaviour
{
    Rigidbody rb;
    public float speed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");
        transform.Translate(new Vector3(horizontalRaw * speed, 0f, verticalRaw * speed)); //To move 

        if (Input.GetKey(KeyCode.Q)) transform.Rotate(0, 6f, 0); //To rotate 
        else if (Input.GetKey(KeyCode.E)) transform.Rotate(0, -6f, 0);
    }
}
