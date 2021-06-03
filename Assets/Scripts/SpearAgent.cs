using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SpearAgent : Agent, IEntity
{
    public GameObject Spear;
    private GameObject SkillOn;
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;
    
    public int stabCoolTime = 100;
    public int health = 50;

    public SpearAgent sp;
    public ShieldAgent sh;
    public MagicianAgent mg;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        SkillOn = Spear.transform.GetChild(7).gameObject;

    }

    public override void OnEpisodeBegin()
    {
        SkillOn.SetActive(false);
        health = 50;
        stabCoolTime = 100;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(stabCoolTime == 0);
        sensor.AddObservation(sp.health);
        sensor.AddObservation(sh.health);
        sensor.AddObservation(mg.health);
    }

    public void agentAction()
    {
        if (stabCoolTime == 0)
        {
            //Debug.Log("Stab!");
            // 창내리기
            Spear.transform.localPosition = new Vector3(0.7f, -0.15f, 0.9f);
            Spear.transform.localRotation = Quaternion.Euler(90f, 0, 0);

            Vector3 StepBack = this.transform.forward * -1f;
            m_AgentRb.AddForce(StepBack * m_PushBlockSettings.agentRunSpeed, ForceMode.VelocityChange);

            Invoke("Stab", 0.5f); // 0.5초 딜레이 후 Stab 메서드 실행
        }
    }

    public void Stab()
    {
        Vector3 stab = this.transform.forward * 2f;
        m_AgentRb.AddForce(stab * m_PushBlockSettings.agentRunSpeed, ForceMode.VelocityChange);

        stabCoolTime = 100;
        SkillOn.SetActive(false);
        Invoke("SpearUp", 1);
    }

    public void SpearUp()
    {
        Spear.transform.localPosition = new Vector3(0.7f, 0.5f, 0.3f);
        Spear.transform.localRotation = Quaternion.Euler(30f, 0, 0);
    }

    public void onDamage(int damage)
    {
        health -= damage;
        m_GameController.HitbyDragon();
        if (health <= 0)
        {
            m_GameController.KilledByDragon(this);
        }
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        stabCoolTime = stabCoolTime <= 0 ? 0 : stabCoolTime-1;
        if (stabCoolTime==0)
        {
            SkillOn.SetActive(true);
        }
        // Debug.Log(stabCoolTime);
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
                agentAction();
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
    }

    void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag("dragon"))
        {
            onDamage(1);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.transform.CompareTag("dragon"))
        {
            m_GameController.HitByWeapon(col);
        }
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
            discreteActionsOut[0] = 7;
        }
    }
}
