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
        // 투사체의 시작지점?

        // 투사체가 날아가면서 좌표가 바뀔텐데 그 거리를 계속 계산할 수 없는가?

        // 수치 조절 필요
        if (other.gameObject.tag == "spearman")
        {
            // 스피어맨 체력 -10
            Destroy(this.gameObject);
        }

        if (other.gameObject.tag == "shieldman")
        {
            // 쉴드맨 체력 -1
            Destroy(this.gameObject);
        }
        if (other.gameObject.tag == "magician")
        {
            // 스피어맨 체력 -30
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
