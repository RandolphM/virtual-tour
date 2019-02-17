using UnityEngine;
using System.Collections;

public class ZEDAsTexture : MonoBehaviour {
	// Use this for initialization
	void Start () {

        gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", sl.zed.ZEDCamera.GetInstance().CreateTextureImageType(sl.zed.ZEDCamera.SIDE.LEFT));
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
