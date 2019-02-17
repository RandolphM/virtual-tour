using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hand_anim : Photon.MonoBehaviour
{

    public Animator controller;
    public float max_arm = 0.3f;

    Vector3 Head_vect;
    Vector3 Hand_vect;
    Vector3 distance;
    float length;
    float point_anim = 0.0f;

    // Use this for initialization
    void Start()
    {
        //controller.SetFloat("Point", 1.0f);
        //controller.SetFloat("Grab", 0.0f);

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView || photonView.isMine)
        {
            GameObject head = GameObject.FindGameObjectWithTag("Head");
            GameObject lefthand = GameObject.FindGameObjectWithTag("LeftH");
            GameObject righthand = GameObject.FindGameObjectWithTag("RightH");



            if (lefthand && this.gameObject.name == "Hand_lf")
                Hand_vect = lefthand.transform.position;
            else if (righthand && this.gameObject.name == "Hand_rt")
                Hand_vect = righthand.transform.position;


            Head_vect = head.transform.position;

            distance = Hand_vect - Head_vect;
            distance.y = 0.0f;
            length = distance.magnitude;
            //if (length > max_arm)
            //    length = max_arm;
            float percent = length / max_arm;
            //Debug.Log(head.transform.position.ToString());
            //Debug.Log(hand.transform.position.ToString());
            float input = percent - 0.3f;
            if (input > 1.0f)
                input = 1.0f;
            point_anim = input;
            
        }

        controller.SetFloat("Point", point_anim);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(point_anim);

        }
        else
        {
            point_anim = (float)stream.ReceiveNext();

        }
    }

}
