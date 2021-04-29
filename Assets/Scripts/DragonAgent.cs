using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DragonAgent : Agent
{
    public float walkSpeed;
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
    }

    public override void OnEpisodeBegin()
    {
        walkSpeed = 2;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag("portal"))
        {
            m_GameController.DragonTouchedHazard(this);
        }

        if (col.transform.CompareTag("agent"))
        {
            m_GameController.KillAgent();
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.L))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.I))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            discreteActionsOut[0] = 2;
        }
    }
}
