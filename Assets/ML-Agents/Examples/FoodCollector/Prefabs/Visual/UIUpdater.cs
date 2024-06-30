using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;


public class UIUpdater : MonoBehaviour
{

    public TMP_Text statusLabel;
    public bool shouldLogPaths = false;

    public void writeFiles()
    {
        var listOfLogger = FindObjectsOfType<ATLogger>();
        foreach (var logger in listOfLogger)
        {
            logger.WriteSVG();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        var list = FindObjectsOfType<VisionTestAgent>();
        var data = new StringBuilder();
        foreach (var agent in list)
        {
            data.AppendLine(agent.myData);
        }
        statusLabel.text = data.ToString();
    }


}
