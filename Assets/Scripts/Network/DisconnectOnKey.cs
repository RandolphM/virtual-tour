using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Roninworks.VectorMath
{
    public class DisconnectOnKey : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            if (!PhotonNetwork.connected)
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.Disconnect();
                PhotonNetwork.LoadLevel("Launcher");
            }
        }
    }
}