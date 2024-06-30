

using System.Collections;
using System.Collections.Generic;
using Unity.MLAgentsExamples;
using UnityEngine;
[System.Serializable]
public struct AttractorSetting
{
    public GameObject attractor;
    public int count;
    public bool respawn;

}

public class TrainArea : Area
{

    public float range;

    public AttractorSetting[] attractors;




    void CreateFood(AttractorSetting setting)
    {
        for (int i = 0; i < setting.count; i++)
        {
            GameObject f = Instantiate(setting.attractor, new Vector3(Random.Range(-range, range), 0f,
                Random.Range(-range, range)) + transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f)));
            var attractor = f.GetComponent<AttractorLogic>();
            attractor.isStatic = false;

            attractor.respawn = setting.respawn;
            attractor.myArea = this;
            attractor.Reset();
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {

                agent.transform.position = new Vector3(Random.Range(-range, range), 2f,
                    Random.Range(-range, range))
                    + transform.position;
                agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            }
        }

        foreach (var a in attractors)
        {
            CreateFood(a);
        }

    }

    public override void ResetArea()
    {
        base.ResetArea();
        var ls = GetComponentsInChildren<AttractorLogic>();

        foreach (var a in ls)
        {
            a.Reset();
        }
    }
}
