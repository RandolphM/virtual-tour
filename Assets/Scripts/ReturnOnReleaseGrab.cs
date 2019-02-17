using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnOnReleaseGrab : Photon.MonoBehaviour
{
    /*For Grabbing*/
    public List<SteamVR_TrackedController> steamvr_controller_list;
    GameObject last_grabber = null;
    Vector3 grabbed_pos;
    Quaternion grabbed_rot;

    /*For Released Return*/
    public float return_time = 3.0f;
    Vector3 default_pos = Vector3.zero;
    Quaternion default_rot = Quaternion.identity;
    Vector3 released_pos = Vector3.zero;
    Quaternion released_rot = Quaternion.identity;
    float release_lerp = 1.0f;

    /*Number signifying if the object is currently being touched*/
    //int num_touching = 0;
    /*This is something I may return to or network later, 
     * but was problematic because of the OCULUS HOME ENTER/EXIT BUG documented below, 
     * it was only really used for color selection anyway*/

    /*If an object has a mesh renderer than change the colors a bit, this is primarily for debug purposes*/
    Color previous_color;/*BASIC COLORING FOR NOW*/

    private void Awake()
    {
        default_pos = transform.position;
    }
    // Use this for initialization
    void Start()
    {
        /*Checks Compatibility with Grabbing, This type of grabbing is designed for objects that are 
         * movable triggers which requires a collider(isTrigger=True) and a Rigidbody(isKinematic=True)*/

        bool fail = true;
        Collider col = GetComponent<Collider>();
        if (col && col.isTrigger)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb && rb.isKinematic)
                fail = false;
        }

        if (fail)
            print("WARNING: Object Should have components Collider with IsTrigger = True and a RigidBody with IsKinematic = True");

        foreach (SteamVR_TrackedController controller in steamvr_controller_list)
        {
            fail = true;
            col = controller.gameObject.GetComponent<Collider>();
            if (col && col.isTrigger)
            {
                Rigidbody rb = controller.gameObject.GetComponent<Rigidbody>();
                if (rb && rb.isKinematic)
                    fail = false;
            }

            if (fail)
                print("WARNING: Controller Should have components Collider(probably with IsTrigger = true) and a RigidBody(probably with IsKinematic = True)");
        }

        /*DEBUG COLORING FOR NOW*/
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        if (mesh_renderer)
        {
            previous_color = mesh_renderer.material.color;
        }
    }
    /*BUG OCULUS... GOING INTO OCULUS HOME AND RETURNING TO UNITY APPLICATION BORKS 1 to 1 Trigger Enter->Exit Behavior
     * Upon going into Oculus Home there is no Trigger Exit, but Upon Returning from Home to Unity Application you receive a redundant trigger Enter.
     * This doesn't Break grabbing, but does cause bugs if you expect 1 to 1 enter exits here.
     * 
     * This doesn't seem to happen at all with SteamVR Menus.
     * POSSIBLE CAUSE MAY BE RELATED TO HOW VR TRACKED CONTROLLER OBJECTS ARE HANDLED BETWEEN THE 2 PLATFORMS
     */
    private void OnTriggerEnter(Collider other)
    {
        foreach (SteamVR_TrackedController controller in steamvr_controller_list)
        {
            if(controller.gameObject == other.gameObject)
            {
                print("ON RETURN GRAB SELECTION TRIGGER ENTER");
                SteamVR_Controller.Input((int)controller.controllerIndex).TriggerHapticPulse(3999);

                //num_touching++;
                //if (num_touching == 1)
                //{
                //    MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
                //    if (mesh_renderer)
                //    {
                //        previous_color = mesh_renderer.material.color;
                //        mesh_renderer.material.color = new Color(0.5f, 0.0f, 0.0f, 0.3f);
                //    }
                //}

                /*BASIC COLORING FOR NOW*/
                MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
                if (mesh_renderer)
                {
                    mesh_renderer.material.color = new Color(0.5f, 0.0f, 0.0f, 0.3f);
                }

                break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        foreach (SteamVR_TrackedController controller in steamvr_controller_list)
        {
            if (controller.gameObject == other.gameObject)
            {
                bool pressed_down = controller.gripped;//SteamVR_Controller.Input((int)controller.controllerIndex).GetHairTriggerDown();
                if (pressed_down && !last_grabber)
                {
                    last_grabber = other.gameObject;

                    Transform grabbed_at = last_grabber.transform;
                    grabbed_pos = grabbed_at.InverseTransformPoint(transform.position);
                    grabbed_rot = Quaternion.Inverse(grabbed_at.rotation) * transform.rotation;

                    release_lerp = 1.0f;

                    /*BASIC COLORING FOR NOW*/
                    MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
                    if (mesh_renderer)
                    {
                        mesh_renderer.material.color = previous_color;
                    }

                    /* NETWORKING
                     * This is an example of taking ownership of an object that has this photon view, immediate isMine ownership checks can fail due to latency
                     * Object must have TakeOver property set on its PhotonView Script settings in the Unity Editor.
                     */
                    PhotonView pv = GetComponent<PhotonView>();
                    if(pv)
                        pv.RequestOwnership();
                    /*END NETWORKING*/
                }

                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (SteamVR_TrackedController controller in steamvr_controller_list)
        {
            if (controller.gameObject == other.gameObject)
            {
                print("ON RETURN GRAB SELECTION TRIGGER EXIT");
                //num_touching--;
                //if (num_touching == 0)
                //{
                    /*BASIC COLORING FOR NOW*/
                    MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
                    if (mesh_renderer)
                    {
                        mesh_renderer.material.color = previous_color;
                    }
                //}

                break;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        /* NETWORKING
         * This is an example of stopping any attempts at state control if a client doesn't have ownership of the object photon view.  A few things are being handled here.
         * Because the ownership request doesn't transfer instantly because of latency, this means it can still be detected as not being owned by this client shortly after 
         * the take control request in OnTriggerStay above.  
         * Because of this, you need to be careful not to use isMine checks to do anything but ignore manipulations of the object state, such as the transform.
         * This grabbing script simply ignores object state changes and properly resets state upon making the take control control request.
         */
        PhotonView pv = GetComponent<PhotonView>();
        if (pv && !pv.isMine)
        {
            return;
        }
        /*END NETWORKING*/

        if (last_grabber)
        {
            bool still_down = last_grabber.GetComponent<SteamVR_TrackedController>().gripped;//SteamVR_Controller.Input((int)last_grabber.GetComponent<SteamVR_TrackedController>().controllerIndex).GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
            if (still_down)
            {
                Transform grabbed_at = last_grabber.gameObject.transform;
                transform.position = grabbed_at.TransformPoint(grabbed_pos);
                transform.rotation = grabbed_at.transform.rotation * grabbed_rot;
            }
            else
            {
                last_grabber = null;
                release_lerp = 0.0f;
                released_pos = transform.position;
                released_rot = transform.rotation;
            }
        }
        else if (release_lerp < 1.0f)
        {
            release_lerp += Time.deltaTime * 1.0f / return_time;
            if (release_lerp > 1.0f)
                release_lerp = 1.0f;

            transform.position = Vector3.Lerp(released_pos, default_pos, release_lerp);
            transform.rotation = Quaternion.Slerp(released_rot, default_rot, release_lerp);
        }
    }
}
