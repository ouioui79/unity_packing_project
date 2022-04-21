using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class Timer : MonoBehaviour
{
    public Text timerText;
    Stopwatch run_timer = new Stopwatch();

    private bool isRunning = false;

    GameObject[] initialObjects;
    Vector3[] originalPosition;
    Vector3[] originalRotation;




    // Start is called before the first frame update
    public void SetUpTimer(int num_objects)
    {
       
        //Fetch the Collider from the GameObject this script is attached to
        initialObjects = GameObject.FindGameObjectsWithTag("Interactable");
        originalPosition = new Vector3[num_objects];
        originalRotation = new Vector3[num_objects];
        for (int i = 0; i < num_objects; i++)
        {
             originalPosition[i] = initialObjects[i].transform.position;
             originalRotation[i] = initialObjects[i].transform.rotation.eulerAngles;
        }
    }
    void ResetInteractables()
    {
        for (int i = 0; i < 1; i++)
        {
            initialObjects[i].transform.position = originalPosition[i];
            initialObjects[i].transform.eulerAngles = originalRotation[i];
            initialObjects[i].GetComponent<Rigidbody>().isKinematic = true;

        }
    }

    public void TimerStart()
    {
        if (!isRunning)
        {
           
            isRunning = true;
            run_timer.Start();
        }
    }

    public void TimerStop()
    {
       
        if (isRunning)
        {
           
            isRunning = false;
            run_timer.Stop();
        }
    }
    /*
    public void TimerReset()
    {
     
        stopTime = 0;
        isRunning = false;
        timerTime = 0;
        string minutes = ((int)timerTime / 60).ToString();
        string seconds = (timerTime % 60).ToString("f2");

        timerText.text = minutes + ":" + seconds;
        ResetInteractables();

    }*/

    // Update is called once per frame
    void Update()
    {

        if (isRunning)
        {
            timerText.text = run_timer.Elapsed.ToString(@"mm\:ss\.ff");
        }

    }
}

