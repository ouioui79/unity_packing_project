using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using UnityEngine.SceneManagement;


public class ConfigReporter : MonoBehaviour
{

    private List<string[]> configRowData = new List<string[]>();
    private List<string[]> reportRowData = new List<string[]>();
    private List<string> selections = new List<string>();

    GameObject[] objects;
    BoxCollider counter_collider;
    ObjectManager manager;

    //private  configOutStream;
    //private  reportOutStream;

    private string configFilePath;
    private string reportFilePath;

    private bool started = false;
    private bool stopped = false;

    public bool runGA = false;

    void Start()
    {
        configFilePath = GetConfigPath();

        reportFilePath = GetReportPath();

    }

    public void SetUp()
    {
        objects = GameObject.FindGameObjectsWithTag("Interactable");
        counter_collider = GameObject.Find("Counter").GetComponent<BoxCollider>();

        if (SceneManager.GetActiveScene().name == "Autonomous Packing" && runGA)
        {
            var temp = GameObject.Find("Controller").GetComponent<ObjectManagerGA>().GetSelections();
            foreach (string name in temp)
            {
                selections.Add(name);
            }
        }

        string[] reportHeaders = new string[6] {
            "Pakcing efficiency",
            "COG",
            "Time",
            "Radiation limit exceed",
            "Weight limit exceed",
            "Number of packed objects"
        };
        reportRowData.Add(reportHeaders);
        started = true;
    }

    public void AppendToReport(string[] line)
    {

        if (started && !stopped)
        {
            reportRowData.Add(line);
        }
    }

    public void Save()
    {
        if (started && !stopped)
        {
            string delimiter = ",";
            // Creating First row of titles manually..
            string[] rowDataTemp = new string[9] {
                "Object",
                "Pos x",
                "Pos y",
                "Pos z",
                "Rot x",
                "Rot y",
                "Rot z",
                "Packed",
                "Used for GA"
            };
            configRowData.Add(rowDataTemp);
            foreach (GameObject obj in objects)
            {
                rowDataTemp = new string[9];
                rowDataTemp[0] = obj.name;
                rowDataTemp[1] = obj.transform.position.x.ToString();
                rowDataTemp[2] = obj.transform.position.y.ToString();
                rowDataTemp[3] = obj.transform.position.z.ToString();
                rowDataTemp[4] = obj.transform.rotation.eulerAngles.x.ToString();
                rowDataTemp[5] = obj.transform.rotation.eulerAngles.y.ToString();
                rowDataTemp[6] = obj.transform.rotation.eulerAngles.z.ToString();
                if (counter_collider.bounds.Contains(obj.transform.position))
                {
                    rowDataTemp[7] = "Y";
                }
                else
                {
                    rowDataTemp[7] = "N";
                }

                if(SceneManager.GetActiveScene().name == "Autonomous Packing")
                {
                    if (selections.Contains(obj.name))
                    {
                        rowDataTemp[8] = "Y";
                    }
                    else
                    {
                        rowDataTemp[8] = "N";
                    }
                }
                configRowData.Add(rowDataTemp);

                string[][] conifigOutput = new string[configRowData.Count][];

                for (int i = 0; i < conifigOutput.Length; i++)
                {
                    conifigOutput[i] = configRowData[i];
                }

                StreamWriter configOutStream = System.IO.File.CreateText(configFilePath);
                int configLength = conifigOutput.GetLength(0);

                StringBuilder configSb = new StringBuilder();

                for (int index = 0; index < configLength; index++)
                    configSb.AppendLine(string.Join(delimiter, conifigOutput[index]));

                configOutStream.WriteLine(configSb);
                configOutStream.Close();
            }

            StreamWriter reportOutStream = System.IO.File.CreateText(reportFilePath);

            string[][] reportOutput = new string[reportRowData.Count][];

            for (int i = 0; i < reportOutput.Length; i++)
            {
                reportOutput[i] = reportRowData[i];
            }

            int reportLength = reportOutput.GetLength(0);

            StringBuilder reportSb = new StringBuilder();

            for (int index = 0; index < reportLength; index++)
                reportSb.AppendLine(string.Join(delimiter, reportOutput[index]));

            reportOutStream.WriteLine(reportSb);
            reportOutStream.Close();
        }

        stopped = true;
    }

    // Following method is used to retrive the relative path as device platform
    private string GetConfigPath()
    {
        return Application.dataPath + "/../Config/Config_" + System.DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";
    }

    private string GetReportPath()
    {
        return Application.dataPath + "/../Report/Report_" + System.DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";
    }
}