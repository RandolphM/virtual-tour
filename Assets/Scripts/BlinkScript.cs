using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkScript : MonoBehaviour 
{

    float timer;
    float waitTime = 0.5f;
    float resetPoint;
    public bool isOn;
    public Material mTerial;
    public Vector3 scale;

    void Start()
    {
        scale = this.transform.localScale;
        mTerial = GetComponent<Renderer>().material;
        mTerial.color = Color.blue;
		resetPoint = waitTime * 2;
    }
    
    // Update is called once per frame
    void Update () 
    {
        timer += Time.deltaTime;

        if (timer < waitTime) { mTerial.color = Color.white; }

        if (timer > waitTime) { mTerial.color = Color.red; }

        if (timer > resetPoint) { timer = 0; }

        if (this.transform.localScale.x < .1f)
            this.transform.localScale += new Vector3(.0001f, .0001f, .0001f);
        else
            this.transform.localScale = scale;
	}
}
