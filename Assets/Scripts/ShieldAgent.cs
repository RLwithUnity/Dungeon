using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class ShieldAgent : Agent, IEntity
{
    // public GameObject MyKey; //my key gameobject. will be enabled when key picked up.
    // public bool IHaveAKey; //have i picked up a key
    public GameObject Shield;
    public PushBlockSettings m_PushBlockSettings;
    public Rigidbody m_AgentRb;
    public DungeonEscapeEnvController m_GameController;
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

        //MyKey.SetActive(false);
        //IHaveAKey = false;
        IsShieldUP = false;


    }

    public override void OnEpisodeBegin()
    {
        //MyKey.SetActive(false);
        //IHaveAKey = false;
        health = 50;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(IHaveAKey);
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
            Debug.Log(health);
            onDamage();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //if we find a key and it's parent is the main platform we can pick it up
        if (col.transform.CompareTag("key") && col.transform.parent == transform.parent && gameObject.activeInHierarchy)
        {
            print("Picked up key");
            //MyKey.SetActive(true);
            //IHaveAKey = true;
            col.gameObject.SetActive(false);
        }
    }

    public void agentAction()
    {
        if (IsShieldUP)
        {
            Debug.Log("shield Down");
            IsShieldUP = false;
            Shield.transform.localPosition = new Vector3(0f, 0.0f, 0.723f);
            m_PushBlockSettings.agentRunSpeed = 3; // default 3
            m_PushBlockSettings.agentRotationSpeed = 15; // default 15
        }
        else
        {
            Debug.Log("shield Up");
            IsShieldUP = true;
            Shield.transform.localPosition = new Vector3(0f, 0.3f, 0.723f);
            m_PushBlockSettings.agentRunSpeed = 1.5f; // default 3
            m_PushBlockSettings.agentRotationSpeed = 10; // default 15
        }
    }

    public void onDamage()
    {
        if (!IsShieldUP)
        {
            Debug.Log("Shield Agent damaged");
            health -= 1;
            
            if (health <= 0)
            {
                m_GameController.KilledByBaddie(this);
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

        //transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f); //original code
        transform.Rotate(rotateDir, m_PushBlockSettings.agentRotationSpeed * Time.fixedDeltaTime * 200/15f);
        m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed,
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
