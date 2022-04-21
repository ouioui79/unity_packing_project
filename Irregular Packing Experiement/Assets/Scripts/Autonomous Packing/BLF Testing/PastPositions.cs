using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastPositions : MonoBehaviour
{
    List<Vector3> pastPositions = new List<Vector3>();
    private float timePassed = 0f;
    private float timeBetweenRecording = 0.1f;

    public bool display = false;

    //to change color
    Renderer rend;
    Color color;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DisplayPositions", 8);

        rend = GetComponent<Renderer>();

        if (this.name == "Cube Object 1") color = Color.red;
        if (this.name == "Cube Object 2") color = Color.blue;
        if (this.name == "Cube Object 3") color = Color.green;

        //rend.material.SetColor("_Color", color);
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        timeBetweenRecording += Time.deltaTime;

        if (timePassed < 3)
        {
            if (timeBetweenRecording > 0.005f)
            {
                StorePosition();
                timeBetweenRecording = 0f;
            }
        }
        
    }

    void DisplayPositions()
    {
        display = true;
        rend.enabled = false;
    }

    void StorePosition()
    {
        pastPositions.Add(transform.position);
    }

    void OnDrawGizmos()
    {
        if (display)
        {
            float cubeSize = 0.1f;
            Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
            foreach (Vector3 pos in pastPositions)
            {
                Gizmos.DrawCube(pos, new Vector3(cubeSize, cubeSize, cubeSize));
            }
        }
    }
}
