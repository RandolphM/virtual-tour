using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonMove : MonoBehaviour
{
    bool isTop = true;
    int min = 1, max = 5;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < max && isTop == false)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime, transform.position.z);
            if (transform.position.y >= max) isTop = true;
        }

        if (transform.position.y > min && isTop == true)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime, transform.position.z);
            if (transform.position.y <= min) isTop = false;
        }
    }

    //while (transform.position.y > 1 && isTop == false)
    //{
    //    transform.position = new Vector3(0, transform.position.y - Time.deltaTime);
    //    if (transform.position.y <= 1)
    //        isTop = false;
    //}
}

