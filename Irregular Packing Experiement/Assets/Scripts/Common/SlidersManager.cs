using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlidersManager : MonoBehaviour
{
    //Make sure to assign this in the Inspector window
    public GameObject collidercount;
    GameObject container;
    CollidersCounter colliders_counter;
    public float Weight;
    public float Radiation;
    public float PackingEfficiency;
    public float centerOfGravity;
    Slider weightslider;
    Slider radioactivityslider;
    Slider volumeslider;
    Slider gravityslider;
    Text weightText;
    Text radiationText;
    Text efficiencyText;
    Text cogErrorText;
    public Text nameOfVolunteer;
    private int frames = 0;
    private int countOfpackedObjects;
    private float COGerror;
    private float largesterror;
    private ConfigReporter reporter;
    bool packing = false;
    string mode = "linear";

    void Awake()
    {
        Weight = 0;
        Radiation = 0;
        PackingEfficiency = 0;
        countOfpackedObjects = 0;
        nameOfVolunteer = GetComponent<Text>();

        colliders_counter = collidercount.GetComponent<CollidersCounter>();
        container = collidercount;

        reporter = GameObject.Find("Controller").GetComponent<ConfigReporter>();

        if (SceneManager.GetActiveScene().name == "Trial-and-Error" || SceneManager.GetActiveScene().name == "Autonomous Packing")
        {
            mode = "trial";
        } else if (SceneManager.GetActiveScene().name == "Linear Packing")
        {
            mode = "linear";
        }

        if (mode == "trial")
        {
            weightslider = GameObject.Find("WeightSlider").GetComponent<Slider>();
            radioactivityslider = GameObject.Find("RadiationSlider").GetComponent<Slider>();
            volumeslider = GameObject.Find("EfficiencySlider").GetComponent<Slider>();
            gravityslider = GameObject.Find("COGError").GetComponent<Slider>();
        }
        else if (mode == "linear")
        {
            weightText = GameObject.Find("WeightPercentage").GetComponent<Text>();
            radiationText = GameObject.Find("RadiationPercentage").GetComponent<Text>();
            efficiencyText = GameObject.Find("EfficiencyPercentage").GetComponent<Text>();
            cogErrorText = GameObject.Find("COGErrorPercentage").GetComponent<Text>();
        }
        
    }

    void Update()
    {
        colliders_counter = collidercount.GetComponent<CollidersCounter>();
        container = collidercount;
        centerOfGravity = 0;
        COGerror = 0;
        largesterror = (container.transform.lossyScale.x + container.transform.lossyScale.y + container.transform.lossyScale.z) / 2;
        Weight = colliders_counter.Totalweight / colliders_counter.weightLimitation;
        Radiation = colliders_counter.Totalradioactivity / colliders_counter.doseLimitation;
        PackingEfficiency = colliders_counter.Totalvolume / colliders_counter.colliderVolume;

        countOfpackedObjects = colliders_counter.count;

        if (colliders_counter.centerOfGravity.y != 0)
        {
            COGerror = Mathf.Abs(colliders_counter.centerOfGravity.y - container.transform.position.y) + Mathf.Abs(colliders_counter.centerOfGravity.x - container.transform.position.x) + Mathf.Abs(colliders_counter.centerOfGravity.z - container.transform.position.z);
            centerOfGravity = COGerror / largesterror;

        }
        
        if (mode == "trial")
        {
            weightslider.value = Weight;
            radioactivityslider.value = Radiation;
            volumeslider.value = PackingEfficiency;
            gravityslider.value = centerOfGravity;
        }
        else if (mode == "linear")
        {
            weightText.GetComponent<ShowValueScript>().textUpdate(Weight);
            radiationText.GetComponent<ShowValueScript>().textUpdate(Radiation);
            efficiencyText.GetComponent<ShowValueScript>().textUpdate(PackingEfficiency);
            cogErrorText.GetComponent<ShowValueScript>().textUpdate(centerOfGravity);
        }

        frames++;
        if (frames % 50 == 0)
        { //If the remainder of the current frame divided by 200 is 0 run the function.
            reporter.AppendToReport(GetReportLine());
        }
    }

    public void SetTextOpacity()
    {
        if (!packing)
        {
            weightText.GetComponent<CanvasGroup>().alpha = 0;
            radiationText.GetComponent<CanvasGroup>().alpha = 0;
            efficiencyText.GetComponent<CanvasGroup>().alpha = 0;
            cogErrorText.GetComponent<CanvasGroup>().alpha = 0;
            packing = true;
        }
        else if (packing)
        {
            weightText.GetComponent<CanvasGroup>().alpha = 1;
            radiationText.GetComponent<CanvasGroup>().alpha = 1;
            efficiencyText.GetComponent<CanvasGroup>().alpha = 1;
            cogErrorText.GetComponent<CanvasGroup>().alpha = 1;
            packing = false;
        }
    }

    string[] GetReportLine()
    {
        string[] returnable = new string[6];
        returnable[0] = PackingEfficiency.ToString();
        returnable[1] = centerOfGravity.ToString();
        returnable[2] = GameObject.Find("Timer").GetComponent<Timer>().timerText.text;
        returnable[3] = Radiation.ToString();
        returnable[4] = Weight.ToString();
        returnable[5] = countOfpackedObjects.ToString();
        return returnable;
    }
}
