using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Tracker
{

    Vector3 _avgPosition;
    Bounds bounds;
    List<Vector3> positions = new List<Vector3>();

    public int duration = 1;
    public Tracker(Vector3 newPosition)
    {
        positions.Add(newPosition);
        bounds = new Bounds(newPosition, newPosition);
        _avgPosition = newPosition;
    }

    public Vector3 avgPosition
    {
        get { return _avgPosition; }
    }

    public void addPosition(Vector3 newPosition)
    {
        bounds.Encapsulate(newPosition);
        _avgPosition = bounds.center;
        _avgPosition.x = (float)Math.Round(_avgPosition.x, 2);
        _avgPosition.y = (float)Math.Round(_avgPosition.y, 2);
        _avgPosition.z = (float)Math.Round(_avgPosition.z, 2);

        positions.Add(newPosition);

    }


}


public class ATLogger : MonoBehaviour
{
    // Start is called before the first frame update

    List<Tracker> locations = new List<Tracker>();

    public void ResetLocations()
    {
        locations.Clear();
    }

    public bool LogPath = false;
    public Color myColor = Color.red;
    public float positionRadiusThreshold = 20f;

    string MyName
    {
        get
        {
            var agent = GetComponent<VisionTestAgent>();
            if (agent != null)
            {
                return agent.myName;
            }
            return null;
        }
    }

    Vector3 cleanVector(Vector3 currentVec)
    {
        currentVec.y = 0f;
        currentVec.x = (float)Math.Round(currentVec.x, 2);
        currentVec.y = (float)Math.Round(currentVec.y, 2);
        currentVec.z = (float)Math.Round(currentVec.z, 2);
        return currentVec;

    }

    // Update is called once per frame
    void Update()
    {
        if (!LogPath)
        {
            return;
        }

        var currentPosition = cleanVector(transform.position);



        if (locations.Any())
        {
            var last = locations.Last();
            var distance = (currentPosition - last.avgPosition).magnitude;



            if (distance < positionRadiusThreshold)
            {
                last.duration++;
                last.addPosition(currentPosition);
                return;
            }
        }


        var tracker = new Tracker(currentPosition);
        locations.Add(tracker);

    }

    void cleanLocations()
    {
        var points = new List<Tracker>();

        Tracker prev = null;
        foreach (Tracker pt in locations)
        {
            if (prev == null || (prev.avgPosition - pt.avgPosition).sqrMagnitude != 0f)
            {

                prev = pt;
                points.Add(pt);

            }
            else
            {
                prev.duration += pt.duration;
            }

        }
        locations = points;
    }

    bool CheckCanWriteFile()
    {

        cleanLocations();

        return LogPath && locations.Count > 2;
    }

    public void WriteCSV()
    {
        if (!CheckCanWriteFile())
        {
            return;
        }

        var csvOutput = new StringBuilder();
        csvOutput.AppendLine("x,y,z,duration");
        foreach (var location in locations)
        {
            csvOutput.AppendLine($"{location.avgPosition.x},{location.avgPosition.y},{location.avgPosition.z},{location.duration}");
        }

        System.IO.File.WriteAllText($"{MyName}.csv", csvOutput.ToString());


    }

    string svgColor(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

    string MyViewBox
    {
        get
        {
            var renders = FindObjectsOfType<Renderer>();
            var bounds = new Bounds(transform.position, transform.position);
            foreach (var render in renders)
            {
                bounds.Encapsulate(render.bounds);
            }

            var min = bounds.min;
            var siz = bounds.size;

            return $"viewBox='{min.x} {min.z} {siz.x} {siz.z}'";


        }
    }

    public void WriteSVG()
    {
        WriteSVGPath();
        WriteSVGDurationBubble();
    }

    public void WriteSVGPath()
    {

        if (!CheckCanWriteFile())
        {
            return;
        }

        var svgPathOutput = new StringBuilder();

        svgPathOutput.AppendLine($"<svg {MyViewBox}  xmlns='http://www.w3.org/2000/svg'>");


        svgPathOutput.AppendLine($"<polyline fill='none' stroke-width='0.5' stroke='{svgColor(myColor)}' points='");

        foreach (var pt in locations)
        {
            svgPathOutput.Append($"{pt.avgPosition.x},{pt.avgPosition.z} ");
        }
        svgPathOutput.AppendLine("'/>");



        var first = locations.First().avgPosition;

        svgPathOutput.AppendLine($"<circle cx='{first.x}' cy='{first.z}' r='1' fill='{svgColor(Color.white)}' stroke-width='0.5' stroke='{svgColor(myColor)}'></circle>");

        var last = locations.Last().avgPosition;
        svgPathOutput.AppendLine($"<circle cx='{last.x}' cy='{last.z}' r='1' fill='{svgColor(myColor)}'></circle>");
        svgPathOutput.AppendLine("</svg>");

        System.IO.File.WriteAllText($"{MyName}-Path.svg", svgPathOutput.ToString());


    }



    public float MaxStayRadius = 5;
    public float MinStayRadius = 1;


    public void WriteSVGDurationBubble()
    {

        if (!CheckCanWriteFile())
        {
            return;
        }

        var svgBubbleOutput = new StringBuilder();

        svgBubbleOutput.AppendLine($"<svg {MyViewBox}  xmlns='http://www.w3.org/2000/svg'>");


        var maxDuration = (float)locations.Max(t => t.duration);
        var minDuration = (float)locations.Min(t => t.duration);

        var pointsLength = locations.Count;

        var count = 0;
        foreach (Tracker pt in locations)
        {

            var adjusted = (pt.duration - minDuration) / maxDuration;
            var bubbleRadius = MinStayRadius + adjusted * MaxStayRadius;

            count++;
            var countPct = count / (float)pointsLength;

            var fillOpacity = System.Math.Round(Mathf.Lerp(0.29f, 0.73f, countPct), 2);

            svgBubbleOutput.AppendLine($"<circle cx='{pt.avgPosition.x}' cy='{pt.avgPosition.z}' r='{bubbleRadius}' fill='{svgColor(myColor)}' fill-opacity='{fillOpacity}'/>");


        }



        svgBubbleOutput.AppendLine("</svg>");

        System.IO.File.WriteAllText($"{MyName}-StayBubbleChart.svg", svgBubbleOutput.ToString());


    }
}
