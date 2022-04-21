﻿//Attach this script to a GameObject with a Collider component
//Create an empty GameObject (Create>Create Empty) and attach it in the New Transform field in the Inspector of the first GameObject
//This script tells if a point  you specify (the position of the empty GameObject) is within the first GameObject’s Collider

using UnityEngine;

public class BoundExample : MonoBehaviour
{
    //Make sure to assign this in the Inspector window
    public Transform m_NewTransform;
    Collider m_Collider;
    Vector3 m_Point;

    void Start()
    {
        //Fetch the Collider from the GameObject this script is attached to
        m_Collider = GetComponent<Collider>();
        //Assign the point to be that of the Transform you assign in the Inspector window
        m_Point = m_NewTransform.position;
    }

    void Update()
    {
        m_Point = m_NewTransform.position;
        //If the first GameObject's Bounds contains the Transform's position, output a message in the console
        if (m_Collider.bounds.Contains(m_Point))
        {
            Debug.Log("Bounds contain the point : " + m_Point);
        }
    }
}