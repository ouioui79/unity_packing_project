using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLFPackingOrig : MonoBehaviour
{
    public GameObject box;

    private int index = 0;
    private string state = "POP_OBJECT";

    private List<GameObject> objects;
    private GameObject current_object;

    // Start is called before the first frame update
    void Awake()
    {
        objects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
        foreach (GameObject obj in objects)
        {
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            obj.GetComponent<CollisionDetectionOrig>().enabled = false;
        }
    }

    void Update()
    {
        if (state == "POP_OBJECT")
        {
            if (index == objects.Count)
            {
                state = "DONE";
                return;
            }
            current_object = objects[index];
            Debug.Log("POPPING OUT OBJECT: " + current_object.gameObject.name);
            state = "WORK_ON_CURRENT_OBJECT";
            index++;

            float x;
            float y;
            float z;
            float orig_x;
            float orig_y;
            float orig_z;

            Vector3 box_blfposition = box.GetComponent<BoxCollider>().transform.position;
            Vector3 box_size = box.GetComponent<BoxCollider>().size;

            x = box_blfposition.x + box_size.x / 2 - current_object.GetComponent<MeshFilter>().sharedMesh.bounds.size.x / 2-0.05f ;
            z = box_blfposition.z + box_size.z / 2 - current_object.GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2 -0.05f;
            //orig_x = box_blfposition.x - box_size.x/2 + current_object.GetComponent<MeshFilter>().sharedMesh.bounds.size.x/2+ 0.05f;
            //orig_y = box_blfposition.y - box_size.y/2 + current_object.GetComponent<MeshFilter>().sharedMesh.bounds.size.y/2+0.05f;
            //orig_z = box_blfposition.z - box_size.z/2 + current_object.GetComponent<MeshFilter>().sharedMesh.bounds.size.z/2+0.05f;
            orig_x = box_blfposition.x - box_size.x / 2;
            orig_y = box_blfposition.y - box_size.y / 2;
            orig_z = box_blfposition.z - box_size.z / 2;
           
            y = orig_y;
            // Debug.Log("Orig location" + orig_x + "," +orig_y + ","+ orig_z);
            // Debug.Log("POP location" + x +"," + y +"," + z);

            current_object.transform.position = new Vector3(x, y, z);
            current_object.GetComponent<CollisionDetectionOrig>().originalPosition = new Vector3(orig_x, orig_y, orig_z);
            current_object.GetComponent<CollisionDetectionOrig>().enabled = true;
        }
        else if (state == "WORK_ON_CURRENT_OBJECT")
        {
            // Debug.Log("WORKING ON OBJECT: " + current_object.gameObject.name);
            if (current_object.GetComponent<CollisionDetectionOrig>().done)
            {
                state = "POP_OBJECT";
            }
        }
        else
        {
            Debug.Log("DONE and state should be " + state);
        }
    }

}
