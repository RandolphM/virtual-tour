using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets {

    public class MouseRaySelector : Selector
    {
        public bool show_marker = true;
        public string marker_prefab_name = "cursor_red_sphere";

        Vector3 grabpos = Vector3.zero;
        static Object marker_prefab = null;

        GameObject marker_obj = null;

        public float ray_length = 10f;

        protected override void SetCursor()
        {
            cursor.localPosition = transform.TransformPoint(grabpos);
            cursor.localRotation = transform.rotation;
        }

        private void OnEnable()
        {

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
            if (marker_obj != null)
                Destroy(marker_obj);
        }

        private void FixedUpdate()
        {
            if (isGrabbing)
            {
                DoGrabbedUpdate();
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10))
                {

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
                    if (marker_obj != null)
                        marker_obj.SetActive(false);

                    DetectVodget(null);
                }
            }
        }
    }
}
