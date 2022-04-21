//Attach this script to a GameObject with a Collider component
//Create an empty GameObject (Create>Create Empty) and attach it in the New Transform field in the Inspector of the first GameObject
//This script tells if a point  you specify (the position of the empty GameObject) is within the first GameObject’s Collider

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class CollidersCounter : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float multiplierX; //resize the container in the beginning
    [Range(0.0f, 10.0f)]
    public float multiplierY; 
    [Range(0.0f, 10.0f)]
    public float multiplierZ; 

    //Make sure to assign this in the Inspector window
    List<GameObject> Wastes = new List<GameObject>();
    List<GameObject> CurrentlyPackedWastes = new List<GameObject>();
    List<Property> Property_script= new List<Property>();
    BoxCollider m_Collider;
    public float Totalvolume;
    public float Totalradioactivity;
    public float Totalweight;
    public float weightLimitation = 367f;
    public float doseLimitation = 1.6f;
    public Vector3 centerOfGravity= new Vector3(0.0f, 0.0f, 0.0f);
    public int  count;
    int[] collidObjects=new int[100];
    Vector3 sumMassTimeslocation= new Vector3(0.0f, 0.0f, 0.0f);
    public float colliderVolume;

    void Awake()
    {
        Vector3 multiplier = new Vector3(multiplierX, multiplierY, multiplierZ);

        if (multiplier != null && multiplier.x > 0 && multiplier.y > 0 && multiplier.z > 0)
        {
            Vector3 initialScale = transform.localScale;

            transform.localScale = Vector3.Scale(transform.localScale, multiplier);

            Vector3 differenceInSize = transform.localScale - initialScale;
            differenceInSize = differenceInSize / 2; //get half of the size increase (to move the box that much more)

            //Move the box more outwards (also away from the ground)
            transform.position = new Vector3(transform.position.x + differenceInSize.x,
                                             transform.position.y + differenceInSize.y,
                                             transform.position.z - differenceInSize.z);
        }
    }

    void Start()
    {
        //Fetch the Collider from the GameObject this script is attached to
        m_Collider = GetComponent<BoxCollider>();
        colliderVolume = m_Collider.size[0] * m_Collider.size[1] * m_Collider.size[2] ;
        colliderVolume = colliderVolume * gameObject.transform.localScale.x * gameObject.transform.localScale.y * gameObject.transform.localScale.z;
        ResetWastes();
    }
    
    void Update()
    {
        /*
        int totalObjCount = 0;
        for (int i = 0; i < Wastes.Count; i++)
        {
            if (m_Collider.bounds.Contains(Wastes[i].transform.position))
            {
                totalObjCount++;
            }
        }
        Debug.Log("total obj count: " + totalObjCount);
        */

        CalculateTotalProperties();
        //string msg = "Total volume: " + Totalvolume + "\nTotal radioactivity: " + Totalradioactivity + "\nTotal weight: " + Totalweight;
        //Debug.Log(msg);
    }

    public void CalculateTotalProperties()
    {
        count = 0;
        Totalvolume = 0;
        Totalradioactivity = 0;
        Totalweight = 0;
        for (int i = 0; i < Wastes.Count; i++)
        {
            if (m_Collider.bounds.Contains(Wastes[i].transform.position))
            {
                Totalvolume = Totalvolume + Wastes[i].GetComponent<Property>().GetVolume();
                Totalradioactivity = Totalradioactivity + Wastes[i].GetComponent<Property>().GetRadioactivity();
                Totalweight = Totalweight + Wastes[i].GetComponent<Property>().GetWeight();
                collidObjects[count] = i;
                count = count + 1;
            }
        }
        sumMassTimeslocation = new Vector3(0,0,0);

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {                                              
                sumMassTimeslocation = sumMassTimeslocation + Wastes[collidObjects[i]].GetComponent<Property>().GetVolume() * Wastes[collidObjects[i]].GetComponent<Property>().centerOfGravity;
                //Debug.Log("sumMassTimeslocation: " + sumMassTimeslocation);
            }

            centerOfGravity = sumMassTimeslocation / Totalvolume;
        }

        else
            centerOfGravity = Vector3.zero;
    }

    public List<GameObject> GetWastes()
    {
        return Wastes;
    }

    public void ResetWastes()
    {
        Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
    }

    public List<GameObject> GetCurrentlyPackedWastes()
    {
        CurrentlyPackedWastes = new List<GameObject>();

        for (int i = 0; i < Wastes.Count; i++)
        {
            if (m_Collider.bounds.Contains(Wastes[i].transform.position))
            {
                CurrentlyPackedWastes.Add(Wastes[i]);
            }
        }

        //Debug.Log("Currently packing: " + string.Join(", ", CurrentlyPackedWastes));

        return CurrentlyPackedWastes;
    }
}