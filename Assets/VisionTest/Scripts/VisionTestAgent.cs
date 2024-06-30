using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class VisionTestAgent : Agent
{
    TrainingSettings settings;
    public GameObject area;
    TrainArea m_MyArea;

    Rigidbody m_AgentRb;

    // Speed of agent rotation.
    public float turnSpeed = 300f;

    public float yPosition = 2f;


    public string myName = "Visual Agent";
    public string myData
    {
        get { return $"{myName} - {myReward}"; }
    }

    // Speed of agent movement.
    public float moveSpeed = 2;
    public Material normalMaterial;

    public bool contribute;
    public bool useVectorObs;
    [Tooltip("Use only the frozen flag in vector observations. If \"Use Vector Obs\" " +
             "is checked, this option has no effect. This option is necessary for the " +
             "VisualFoodCollector scene.")]


    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<TrainArea>();
        settings = FindObjectOfType<TrainingSettings>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            var localVelocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.z);

        }

    }



    public Color32 ToColor(int hexVal)
    {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }


    public float durationPenalty = 0.00001f;

    public void MoveAgent(ActionBuffers actionBuffers)
    {


        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;


        var forward = Mathf.Clamp(continuousActions[0], 0f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -0.5f, 0.5f);

        dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        rotateDir = -transform.up * rotate;


        m_AgentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);


        if (m_AgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            m_AgentRb.velocity *= 0.95f;
        }


    }


    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        MoveAgent(actionBuffers);
        updateMyReward(durationPenalty);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1;
        }
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public override void OnEpisodeBegin()
    {

        m_AgentRb.velocity = Vector3.zero;

        transform.position = new Vector3(Random.Range(-m_MyArea.range, m_MyArea.range),
            yPosition, Random.Range(-m_MyArea.range, m_MyArea.range))
            + area.transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        SetResetParameters();
        myReward = 0f;
    }


    float myReward = 0f;
    void updateMyReward(float reward)
    {

        AddReward(reward);
        myReward += reward;
        if (contribute)
        {
            settings.totalScore += reward;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        var attractor = collision.gameObject.GetComponent<AttractorLogic>();
        if (attractor != null)
        {
            if (attractor.OnAgentEncountered())
            {
                updateMyReward(attractor.myReward);
                if (attractor.restartsEpisode)
                {
                    EndEpisode();
                }
            }

        }
    }

    void OnTriggerEnter(Collider collider)
    {
        var attractor = collider.gameObject.GetComponent<AttractorLogic>();
        if (attractor != null)
        {
            if (attractor.OnAgentEncountered())
            {
                updateMyReward(attractor.myReward);
                if (attractor.restartsEpisode)
                {
                    EndEpisode();
                }
            }

        }
    }



    public void SetAgentScale()
    {
        /* float agentScale = m_ResetParams.GetWithDefault("agent_scale", 1.0f);
         gameObject.transform.localScale = new Vector3(agentScale, agentScale, agentScale);
        */
    }

    public void SetResetParameters()
    {

        m_MyArea.ResetArea();
        SetAgentScale();
    }
}
