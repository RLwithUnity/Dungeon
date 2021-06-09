using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wall : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject explosionVFX;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "fireball")
        {
            Vector3 pos = collision.contacts[0].point;
            Quaternion rot = new Quaternion(0, 0, 0, 0);
            GameObject explosion = Instantiate(explosionVFX, pos, rot) as GameObject;
            Debug.Log("Coliision");
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
