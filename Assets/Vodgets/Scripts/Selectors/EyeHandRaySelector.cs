using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace Vodgets
{
    public class EyeHandRaySelector : Selector
    {
        public bool isRightEyeDominant = true;
        public float max_dist = 10f;

        // The tip can be used to offset the tracked controller grab location.
        // It should be set during configuration. Setting dynamically allows creation of the default cursor.
        public Transform tip = null;
        public bool create_jack_cursor = true;
        public bool create_jump_cursor = true;

        static Object cursor_jack_prefab = null;
        static Object cursor_ball_prefab = null;
        GameObject cursor_jack_obj = null;
        GameObject cursor_ball_obj = null;

        Transform cameraRig = null;
        Vector3 dir = Vector3.zero;
        Vector3 eyepos = Vector3.zero;
        //Vector3 cursor_position = Vector3.zero;
        float armscale = 1f;

        protected override void SetCursor()
        {

            eyepos = UnityEngine.XR.InputTracking.GetLocalPosition((isRightEyeDominant) ? UnityEngine.XR.XRNode.RightEye : UnityEngine.XR.XRNode.LeftEye);
            eyepos = cameraRig.TransformPoint(eyepos);

            if (tip != null)
            {
                cursor.localPosition = eyepos + ((tip.position - eyepos) * armscale);
                cursor.localRotation = tip.rotation;
            }
            else
            {
                cursor.localPosition = eyepos + ((transform.position - eyepos) * armscale);
                cursor.localRotation = transform.rotation;
            }

            if (cursor_ball_obj != null)
                cursor_ball_obj.transform.position = cursor.localPosition;
        }

        private void OnEnable()
        {
            if (create_jack_cursor)
            {
                if (cursor_jack_prefab == null)
                    cursor_jack_prefab = Resources.Load("cursor_green_jack");
                if (cursor_jack_prefab != null)
                {
                    cursor_jack_obj = (GameObject)GameObject.Instantiate(cursor_jack_prefab);
                    cursor_jack_obj.transform.SetParent(transform, false);
                    cursor_jack_obj.transform.localPosition = Vector3.zero;
                    cursor_jack_obj.transform.localRotation = Quaternion.identity;
                }
            }

            if (create_jump_cursor)
            {
                if (cursor_ball_prefab == null)
                    cursor_ball_prefab = Resources.Load("cursor_red_sphere");
                if (cursor_ball_prefab != null)
                {
                    cursor_ball_obj = (GameObject)GameObject.Instantiate(cursor_ball_prefab);
                    cursor_ball_obj.transform.SetParent(transform, false);
                    cursor_ball_obj.transform.localPosition = Vector3.zero;
                    cursor_ball_obj.transform.localRotation = Quaternion.identity;
                }
            }

            cameraRig = transform.parent;
        }

        private void OnDisable()
        {
            //ReleaseController();

            if (cursor_jack_obj != null)
                Destroy(cursor_jack_obj);
            if (cursor_ball_obj != null)
                Destroy(cursor_ball_obj);
        }

       //Vector3 grabpos = Vector3.zero;

        private void FixedUpdate()
        {
            if (isGrabbing)
            {
                DoGrabbedUpdate();
            }
            else
            {
                Vector3 p = UnityEngine.XR.InputTracking.GetLocalPosition((isRightEyeDominant) ? UnityEngine.XR.XRNode.RightEye : UnityEngine.XR.XRNode.LeftEye);
                eyepos = transform.parent.TransformPoint(p);

                Vector3 tippos = (tip != null) ? tip.position : transform.position;
                dir = (tippos - eyepos).normalized;

                RaycastHit hit;
                bool hitfound = Physics.Raycast(eyepos, dir, out hit, 10f);
                Vector3 hitdir = hit.point - tippos;
                bool usehitpt = Vector3.Dot(hitdir, dir) > 0f;
                Vector3 grabpt = (usehitpt) ? hit.point : tippos;

                if (hitfound && (! usehitpt || hitdir.magnitude < max_dist) )
                {
                    armscale = (grabpt - eyepos).magnitude / (tippos - eyepos).magnitude;
                    cursor.localPosition = grabpt;
                    if (cursor_ball_obj != null)
                        cursor_ball_obj.transform.position = hit.point;
                    DetectVodget(hit.collider.transform.gameObject);

                    DoFocusUpdate();
                }
                else
                {
                    if (cursor_ball_obj != null)
                        cursor_ball_obj.transform.localPosition = Vector3.zero;
                    DetectVodget(null);
                }

            }
        }
    }
}