using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MagicianAgent: Agent
{

    public GameObject healParticles;
    // public GameObject MyKey; //my key gameobject. will be enabled when key picked up.
    //public bool IHaveAKey; //have i picked up a key
    private PushBlockSettings m_PushBlockSettings;
    private Rigidbody m_AgentRb;
    private DungeonEscapeEnvController m_GameController;

    public int health;

    public override void Initialize()
    {
        m_GameController = GetComponentInParent<DungeonEscapeEnvController>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
        // MyKey.SetActive(false);
        healParticles.SetActive(false);
        // IHaveAKey = false;
        health = 50;
    }

    public override void OnEpisodeBegin()
    {
        //MyKey.SetActive(false);
        //IHaveAKey = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(IHaveAKey);
    }

    void Heal(int value) {
        StartCoroutine(HealEffect());
        health += value;
        if (health > 100) {
            health = 100;
        }
    }

    private IEnumerator HealEffect() {
        healParticles.SetActive(true);
        yield return new WaitForSeconds(1f);
        healParticles.SetActive(false);
        yield return new WaitForSeconds(1f);
        healParticles.SetActive(true);
        yield return new WaitForSeconds(1f);
        healParticles.SetActive(false);
    }
    
    void CheckForHeal() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 4f);
        foreach(Collider c in colliders) {
            if (c.GetComponent<Agent>()) {
                //c.GetComponent<Agent>().Heal(5);
            }
        }
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
        if (col.transform.CompareTag("lock"))
        {
            /*
            if (IHaveAKey)
            {
                MyKey.SetActive(false);
                IHaveAKey = false;
                m_GameController.UnlockDoor();
            }
            */
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //if we find a key and it's parent is the main platform we can pick it up
        if (col.transform.CompareTag("key") && col.transform.parent == transform.parent && gameObject.activeInHierarchy)
        {
            print("Picked up key");
            // MyKey.SetActive(true);
            // IHaveAKey = true;
            col.gameObject.SetActive(false);
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
