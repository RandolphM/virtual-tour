using UnityEngine;
using System;

namespace Vodgets
{
    public class WorldGrabber : MonoBehaviour
    {
        public Controller controller_left;
        public Controller controller_right;
        public bool dolly_mode = true;
        public bool allow_scaling = true;

        Vector3 world_GrabLoc_left = Vector3.zero;
        Vector3 world_GrabLoc_right = Vector3.zero;
        Vector3 world_grab_vec = Vector3.zero;
        float world_grab_vec_len = 0f;

        bool leftGrabbing = false;
        bool rightGrabbing = false;

        Srt controller = new Srt();
        Srt child = new Srt();

        // Use this for initialization
        void Start()
        {
            // All grabbers support dolly_mode which constrains the up vector from changing.
            controller_left.Gripped += GrabLeft;
            controller_right.Gripped += GrabRight;
        }

        void GrabLeft(bool state)
        {
            if (state)
                GrabLeft();
            else
                UngrabLeft();
        }

        void GrabRight(bool state)
        {
            if (state)
                GrabRight();
            else
                UngrabRight();
        }

        void GrabLeft()
        {
            leftGrabbing = true;
            world_GrabLoc_left = controller_left.transform.position;

            if (rightGrabbing)
            {
                controller.Set((world_GrabLoc_left + world_GrabLoc_right) * 0.5f, Quaternion.identity, Vector3.one);
                world_grab_vec = world_GrabLoc_left - world_GrabLoc_right;
                if (dolly_mode)
                    world_grab_vec.y = 0f;
                world_grab_vec_len = world_grab_vec.magnitude;
            }
            else
            {
                controller.Set(world_GrabLoc_left, controller_left.transform.rotation, Vector3.one);
                if (dolly_mode)
                    controller.localRotation = Quaternion.FromToRotation(controller.localRotation * Vector3.up, Vector3.up) * controller.localRotation;
            }
            // Set world origin as child of controller
            child.Clear();
            TransformXtra.SiblingToChild(child, controller);
        }

        void GrabRight()
        {
            rightGrabbing = true;
            world_GrabLoc_right = controller_right.transform.position;

            if (leftGrabbing)
            {
                controller.Set((world_GrabLoc_left + world_GrabLoc_right) * 0.5f, Quaternion.identity, Vector3.one);
                world_grab_vec = world_GrabLoc_left - world_GrabLoc_right;
                if (dolly_mode)
                    world_grab_vec.y = 0f;
                world_grab_vec_len = world_grab_vec.magnitude;
            }
            else
            {
                controller.Set(world_GrabLoc_right, controller_right.transform.rotation, Vector3.one);
                if (dolly_mode)
                    controller.localRotation = Quaternion.FromToRotation(controller.localRotation * Vector3.up, Vector3.up) * controller.localRotation;

            }

            // Set world origin as child of controller
            child.Clear();
            TransformXtra.SiblingToChild(child, controller);
        }

        void UngrabLeft()
        {
            leftGrabbing = false;
            if (rightGrabbing)
                GrabRight();
        }

        void UngrabRight()
        {
            rightGrabbing = false;
            if (leftGrabbing)
                GrabLeft();
        }

        void Update()
        {
            if (leftGrabbing || rightGrabbing)
            {
                if (leftGrabbing && rightGrabbing)
                {
                    controller.localPosition = (controller_left.transform.position + controller_right.transform.position) * 0.5f;

                    Vector3 controller_vec = controller_left.transform.position - controller_right.transform.position;
                    if (dolly_mode)
                        controller_vec.y = 0f;

                    controller.localRotation = Quaternion.FromToRotation(world_grab_vec, controller_vec); ;

                    if (allow_scaling)
                        controller.localScale = Vector3.one * (controller_vec.magnitude / world_grab_vec_len);

                }
                else if (leftGrabbing)
                {
                    controller.localPosition = controller_left.transform.position;
                    controller.localRotation = controller_left.transform.rotation;
                }
                else
                {
                    controller.localPosition = controller_right.transform.position;
                    controller.localRotation = controller_right.transform.rotation;
                }

                if (dolly_mode)
                    controller.localRotation = Quaternion.FromToRotation(controller.localRotation * Vector3.up, Vector3.up) * controller.localRotation;

                // Convert human platform into grabbed world space
                TransformXtra.SiblingToChild(transform, controller);
                TransformXtra.SiblingToChild(transform, child);
            }
        }

    }
}