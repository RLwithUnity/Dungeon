using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class fireball : MonoBehaviour
{
    public float duration;
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wall")
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed");
        }
        // ����ü�� ��������?

        // ����ü�� ���ư��鼭 ��ǥ�� �ٲ��ٵ� �� �Ÿ��� ��� ����� �� ���°�?

        // ��ġ ���� �ʿ�
        if (other.gameObject.tag == "spearman")
        {
            // ���Ǿ�� ü�� -10
            Destroy(this.gameObject);
        }

        if (other.gameObject.tag == "shieldman")
        {
            // ����� ü�� -1
            Destroy(this.gameObject);
        }
        if (other.gameObject.tag == "magician")
        {
            // ���Ǿ�� ü�� -30
            Destroy(this.gameObject);
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
