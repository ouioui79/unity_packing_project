﻿//Attach this script to a GameObject with a Collider component
//Create an empty GameObject (Create>Create Empty) and attach it in the New Transform field in the Inspector of the first GameObject
//This script tells if a point  you specify (the position of the empty GameObject) is within the first GameObject’s Collider

using UnityEngine;
using System;
using System.Collections;

public class CountCollider : MonoBehaviour
{
    //Make sure to assign this in the Inspector window
    public GameObject[] Wastes = new GameObject[20];
    Property[] Property_script= new Property[20];
    Collider m_Collider;
    Vector3[] Wastes_Point=new Vector3[20];
    public float Totalvolume;
    public float Totalradioactivity;
    public float Totalweight;
    public float weightLimitation = 4100.27f;
    public float doseLimitation = 2f;
    public Vector3 centerOfGravity= new Vector3(0.0f, 0.0f, 0.0f);
    public int  count;
    int[] collidObjects=new int[20];
    Vector3 sumMassTimeslocation= new Vector3(0.0f, 0.0f, 0.0f);
    public float colliderVolume;

    void Start()
    {
        //Fetch the Collider from the GameObject this script is attached to
        m_Collider = GetComponent<Collider>();
        colliderVolume = gameObject.transform.lossyScale.x * gameObject.transform.lossyScale.y * gameObject.transform.lossyScale.z;
        Wastes = GameObject.FindGameObjectsWithTag("Interactable");
        //Assign the point to be that of the Transform you assign in the Inspector window
        for (int i=0; i<Wastes.Length; i++)
        {
            Wastes_Point[i] = Wastes[i].transform.position;
            Property_script[i]= Wastes[i].GetComponent<Property>();
            count = 0;
        }
      
    }


    void Update()
    {
        for (int i = 0; i < Wastes.Length; i++)
        {
            Wastes_Point[i] = Wastes[i].transform.position;
        }
        count = 0;
        Totalvolume = 0;
        Totalradioactivity = 0;
        Totalweight = 0;
        for (int i = 0; i < Wastes.Length; i++)
        {
            if (m_Collider.bounds.Contains(Wastes_Point[i]))
            {
                
                Totalvolume = Totalvolume + Property_script[i].GetVolume();
                Totalradioactivity = Totalradioactivity + Property_script[i].GetRadioactivity();
                Totalweight = Totalweight + Property_script[i].GetWeight();
                collidObjects[count] = i;
                count = count + 1;
            }
        }
        sumMassTimeslocation = new Vector3(0,0,0);

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                sumMassTimeslocation = sumMassTimeslocation + Property_script[collidObjects[i]].GetVolume() * Property_script[collidObjects[i]].centerOfGravity;
                //Debug.Log("sumMassTimeslocation: " + sumMassTimeslocation);
            }

            centerOfGravity = sumMassTimeslocation / Totalvolume;

        }

        else
            centerOfGravity = Vector3.zero;


        //If the first GameObject's Bounds contains the Transform's position, output a message in the console
    }
}