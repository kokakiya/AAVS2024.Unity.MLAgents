using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class TrainingSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;

    [HideInInspector]
    public TrainArea[] trainListArea;

    [HideInInspector]
    public float totalScore;

    public Camera activeCamera;

    public TMP_Text statusLabel;

    public string LogFilePath = "output";
    public DateTime Session = DateTime.Now;

    StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    void EnvironmentReset()
    {
        var ls = GameObject.FindObjectsOfType<AttractorLogic>();

        foreach (var a in ls.Where(m => m.isStatic == true))
        {
            a.Reset();
        }

        ClearObjects(ls.Where(m => m.isStatic == false).Select(m => m.gameObject).ToArray());

        agents = GameObject.FindGameObjectsWithTag("agent");

        trainListArea = FindObjectsOfType<TrainArea>();
        foreach (var fa in trainListArea)
        {
            fa.ResetFoodArea(agents);
        }

        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var food in objects)
        {
            Destroy(food);
        }
    }

    string getSession()
    {
        var dt = DateTime.Now;
        return dt.ToString("MMdd-HHmmss");
    }

    public void writeAllFiles()
    {
        var listOfLogger = FindObjectsOfType<ATLogger>();
        var session = getSession();
        foreach (var logger in listOfLogger)
        {
            logger.WriteCSV(session);
            logger.WriteSVGPath(session);
            logger.WriteSVGDurationBubble(session);
        }
    }

    public void writeCSVFiles()
    {
        var listOfLogger = FindObjectsOfType<ATLogger>();
        foreach (var logger in listOfLogger)
        {
            logger.WriteCSV(getSession());
        }
    }

    public void writePathFiles()
    {
        var listOfLogger = FindObjectsOfType<ATLogger>();
        foreach (var logger in listOfLogger)
        {
            logger.WriteSVGPath(getSession());
        }
    }

    public void writeBubbleChartFiles()
    {
        var listOfLogger = FindObjectsOfType<ATLogger>();
        foreach (var logger in listOfLogger)
        {
            logger.WriteSVGDurationBubble(getSession());
        }
    }

    public void FixedUpdate()
    {
        if (activeCamera == null)
        {
            return;
        }

        activeCamera.enabled = true;
    }

    public void Update()
    {
        var list = FindObjectsOfType<VisionTestAgent>();
        var listLength = System.Math.Max(list.Length, 1);
        var data = new StringBuilder();
        data.AppendLine($"Total Score: {totalScore}");
        data.AppendLine($"Average Score: {totalScore / listLength}");
        data.AppendLine($"-----");
        foreach (var agent in list)
        {
            data.AppendLine(agent.myData);
        }
        statusLabel.text = data.ToString();

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalScore", totalScore);
        }
    }
}
