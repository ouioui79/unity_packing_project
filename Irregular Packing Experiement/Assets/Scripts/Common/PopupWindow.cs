﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PopupWindow : MonoBehaviour
{
    GameObject window;
    public Text messageField;
    Slider weightslider;
    Slider radioactivityslider;
    string message;
    SlidersManager slider_manager;
    string mode = "linear";

    // Show(string)
    //Display the indicated message in a pop-up window

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Trial-and-Error" || SceneManager.GetActiveScene().name == "Autonomous Packing")
        {
            mode = "trial";
        }
        if (mode == "trial")
        {
            weightslider = GameObject.Find("WeightSlider").GetComponent<Slider>();
            radioactivityslider = GameObject.Find("RadiationSlider").GetComponent<Slider>();
        }
        else if (mode == "linear")
        {
            slider_manager = GameObject.Find("ScoreUI").GetComponent<SlidersManager>();
        }
        
        window = GameObject.Find("PopUpWindow");
    }

    void Update()
    {
        if (mode == "trial")
        {
            if (weightslider.value > 1)
            {
                if (radioactivityslider.value <= 1)
                {
                    Show("Weight limit exceeded!");
                }

                else
                {
                    Show("Weight and radiation limits exceeded!");
                }

            }
            else
            {
                if (radioactivityslider.value > 1)
                {
                    Show("Radiation limit exceeded!");
                }

                else
                    window.SetActive(false);

            }
        }
        else if (mode == "linear")
        {
            if (slider_manager.Weight > 1)
            {
                if (slider_manager.Radiation <= 1)
                {
                    Show("Weight limit exceeded!");
                }

                else
                {
                    Show("Weight and radiation limits exceeded!");
                }

            }
            else
            {
                if (slider_manager.Radiation > 1)
                {
                    Show("Radiation limit exceeded!");
                }

                else
                    window.SetActive(false);

            }
        }
            
    }

    public void Show(string message)
    {
        //messageField.text = message;
        window.SetActive(true);
    }

    
}
