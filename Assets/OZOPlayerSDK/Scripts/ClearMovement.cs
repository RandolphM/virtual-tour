using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearMovement : MonoBehaviour {
    private GameObject head;

	// Use this for initialization
	void Start () {
        head = transform.GetChild(0).gameObject;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 p = -head.transform.localPosition;
        transform.localPosition = p;
    }
}
