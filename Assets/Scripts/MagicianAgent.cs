using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MagicianAgent: Agent
{
    public GameObject Wand;
    private GameObject SkillOn;
    public GameObject healParticles;
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;

    public int health;
    public int stabCoolTime = 100;

    public SpearAgent sp;
    public ShieldAgent sh;
    public MagicianAgent mg;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        healParticles.SetActive(false);
        SkillOn = Wand.transform.GetChild(4).gameObject;
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

    private IEnumerator HealEffect() {
        healParticles.SetActive(true);
        yield return new WaitForSeconds(1f);
        healParticles.SetActive(false);
    }
    
    void CheckForHeal() {

        // Debug.Log("Heal!");
        if (stabCoolTime == 0) 
        { 
            Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
            foreach(Collider c in colliders) {
                if (c.GetComponent<SpearAgent>()) {
                    StartCoroutine(HealEffect());
                    SpearAgent Agent = c.GetComponent<SpearAgent>();
                    Agent.health = Agent.health > 50 ? 50 : Agent.health + 10;
                    // Debug.Log(Agent.health);
                }

                else if (c.GetComponent<ShieldAgent>())
                {
                    StartCoroutine(HealEffect());
                    ShieldAgent Agent = c.GetComponent<ShieldAgent>();
                    Agent.health = Agent.health > 50 ? 50 : Agent.health + 10;
                    // Debug.Log(Agent.health);
                }
            }
            stabCoolTime = 100;
            SkillOn.SetActive(false);
        }
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
        stabCoolTime = stabCoolTime <= 0 ? 0 : stabCoolTime - 1;
        if (stabCoolTime == 0)
        {
            SkillOn.SetActive(true);
        }
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
                CheckForHeal();
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

    void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag("dragon"))
        {
            //Debug.Log(health);
            onDamage(1);
        }
    }

    void OnCollisionEnter(Collision col)
    {
    }

    void OnTriggerEnter(Collider col)
    {
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
        else if (Input.GetKey(KeyCode.O))
        {
            CheckForHeal();
        }
        else if (Input.GetKey(KeyCode.P))
        {
            health -= 5;
        }
    }
}
