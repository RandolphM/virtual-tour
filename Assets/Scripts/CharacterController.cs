using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    bool forward = Input.GetKey(KeyCode.W);
    bool left = Input.GetKey(KeyCode.A);
    bool right = Input.GetKey(KeyCode.D);
    bool back = Input.GetKey(KeyCode.S);

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxis("Horizontal") * 10 * Time.deltaTime;
        var z = Input.GetAxis("Vertical") * 5 * Time.deltaTime;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
    }
}
