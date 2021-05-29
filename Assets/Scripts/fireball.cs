using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class fireball : MonoBehaviour
{
    public float duration;
    public SpearAgent spearagent;
    public ShieldAgent shieldagent;

    private List<string> tagList = new List<string>()
    {
        "shield",
        "wall",
        "spearAgent",
        "magicianAgent",
        "shieldAgent"
    };
    
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (tagList.Contains(other.gameObject.tag))
        {
            Destroy(this.gameObject);
            //Debug.Log("Destroyed");
        }

        if (other.GetComponent<SpearAgent>())
        {
            SpearAgent Agent = other.GetComponent<SpearAgent>();
            Agent.onDamage();
            //Debug.Log(Agent.health);
        }

        if (other.GetComponent<MagicianAgent>())
        {
            MagicianAgent Agent = other.GetComponent<MagicianAgent>();
            Agent.onDamage();
            //Debug.Log(Agent.health);
        }
        if (other.GetComponent<ShieldAgent>())
        {
            ShieldAgent Agent = other.GetComponent<ShieldAgent>();
            Agent.onDamage();
            //Debug.Log(Agent.health);
        }
    }

    void Start()
    {
        duration = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        duration += Time.deltaTime;
        Debug.Log(duration);
        if (duration > 1.0f)
            Destroy(this.gameObject);

    }
}
