using UnityEngine;

namespace Vodgets
{

    public class Grabber : Vodget
    {
        public bool dolly_mode = false;
        public bool yaw_mode = false;
        //public bool allow_scaling = true;
        public float velocity_scale = 100f;
        public float breaking_dist = 0.75f;
        //public UnityEvent grab_evt;
        //public UnityEvent release_evt;


        Srt cursor = new global::Srt();
        Srt obj_child = new global::Srt();

        bool save_useGravity = true;
        bool save_isKinematic = true;
        //float save_drag = 0f;
        //float save_angdrag = 0f;

        // Note: Only allow the object to be grabbed by one hand at a time (for now).
        // The first hand that grabs an object must release before another hand can grab.
        protected Selector grabbing_selector = null;

        void SetCursor( Selector selector )
        {
            cursor.Set(selector.Cursor);
            if (dolly_mode)
            {
                if (yaw_mode)
                    cursor.localRotation = Quaternion.identity;
                else
                    cursor.localRotation = Quaternion.FromToRotation(cursor.localRotation * Vector3.up, Vector3.up) * cursor.localRotation;
            }
        }

        Srt obj_world = new Srt();
        public void Regrab(Selector selector)
        {
            SetCursor(selector);
            // Lossy scale is correct if object is not skewed by a rotation up the tree.
            obj_world.Set(transform.position, transform.rotation, transform.lossyScale);
            obj_child.Set(obj_world);
            obj_child.TransformUp(cursor);
        }

        Srt grab_loc = new Srt();

        public override void DoGrab(Selector selector, bool state)
        {
            base.DoGrab(selector, state);

            if (state)
            {
                if (grabbing_selector == null)
                {
                    Rigidbody body = GetComponent<Rigidbody>();
                    if (body != null)
                    {
//#if USE_VELOCITY
                        save_useGravity = body.useGravity;
                        body.useGravity = false;
//#else
//                        save_drag = body.drag;
//                        save_angdrag = body.angularDrag;

//                        save_useGravity = body.useGravity;
//                        body.useGravity = true;
//#endif
                        save_isKinematic = body.isKinematic;
                        body.isKinematic = false;
                    }

                    //#if USE_VELOCITY
                    RotationMover mover = gameObject.GetComponent<RotationMover>();
                    if (mover)
                        Destroy(mover);
                    //#endif

                    Regrab(selector);

                    grab_loc.Set(cursor);
                    grab_loc.TransformUp(obj_world);

                    grabbing_selector = selector;

                    //grab_evt.Invoke();
                }
            } else
            {
                if (grabbing_selector == selector)
                {
                    Rigidbody body = GetComponent<Rigidbody>();
                    if (body != null)
                    {
                        body.useGravity = save_useGravity;

                        body.velocity = (save_isKinematic) ? Vector3.zero : selector.GetVelocity();
                        body.angularVelocity = (save_isKinematic || dolly_mode) ? Vector3.zero : selector.GetAngularVelocity();
                        //body.angularVelocity = Vector3.zero;

//#if USE_VELOCITY
                        if (dolly_mode && gameObject.GetComponent<RotationMover>() == null)
                        {
                            RotationMover mover = gameObject.AddComponent<RotationMover>();
                            mover.Set(yaw_mode, save_isKinematic);
                        } else
                        {
                            body.isKinematic = save_isKinematic;
                        }
//#else
//                        body.isKinematic = save_isKinematic;
//                        body.drag = save_drag;
//                        body.angularDrag = save_angdrag;
//#endif
                    }

                    grabbing_selector = null;
                    //release_evt.Invoke();

                }
            }
        }

        public override void DoUpdate(Selector selector)
        {
            if (selector == grabbing_selector)
            {
                SetCursor(selector);

                Rigidbody body = GetComponent<Rigidbody>();

                if (body == null)
                {
                    obj_world.Set(obj_child);
                    obj_world.TransformDown(cursor);
                    transform.position = obj_world.localPosition;
                    transform.rotation = obj_world.localRotation;
                } else
                {
                    obj_world.Set(transform.position, transform.rotation, transform.lossyScale);
                    Srt world_grab_loc = new Srt(grab_loc);
                    world_grab_loc.TransformDown(obj_world);

                    Vector3 ds = cursor.localPosition - world_grab_loc.localPosition;

//#if USE_VELOCITY
                    if (ds.magnitude > breaking_dist)
                    {
                        DoGrab(grabbing_selector, false);

                        if (dolly_mode && gameObject.GetComponent<RotationMover>() == null) {
                            RotationMover mover = gameObject.AddComponent<RotationMover>();
                            mover.Set(yaw_mode, save_isKinematic);
                        }

                        return;
                    }

                    body.velocity = ds * velocity_scale;

                    Quaternion dq = cursor.localRotation * Quaternion.Inverse(world_grab_loc.localRotation);
                    float angle;
                    Vector3 axis;
                    dq.ToAngleAxis(out angle, out axis);
                    body.angularVelocity = axis * angle;

                    //    body.velocity = Vector3.zero;
                    //    body.angularVelocity = Vector3.zero;
                    //    body.MovePosition(obj_world.localPosition);
                    //    body.MoveRotation(obj_world.localRotation);
//#else
//                    body.AddForceAtPosition(ds * velocity_scale, world_grab_loc.localPosition, ForceMode.Force);

//                    Quaternion dq = cursor.localRotation * Quaternion.Inverse(world_grab_loc.localRotation);
//                    float angle;
//                    Vector3 axis;
//                    dq.ToAngleAxis(out angle, out axis);
//                    body.angularVelocity = axis * angle;

//                    body.AddTorque(axis * angle * velocity_scale * 0.1f);
//#endif
                }

            }
        }

    }
}