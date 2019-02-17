using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to be placed on Head under SteamVR CameraRig node
namespace Vodgets
{
    public class HeadRaySelector : Selector
    {
#if false

        GameObject jump_cursor = null;
        Vector3 grabbing_pos = Vector3.zero;
        float elapsed_secs = 0f;
        bool isGrabbing = false;

        protected override void SetCursor()
        {

        }

        // Use this for initialization
        void Start()
        {
            jump_cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jump_cursor.transform.SetParent(null);
            jump_cursor.transform.localScale = Vector3.one * 0.075f;
            jump_cursor.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Blue.mat", typeof(Material)) as Material;
        }

        private void OnTriggerEnter(Collider other)
        {
            DetectVodget(other.gameObject.GetComponent<Vodget>());
        }
        private void OnTriggerExit(Collider other)
        {
            DetectVodget(null);
        }

        private void FixedUpdate()
        {
            if (curr_vodget != null && !isGrabbing)
            {
                elapsed_secs += Time.deltaTime;
                if (elapsed_secs > 1f)
                {
                    curr_vodget.DoGrab(null);
                    elapsed_secs = 0f;
                }
            }
            if (isGrabbing)
            {
                //curr_vodget.DoUpdate(null);
                elapsed_secs += Time.deltaTime;
                if (elapsed_secs > 1f)
                {
                    isGrabbing = false;
                    elapsed_secs = 0f;
                    curr_vodget = null;
                }
            } else
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
                {
                    jump_cursor.transform.position = hit.point;
                    grabbing_pos = hit.point;
                    DetectVodget(hit.transform.gameObject.GetComponent<Vodget>());
                }
                else
                {
                    jump_cursor.transform.position = Vector3.zero;
                }
            }
        }
#endif

    }
}