using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;

public class MovementTracker
{
    public MovementTracker(Vector3 pos) {
        positions.Add(pos);
        bounds = new Bounds(pos, pos);
        _avgPosition = pos ;
    }
    public void addPosition(Vector3 pos)
    {
        bounds.Encapsulate(pos);
        _avgPosition = bounds.center;
        _avgPosition.x = (float) Math.Round(_avgPosition.x, 2);
        _avgPosition.y = (float)Math.Round(_avgPosition.y, 2);
        _avgPosition.z = (float)Math.Round(_avgPosition.z, 2);

        positions.Add(pos);
    }

    Vector3 _avgPosition;
    public Vector3 avgPosition
    {
        get { return _avgPosition; }
    }

    Bounds bounds;
    public List<Vector3> positions = new List<Vector3>();
 
    public float enterTime = 0f;
    public float leaveTime = 0f;
    public int duration = 1;

    public float radius
    {
        get
        {
            return Math.Max(leaveTime - enterTime, 1);
        }
    }
}
public class PathLogger : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {


    }
    public bool LogPath = false;

    int count = 0;
    MovementTracker last = null;
    List<MovementTracker> locations = new List<MovementTracker>();

    public string myName
    {
        get { return GetComponent<FoodCollectorAgent>().myName; }
    }



    public void reset()
    {
        locations.Clear();
    }
    float distanceThreshold = 10f;
    // Update is called once per frame
    void Update()
    {
        if (!LogPath)
        {
            return;
        }
        var pos = transform.position;
        pos.y = 0f;
        pos.x = (float)Math.Round(pos.x,2);
        pos.y = (float)Math.Round(pos.y, 2);
        pos.z = (float)Math.Round(pos.z, 2);

        if (last != null)
        {
            last.leaveTime = Time.frameCount;
            var distance = (pos - last.avgPosition).magnitude;
            var text = $": {last.leaveTime} /  {last.radius} / {last.positions.Count} / {distance}";



            if (distance < distanceThreshold)
            {
                last.duration++;
                last.addPosition(pos);

              
                //last = last.Value + (new Vector3(0, 0.25f, 0)); 
                //locations[locations.Count -1] = last.Value;
                return;
            }
            // Debug.Log($"New Position for {myName}{text}");
        }

        last = new MovementTracker(pos);   
        last.enterTime = Time.frameCount;
        last.leaveTime = Time.frameCount;
        locations.Add(last);
        count++;
        count = count % 100;


    }


    public float MaxStayRadius = 5;
    public float MinStayRadius = 1;

    public void writeSVG(string logFileName, Color myColor)
    {
        if (!LogPath)
        {
            return;
        }

        var points = new List<MovementTracker>();

        MovementTracker prev = null;
        var maxRadius = 0f;
        var minRadius = 0f;
        foreach (MovementTracker pt in locations)
        {
            if (prev == null || (prev.avgPosition  - pt.avgPosition).sqrMagnitude != 0f)
            {
                if (prev != null)
                {
                    //Debug.Log($"Next for {pt.avgPosition} {(prev.avgPosition - pt.avgPosition).sqrMagnitude}");
                }
                prev = pt;
                points.Add(pt);

            }
            else
            {
                prev.duration += pt.duration;
            }
            maxRadius = Math.Max(prev.duration, maxRadius);
            minRadius = minRadius == 0 ? maxRadius : Math.Min(prev.duration, minRadius);
        }




        Renderer[] renderers = FindObjectsOfType<Renderer>();

        var _worldBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; ++i)
            _worldBounds.Encapsulate(renderers[i].bounds);
        var min = _worldBounds.min;
        var sz = _worldBounds.size;
        var bx = $"viewBox='{min.x} {min.z} {sz.x} {sz.z}'";

        var color = $"rgb({myColor.r * 255} {myColor.g * 255} {myColor.b * 255})";
        var wColor = Color.white;

        var innerColor = $"rgb({wColor.r * 255} {wColor.g * 255} {wColor.b * 255})";

        StringBuilder str = new StringBuilder($"<svg {bx} xmlns='http://www.w3.org/2000/svg'>");

        str.AppendLine($"<polyline  fill='none'  stroke-width='0.5' stroke='{color}' points='");

        foreach (MovementTracker pt in points)
        {

            str.Append($"{pt.avgPosition.x},{pt.avgPosition.z} ");


        }
        str.AppendLine("'/>");
        var p = points.First();
        str.AppendLine($"<circle cx='{p.avgPosition.x}' cy='{p.avgPosition.z}' r='{1f}' fill='{innerColor}' stroke-width='0.5' stroke='{color}'/>");

        p = points.Last();

        str.AppendLine($"<circle cx='{p.avgPosition.x}' cy='{p.avgPosition.z}' r='{1f}' fill='{color}'/>");



   
        str.AppendLine("</svg>");

        System.IO.File.WriteAllText($"{logFileName}-PolyLine.svg", str.ToString());

        str = new StringBuilder($"<svg {bx} xmlns='http://www.w3.org/2000/svg'>");

      //  var range = maxRadius - minRadius;
        var pointsLength = points.Count;

        var count = 0;
        foreach (MovementTracker pt in points)
        {

            var adjusted = ( pt.duration - minRadius) / maxRadius ;
            var distance = MinStayRadius + adjusted * MaxStayRadius;
            // Debug.Log($"Radius for {pt.duration} / {adjusted} / {distance}");

            count++;
            var countPct = count / (float)pointsLength;

            var fillOpacity = Math.Round(Mathf.Lerp(0.41f, 0.73f, countPct), 2);

            str.AppendLine($"<circle cx='{pt.avgPosition.x}' cy='{pt.avgPosition.z}' r='{distance}' fill='{color}' fill-opacity='{fillOpacity}'/>");


        }


        str.AppendLine("</svg>");
        System.IO.File.WriteAllText($"{logFileName}-Positions.svg", str.ToString());

    }
}
