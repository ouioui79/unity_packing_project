﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScene : MonoBehaviour
{
    GameObject[] initialObjects = new GameObject[1];
    Vector3[] originalPosition = new Vector3[23];
    Vector3[] originalRotation = new Vector3[23];

    void Start()
    {
        //Fetch the Collider from the GameObject this script is attached to
        initialObjects = GameObject.FindGameObjectsWithTag("Interactable");
        for (int i = 0; i < initialObjects.Length; i++)
        {
            //originalPosition[i] = initialObjects[i].transform.position;
            //originalRotation[i] = initialObjects[i].transform.eulerAngles;
        }
        //Assign the point to be that of the Transform you assign in the Inspector window
     
    }

    void ResetInteractables()
    {
        for (int i = 0; i < initialObjects.Length; i++)
        {
            initialObjects[i].transform.position = originalPosition[i];
            initialObjects[i].transform.eulerAngles = originalPosition[i];
        }
    }
}
