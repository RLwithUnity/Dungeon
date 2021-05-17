using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class DungeonEscapeEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public SpearAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
    }

    [System.Serializable]
    public class DragonInfo
    {
        public DragonAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Collider Col;
        public bool IsDead;
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    private int m_ResetTimer;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    Material m_GroundMaterial; //cached on Awake()

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    public List<DragonInfo> DragonsList = new List<DragonInfo>();
    private Dictionary<SpearAgent, PlayerInfo> m_PlayerDict = new Dictionary<SpearAgent, PlayerInfo>();
    public bool UseRandomAgentRotation = true;
    public bool UseRandomAgentPosition = true;
    PushBlockSettings m_PushBlockSettings;

    private int m_NumberOfRemainingPlayers;
    public GameObject Key;
    public GameObject Tombstone;
    private SimpleMultiAgentGroup m_AgentGroup;
    private SimpleMultiAgentGroup m_DragonGroup;
    void Start()
    {

        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();

        //Reset Players Remaining
        m_NumberOfRemainingPlayers = AgentsList.Count;

        //Hide The Key
        Key.SetActive(false);

        // Initialize TeamManager
        m_AgentGroup = new SimpleMultiAgentGroup();
        m_DragonGroup = new SimpleMultiAgentGroup();
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            item.Col = item.Agent.GetComponent<Collider>();
            // Add to team manager
            m_AgentGroup.RegisterAgent(item.Agent);
        }
        foreach (var item in DragonsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Col = item.Agent.GetComponent<Collider>();
            m_DragonGroup.RegisterAgent(item.Agent);
        }

        ResetScene();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            GroupEpisodeInterrupted();
            ResetScene();
        }
    }
    /*
    public void PlayerTouchedHazard(PushAgentEscape agent)
    {
        m_NumberOfRemainingPlayers--;
        if (m_NumberOfRemainingPlayers == 0 || agent.IHaveAKey)
        {
            StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.failMaterial, 0.5f));
            EndGroupEpisode();
            ResetScene();
        }
        else
        {
            agent.gameObject.SetActive(false);
        }
    }
    */

    public void DragonTouchedHazard(DragonAgent agent)
    {
        m_DragonGroup.AddGroupReward(1f);
        EndGroupEpisode();
        ResetScene();
    }

    public void GetKey(SpearAgent agent, Collider col)
    {
        print("Picked up key");
        agent.MyKey.SetActive(true);
        agent.IHaveAKey = true;
        m_AgentGroup.AddGroupReward(0.3f);
        col.gameObject.SetActive(false);
    }

    public void UnlockDoor()
    {
        m_AgentGroup.AddGroupReward(0.4f);
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));

        print("Unlocked Door");
        EndGroupEpisode();

        ResetScene();
    }
    public void KillAgent()
    {
        m_DragonGroup.AddGroupReward(0.5f);
    }

    public void KilledByBaddie(SpearAgent agent)
    {
        m_NumberOfRemainingPlayers--;
        if (m_NumberOfRemainingPlayers == 0)
        {
            StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.failMaterial, 0.5f));
            EndGroupEpisode();
            ResetScene();
        }
        else
        {
            agent.gameObject.SetActive(false);
            // print($"{baddieCol.gameObject.name} ate {agent.transform.name}");

            //Spawn Tombstone
            Tombstone.transform.SetPositionAndRotation(agent.transform.position, agent.transform.rotation);
            Tombstone.SetActive(true);
        }
    }

    public void HitByWeapon(Collider dragonCol)
    {
        m_AgentGroup.AddGroupReward(0.3f);
        dragonCol.gameObject.SetActive(false);
        print($"{dragonCol.gameObject.name} is killed by weapon");

        //Spawn the Key Pickup
        Key.transform.SetPositionAndRotation(dragonCol.GetComponent<Collider>().transform.position, dragonCol.GetComponent<Collider>().transform.rotation);
        Key.SetActive(true);
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(3f, 0.01f, 3f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    public void BaddieTouchedBlock()
    {
        EndGroupEpisode();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.failMaterial, 0.5f));
        ResetScene();
    }

    public void EndGroupEpisode()
    {
        m_AgentGroup.EndGroupEpisode();
        m_DragonGroup.EndGroupEpisode();
    }

    public void GroupEpisodeInterrupted()
    {
        m_AgentGroup.GroupEpisodeInterrupted();
        m_DragonGroup.GroupEpisodeInterrupted();
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    void ResetScene()
    {

        //Reset counter
        m_ResetTimer = 0;

        //Reset Players Remaining
        m_NumberOfRemainingPlayers = AgentsList.Count;

        //Random platform rot
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var pos = UseRandomAgentPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomAgentRotation ? GetRandomRot() : item.StartingRot;

            item.Agent.transform.SetPositionAndRotation(pos, rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.Agent.MyKey.SetActive(false);
            item.Agent.IHaveAKey = false;
            item.Agent.gameObject.SetActive(true);
            m_AgentGroup.RegisterAgent(item.Agent);
        }

        //Reset Key
        Key.SetActive(false);

        //Reset Tombstone
        Tombstone.SetActive(false);

        //End Episode
        foreach (var item in DragonsList)
        {
            item.Agent.transform.SetPositionAndRotation(item.StartingPos, item.StartingRot);
            item.Agent.gameObject.SetActive(true);
            m_DragonGroup.RegisterAgent(item.Agent);
        }
    }
}