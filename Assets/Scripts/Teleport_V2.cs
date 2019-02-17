using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport_V2 : MonoBehaviour {

    // camerarig to move around
    public Transform person_position = null;
    //steam controller for telport movement
    public Transform eye_position = null;
    public SteamVR_TrackedController controller_left;
    public SteamVR_TrackedController controller_right;
    //Render for interaction 
    public LineRenderer m_RRay;
    public LineRenderer m_LRay;
    public MeshRenderer arrowpart1;
    public MeshRenderer arrowpart2;
    public MeshRenderer arrowpart3;

    //public and private variables for the teleporting
    public GameObject teleport_indicator;
    public GameObject teleportfloor_area;
    private Vector3 n_position;
    private int layer_mask = 1 << 8;
    private bool teleport_use = false;
    private bool rightpad_use = false;
    private bool leftpad_use = false;
    private float max_len = 4.5f;

    // Use this for initialization
    void Start () {
        controller_left.PadClicked += leftpadpressed;
        controller_left.PadUnclicked += leftpadUnpressed;
        controller_right.PadClicked += rightpadpressed;
        controller_right.PadUnclicked += rightpadUnpressed;
        teleport_indicator.SetActive(false);
        teleportfloor_area.SetActive(false);
        teleport_use = false;
        m_RRay.enabled = false;
        m_LRay.enabled = false;
        
    }
    //Leftpad button press actions
    void leftpadpressed(object sender, ClickedEventArgs e)
    {
        if (!rightpad_use)
        {
            leftpad_use = true;
            m_LRay.enabled = true;
            //teleport_indicator.SetActive(true);
            //teleportfloor_area.SetActive(true);
            teleport_use = true;
        }
    }

    bool ishit = false;

    void leftpadUnpressed(object sender, ClickedEventArgs e)
    {
        if (!rightpad_use)
        {
            leftpad_use = false;
            m_LRay.enabled = false;
            //teleport_indicator.SetActive(false);
            //teleportfloor_area.SetActive(false);
            teleport_use = false;
            //Teleport to new position
            if (n_position != transform.position && ishit)
                teleport_fademovement();
        }
    }
    //rightpad button press actions
    void rightpadpressed(object sender, ClickedEventArgs e)
    {
       if(!leftpad_use)
        {
            rightpad_use = true;
            m_RRay.enabled = true;
            //teleport_indicator.SetActive(true);
            //teleportfloor_area.SetActive(true);
            teleport_use = true;
        }
    }

    void rightpadUnpressed(object sender, ClickedEventArgs e)
    {
        if (!leftpad_use)
        {
            rightpad_use = false;
            m_RRay.enabled = false;
            //teleport_indicator.SetActive(false);
            //teleportfloor_area.SetActive(false);
            teleport_use = false;
            //Teleport to new position
            if (n_position != transform.position && ishit)
                teleport_fademovement();
        }
    }
    void teleport_fademovement()
    {
        //Fade screen to black
        SteamVR_Fade.Start(Color.clear, 0f);
        SteamVR_Fade.Start(Color.black, 2.5f);
        //Move to the new position
        Vector3 position = n_position - eye_position.position;
        position.y = 0.0f;
        person_position.position += position;
        //Fade to screen back to what the player/person sees;
        SteamVR_Fade.Start(Color.black, 0f);
        SteamVR_Fade.Start(Color.clear, 2.5f);
    }

    void Update()
    {
        bool justHit = false;
        RaycastHit hit;
        if (Physics.Raycast(controller_right.transform.position, controller_right.transform.forward, out hit, max_len) && rightpad_use)
        {
            justHit = true;
            n_position = hit.point;
            if (rightpad_use && hit.collider.gameObject.layer == 8)
            {
                if (rightpad_use && m_RRay.enabled && teleport_use)
                {
                    m_RRay.material.color = Color.green;
                    arrowpart1.material.color = Color.green;
                    arrowpart2.material.color = Color.green;
                    arrowpart3.material.color = Color.green;

                    teleport_indicator.transform.position = hit.point;
                    teleport_indicator.transform.Rotate(0, 5, 0);
                }
                else
                {
                    m_RRay.enabled = true;
                    teleport_indicator.SetActive(true);
                    teleport_use = true;
                }
            }
            else if (rightpad_use && hit.collider.gameObject.layer != 8 )
            {
                m_RRay.enabled = false;
                n_position = transform.position;
                teleport_indicator.SetActive(false);
                teleport_use = false;
            }
        }
        else if (Physics.Raycast(controller_left.transform.position, controller_left.transform.forward, out hit, max_len) && leftpad_use)
        {
            justHit = true;
            n_position = hit.point;
            if (leftpad_use && hit.collider.gameObject.layer == 8)
            {
                if (leftpad_use && m_LRay.enabled && teleport_use)
                {
                    m_LRay.material.color = Color.green;
                    arrowpart1.material.color = Color.green;
                    arrowpart2.material.color = Color.green;
                    arrowpart3.material.color = Color.green;
                    teleport_indicator.transform.position = hit.point;
                    teleport_indicator.transform.Rotate(0, 5, 0);
                }
                else
                {
                    m_LRay.enabled = true;
                    teleport_use = true;
                }

            }
            else if (leftpad_use && hit.collider.gameObject.layer != 8)
            {
                m_LRay.enabled = false;
                n_position = transform.position;
                teleport_indicator.SetActive(false);
                teleport_use = false;
            }
        } else
        {
            justHit = false;
        }
        if ( ishit != justHit )
        {
            ishit = justHit;
            teleport_indicator.SetActive(ishit);
            teleportfloor_area.SetActive(ishit);
        }

        //if (leftpad_use && hit.collider.gameObject.layer == layer_mask)
        //{
        //    if (leftpad_use && m_LRay.enabled && teleport_use)
        //    {
        //        m_LRay.material.color = Color.green;
        //        arrowpart1.material.color = Color.green;
        //        arrowpart2.material.color = Color.green;
        //        arrowpart3.material.color = Color.green;
        //        teleport_indicator.transform.position = hit.point;
        //        teleport_indicator.transform.Rotate(0, 5, 0);
        //    }
        //    else
        //    {
        //        m_LRay.enabled = true;
        //        teleport_indicator.SetActive(true);
        //        teleport_use = true;
        //    }

        //}
        //else if (leftpad_use && hit.collider.gameObject.layer != layer_mask && hit.point == Vector3.zero)
        //{
        //    m_LRay.enabled = false;
        //    n_position = transform.position;
        //    teleport_indicator.SetActive(false);
        //    teleport_use = false;
        //}
        //else if (rightpad_use && hit.collider.gameObject.layer == 8)
        //{
        //    if (rightpad_use && m_RRay.enabled && teleport_use)
        //    {
        //        m_RRay.material.color = Color.green;
        //        arrowpart1.material.color = Color.green;
        //        arrowpart2.material.color = Color.green;
        //        arrowpart3.material.color = Color.green;

        //        teleport_indicator.transform.position = hit.point;
        //        teleport_indicator.transform.Rotate(0, 5, 0);
        //    }
        //    else
        //    {
        //        m_RRay.enabled = true;
        //        teleport_indicator.SetActive(true);
        //        teleport_use = true;
        //    }
        //}
        //else if (rightpad_use && hit.collider.gameObject.layer != 8 && hit.point == Vector3.zero)
        //{
        //    m_RRay.enabled = false;
        //    n_position = transform.position;
        //    teleport_indicator.SetActive(false);
        //    teleport_use = false;
        //}



    }

}
