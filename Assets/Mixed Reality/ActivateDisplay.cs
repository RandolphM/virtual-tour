using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisplay : MonoBehaviour {

    static bool started = false;
	// Use this for initialization
	void Start () {

#if !UNITY_EDITOR
        if(!started)
        {
            Display.displays[1].Activate();
            
        }
#endif

        started = true;
    }


    // Update is called once per frame
    void Update () {
		
	}
}
