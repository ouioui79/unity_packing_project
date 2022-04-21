using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scaler : MonoBehaviour
{
    public Slider slider;
    public GameObject obj;
    private Vector3 original_scale = Vector3.zero;
    
    public void ScaleObject()
    {
        if (original_scale == Vector3.zero)
        {
            original_scale = obj.transform.localScale;
        }
        obj.transform.localScale = new Vector3(original_scale.x/slider.value,original_scale.y/slider.value,original_scale.z/slider.value);
    }
}