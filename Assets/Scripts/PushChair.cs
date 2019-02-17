using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushChair : MonoBehaviour {

    public Transform chair;
    public Transform offset;

    //Vector3 current_pos;
    //Vector3 offset_vec;
    

	// Use this for initialization
	void Start () {
       
	}
	
	// Update is called once per frame
	void LateUpdate () {

        Vector3 error = transform.position - offset.position;
        error.y = 0.0f;
        //error.z = 0.0f;
        chair.position += error;

        error = transform.position - offset.position;
        error.y = 0.0f;
        error.z = 0.0f;

        //Debug.Log(error);
    }
}
