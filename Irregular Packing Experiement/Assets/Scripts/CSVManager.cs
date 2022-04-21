using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;


public class CSVManager : MonoBehaviour
{

    private string reportDirectoryName = "Report";
    private string reportSeparator = ",";
    private string[] reportHeaders = new string[6] {
        "Pakcing efficiency",
        "COG",
        "Time",
        "Radiation limit exceed",
        "Weight limit exceed",
        "Number of packed objects"
    };
    private string timeStampHeader = "time stamp";

    #region Interactions

    public void AppendToReport(string[] strings)
    {
        VerifyDirectory();
        VerifyFile();
        using (StreamWriter sw = File.AppendText(GetFilePath()))
        {
            string finalString = "";
            for (int i = 0; i < strings.Length; i++)
            {
                if (finalString != "")
                {
                    finalString += reportSeparator;
                }
                finalString += strings[i];
            }
            finalString += reportSeparator + GetTimeStamp();
            sw.WriteLine(finalString);
        }
    }

    public void CreateReport()
    {
        VerifyDirectory();
        using (StreamWriter sw = File.CreateText(GetFilePath()))
        {
            string finalString = "";
            for (int i = 0; i < reportHeaders.Length; i++)
            {
                if (finalString != "")
                {
                    finalString += reportSeparator;
                }
                finalString += reportHeaders[i];
            }
            finalString += reportSeparator + timeStampHeader;
            sw.WriteLine(finalString);
        }
    }

    public void SaveReport()
    {
        VerifyDirectory();
        using (StreamWriter sw = File.CreateText(GetFilePath()))
        {
            sw.Close();
        }
    }

    #endregion


    #region Operations

    void VerifyDirectory()
    {
        string dir = GetDirectoryPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

     void VerifyFile()
    {
        string file = GetFilePath();
        if (!File.Exists(file))
        {
            CreateReport();
        }
    }

    #endregion


    #region Queries

     string GetDirectoryPath()
    {
        return Application.dataPath + "/../" + reportDirectoryName;
    }

     string GetFilePath()
    {
        return GetDirectoryPath() + "/Report_" + System.DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";
    }

     string GetTimeStamp()
    {
        return System.DateTime.Now.ToString();
    }

    #endregion

}
