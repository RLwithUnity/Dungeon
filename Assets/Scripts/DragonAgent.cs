using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class DragonAgent : Agent
{
    public Rigidbody m_AgentRb;
    public float health;
    public GameObject projectile;
    public float projectileSpeed = 1;
    private DungeonEscapeEnvController m_GameController;

    // Start is called before the first frame update
    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        health = 100;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(health);
    }

    public void onDamage()
    {
        //Debug.Log(health);
        health -= 1;
        if (health <= 0)
        {
            m_GameController.EndGroupEpisodeAgent();
        }
    }
    public override void OnEpisodeBegin()
    {
        health = 100;
    }

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
            case 7:
                GameObject fireball = Instantiate(projectile, transform) as GameObject;
                fireball.transform.parent = GameObject.FindWithTag("area").transform;
                Rigidbody rb = fireball.GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * projectileSpeed, ForceMode.VelocityChange);
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.Z))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.V))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 7;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
