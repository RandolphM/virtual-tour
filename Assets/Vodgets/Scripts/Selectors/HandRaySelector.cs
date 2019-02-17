using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace Vodgets {

    public class HandRaySelector : Selector
    {
        public bool show_ray = true;
        public bool show_marker = true;
        public bool distance_grab = true;
        public float distance_scale = 2f;
        public string ray_prefab_name = "cursor_blue_ray";
        public string marker_prefab_name = "cursor_red_sphere";

        Vector3 grabpos = Vector3.zero;
        static Object cursor_prefab = null;
        static Object marker_prefab = null;

        GameObject cursor_obj = null;
        GameObject marker_obj = null;

        public float ray_length = 10f;
        LineRenderer ray = null;
        Vector3 ray_max_pos = Vector3.zero;
        Vector3 local_hit_pt = Vector3.zero;

        float armscale = 1f;
        Vector3 headpos;

        protected override void SetCursor()
        {
            if (distance_grab && isGrabbing)
            {
                Vector3 p = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
                headpos = transform.parent.TransformPoint(p);

                float head_len = Vector3.Dot(ray.transform.forward, (transform.position - headpos) * distance_scale);
                float ray_len = armscale * head_len;

                //float ray_len = armscale * (transform.position - headpos).magnitude;

                //Vector3 dpos = transform.position - headpos;
                //dpos.y = 0f;
                //float ray_len = armscale * dpos.magnitude;


                cursor.localPosition = transform.position + (ray.transform.forward * ray_len);
                ray.SetPosition(1, ray.transform.InverseTransformPoint( cursor.localPosition ));

                if (marker_obj != null)
                    marker_obj.transform.position = cursor.localPosition;
            }
            else
            {
                cursor.localPosition = transform.TransformPoint(grabpos);
            }
            cursor.localRotation = transform.rotation;
       }

        private void OnEnable()
        {
            if (show_ray)
            {
                if (cursor_prefab == null)
                    cursor_prefab = Resources.Load(ray_prefab_name);
                if (cursor_prefab != null)
                {
                    cursor_obj = (GameObject)GameObject.Instantiate(cursor_prefab);
                    cursor_obj.transform.SetParent(transform, false);
                    cursor_obj.transform.localPosition = Vector3.zero;
                    cursor_obj.transform.localRotation = Quaternion.identity;

                    ray = cursor_obj.GetComponent<LineRenderer>();

                    ray_max_pos.z = ray_length;
                    ray.SetPosition(1, ray_max_pos);
                }
            }

            if (show_marker)
            {
                if (marker_prefab == null)
                    marker_prefab = Resources.Load(marker_prefab_name);
                if (marker_prefab != null)
                {
                    marker_obj = (GameObject)GameObject.Instantiate(marker_prefab);
                    marker_obj.transform.SetParent(transform, false);
                    marker_obj.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            //ReleaseController();

            if (cursor_obj != null)
                Destroy(cursor_obj);
        }

        private void FixedUpdate()
        {
            if (isGrabbing)
            {
                DoGrabbedUpdate();
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
                {

                    if (ray != null)
                    {
                        Vector3 p = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
                        headpos = transform.parent.TransformPoint(p);

                        float head_len = Vector3.Dot(ray.transform.forward, (transform.position - headpos)* distance_scale);
                        float ray_len = (hit.point - transform.position).magnitude;

                        //float head_len = (transform.position - headpos).magnitude;

                        //Vector3 dpos = transform.position - headpos;
                        //dpos.y = 0f;
                        //float head_len = dpos.magnitude;

                        armscale = ray_len / head_len;

                        local_hit_pt = ray.transform.InverseTransformPoint(hit.point);
                        ray.SetPosition(1, local_hit_pt);
                    }

                    if (marker_obj != null )
                    {
                        marker_obj.SetActive(true);
                        marker_obj.transform.position = hit.point;
                    }

                    grabpos = transform.InverseTransformPoint(hit.point);

                    DetectVodget(hit.transform.gameObject);

                    DoFocusUpdate();
                }
                else
                {
                    if (ray != null)
                        ray.SetPosition(1, ray_max_pos);

                    if (marker_obj != null)
                        marker_obj.SetActive(false);

                    DetectVodget(null);
                }


            }
        }
    }
}
