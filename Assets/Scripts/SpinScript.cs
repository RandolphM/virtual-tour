using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour {

    #region Rotate
    float sensitivity = 0.1f;
    Vector3 mouseRef;
    Vector3 mouseOffset;
    Vector3 rotation = Vector3.zero;
    bool isRotating;
    #endregion

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isRotating){
            mouseOffset = (Input.mousePosition - mouseRef);

            rotation.y = -(mouseOffset.x + mouseOffset.y) * sensitivity;
            transform.Rotate(rotation);
            transform.eulerAngles += rotation;
        }
	}

    private void OnMouseDown()
    {
        isRotating = true;
        mouseRef = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        isRotating = false;
    }
}
