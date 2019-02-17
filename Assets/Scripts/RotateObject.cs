using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{

    Vector3 startPos;
    Transform currentPos;
    // Use this for initialization
    void Awake()
    {
        //startPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, 0, 80 * Time.deltaTime);
        //currentPos = GetComponent<Transform>().Find("Arrow");
        //Debug.Log(currentPos.position.x);

        //if (this.gameObject.name == "Arrow")
        //{
        //    if (currentPos.position.x < startPos.x + 2)
        //        this.transform.Translate(0, 0, Time.deltaTime * -1, Space.World);
        //    else
        //        this.transform.position = startPos;
        //}

        //if (this.gameObject.name == "Down Arrow")
        //{
        //    if (this.transform.position.x < 0f && this.transform.position.y > -0.2)
        //        this.transform.Translate(Time.deltaTime * .5f, Time.deltaTime * -.5f, 0, Space.World);
        //    else
        //        this.transform.position = startPos;
        //}
    }
}
