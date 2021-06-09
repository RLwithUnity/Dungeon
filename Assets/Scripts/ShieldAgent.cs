using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class ShieldAgent : Agent, IEntity
{
    public GameObject Shield;
    private GameObject SkillOn;
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;
    private float SlowDown;

    public int health = 50;
    public bool IsShieldUP; // have i lift a shield.

    public SpearAgent sp;
    public ShieldAgent sh;
    public MagicianAgent mg;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        SkillOn = Shield.transform.GetChild(5).gameObject;
        SlowDown = 1f;
    }

    public override void OnEpisodeBegin()
    {
        health = 50;
        IsShieldUP = false;
        SkillOn.SetActive(IsShieldUP);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(IsShieldUP);
        sensor.AddObservation(sp.health);
        sensor.AddObservation(sh.health);
        sensor.AddObservation(mg.health);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag("dragon"))
        {
            // Debug.Log(health);
            onDamage(1);
        }
    }

    void OnTriggerEnter(Collider col)
    {
    }

    public void agentAction()
    {
        if (IsShieldUP)
        {
            //Debug.Log("shield Down");
            IsShieldUP = false;
            SkillOn.SetActive(IsShieldUP);
            Shield.transform.localPosition = new Vector3(0f, 0.0f, 0.723f);
            SlowDown = 1f;
        }
        else
        {
            //Debug.Log("shield Up");
            IsShieldUP = true;
            SkillOn.SetActive(IsShieldUP);
            Shield.transform.localPosition = new Vector3(0f, 0.3f, 0.723f);
            SlowDown = 0.5f;
        }
    }

    public void onDamage(int damage)
    {
        if (!IsShieldUP)
        {
            //Debug.Log("Shield Agent damaged");
            health -= damage;
            m_GameController.HitbyDragon();

            if (health <= 0)
            {
                m_GameController.KilledByDragon(this);
            }
        }
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
            case 7: // shield action
                agentAction();
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200 * SlowDown);
        m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed * SlowDown,
            ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // shield action
            discreteActionsOut[0] = 7;
        }
    }
}
