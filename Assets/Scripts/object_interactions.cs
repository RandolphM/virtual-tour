using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class object_interactions : MonoBehaviour {

    //steam controller for telport movement
    public SteamVR_TrackedController controller_left;
    public SteamVR_TrackedController controller_right;

    // Use this for initialization
    void Start()
    {
        controller_left.TriggerClicked += left_triggerpressed;
        controller_left.TriggerUnclicked += left_triggerUnpressed;
        controller_right.TriggerClicked += right_triggerpressed;
        controller_right.TriggerUnclicked += right_triggerUnpressed;
    }
    void left_triggerpressed(object sender, ClickedEventArgs e)
    {
        Debug.Log("Left trigger pressed");
    }
    void left_triggerUnpressed(object sender, ClickedEventArgs e)
    {
        Debug.Log("Left trigger not pressed");
    }
    void right_triggerpressed(object sender, ClickedEventArgs e)
    {
        Debug.Log("Right trigger pressed");
    }
    void right_triggerUnpressed(object sender, ClickedEventArgs e)
    {
        Debug.Log("Right trigger not pressed");
    }
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.SphereCast(controller_left.transform.position,0.03f,controller_left.transform.forward,out hit))
        {
            Debug.Log("hit object");
        }
        if (Physics.SphereCast(controller_right.transform.position, 0.03f, controller_right.transform.forward, out hit))
        {
            Debug.Log("hit object");
        }

    }
}
