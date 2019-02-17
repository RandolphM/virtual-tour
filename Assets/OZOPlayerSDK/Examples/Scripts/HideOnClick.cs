using UnityEngine;
using System.Collections;

public class HideOnClick : MonoBehaviour
{
	// Update is called once per frame
	void Update()
	{
		bool pressed = Input.GetButton("Fire1");
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || pressed)
		{
			GetComponent<Renderer>().enabled = false;
		}
	}
}
