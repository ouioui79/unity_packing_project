using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetectionOrig : MonoBehaviour
{
    [SerializeField]
    private bool colliding = false;

    [SerializeField]
    private float moveSpeedx = 0f;
    private float moveSpeedy = 0f;
    private float moveSpeedz = 0f;

    private string state = "y";

    public Vector3 originalPosition;
    public Vector3 previousPosition;
    public bool done = false;

    private int iteration = 0;
    private const int MAX_ITERATION = 10;

    private int collitionCount = 0;

    void OnEnable()
    {
        Debug.Log("ENABLE: " + this.gameObject.name);
    }

    void OnCollisionEnter(Collision other)
    {
        if (!enabled)
        {
            return;
        }

        colliding = true;
        collitionCount++;

        if (state == "y")
        {
            Debug.Log("COLLISION ENTER y");
            moveSpeedy = 0.01f;
        }
        else if (state == "x")
        {
            Debug.Log("COLLISION ENTER x");
            moveSpeedx = 0.01f;
            
        }
        else if (state == "z")
        {
            Debug.Log("COLLISION ENTER z");
            moveSpeedz = 0.01f;
        }
        Debug.Log("COLLISION ENTER -- THIS: " + this.gameObject.name + ", THAT: " + other.gameObject.name + " COUNT: " + collitionCount);
    }

    
    void OnCollisionExit(Collision other)
    {
        if (!enabled)
        {
            return;
        }
 
        collitionCount--;

        Debug.Log(gameObject.name + " EXIT from " + other.gameObject.name + " count is " + collitionCount);

        if (collitionCount == 0)
        {
            colliding = false;
        } else
        {
            return;
        }

        if (state == "y")
        {
           
            moveSpeedy = 0f;
            /*state = "x";
            transform.position = new Vector3(xyz_orig.x, transform.position.y, transform.position.z);*/
            Debug.Log("COLLISION EXIT from y, position: " + transform.position);
        }
        else if (state == "x")
        {
            
            moveSpeedx = 0f;
            /*state = "z";
            transform.position = new Vector3(transform.position.x, transform.position.y, xyz_orig.z);*/
            Debug.Log("COLLISION EXIT from x" + transform.position);
        }
        else if (state == "z")
        {
            
            moveSpeedz = 0f;
            /*state = "Done";
            xyz_fix = this.transform.position; */
            Debug.Log("COLLISION EXIT from z" + transform.position);
        }
    }
    
    public bool CurrentlyColliding()
    {
        return colliding;
    }

    void Update()
    {
        if (!enabled)
        {
            return;
        }

        if (state == "y" && moveSpeedy == 0)
        {
            Debug.Log("UPDATE y to x");
            state = "x";
            transform.position = new Vector3(originalPosition.x, transform.position.y, transform.position.z);
        }
        else if (state == "x" && moveSpeedx == 0)
        {
            Debug.Log("UPDATE x to z");
            state = "z";
            transform.position = new Vector3(transform.position.x, transform.position.y, originalPosition.z);

        }
        else if (state == "z" && moveSpeedz == 0)
        {
            Debug.Log("UPDATE z");
            if (
                iteration == MAX_ITERATION ||
                previousPosition.ToString("f3") == transform.position.ToString("f3") ||
                gameObject.name == "object3"
            )
            {
                state = "Done";
                done = true;
                enabled = false;
                // Debug.Log("Last iteration" + iteration);
            }
            else
            {
                // Debug.Log("Iteration number" + iteration);
                state = "y";
                previousPosition = transform.position;
                transform.position = new Vector3(transform.position.x, originalPosition.y, transform.position.z);
                iteration++;
            }
        }

        if (!done)
        {
            Vector3 transformation = new Vector3(moveSpeedx, moveSpeedy, moveSpeedz);
            // Debug.Log(gameObject.name + " add vector: " + transformation.ToString("f3"));
            transform.position = transform.position + transformation;
        }
    }
}
