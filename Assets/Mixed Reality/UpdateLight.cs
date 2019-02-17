using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLight : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;

        //GameObject camera = GameObject.Find("TrackedCamera");
        /*CAREFUL THE INDEX THAT GETS THE CAMERA CHANGED IN 5.6 IS VERY IMPORT TO THE LIGHT*/
        position = Camera.allCameras[0].transform.InverseTransformPoint(position);
        GlobalScale.light_position[0] = position.x;
        GlobalScale.light_position[1] = position.y;
        GlobalScale.light_position[2] = position.z;
    }
}
