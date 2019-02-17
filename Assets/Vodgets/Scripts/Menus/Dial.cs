using UnityEngine;
using UnityEngine.Events;

namespace Vodgets
{

    public class Dial : Vodget
    {
        public Vector3 notch_dir;
        public Vector3 right_dir;
        public Vector3 spin_dir;

        public float curr_val;

        [System.Serializable]
        public class DialEvent : UnityEvent<float> { }

        //[SerializeField]
        public DialEvent dial_changed;

        Vector3 grab_dir;

        private void Start()
        {
            spin_dir.Normalize();
            notch_dir.Normalize();
            dial_changed.Invoke(ComputeAngle());
        }

        public override void DoGrab(Selector selector, bool state)
        {
            grab_dir = transform.InverseTransformPoint(selector.Cursor.localPosition);
            grab_dir -= spin_dir * Vector3.Dot(spin_dir, grab_dir);
            //grab_dir.z = 0f;
            grab_dir.Normalize();

        }

        float ComputeAngle()
        {
            Vector3 dir = transform.localRotation * notch_dir;
            float proj = Vector3.Dot(dir, notch_dir);
            float proj2 = Vector3.Dot(dir, right_dir);

            float angle = Mathf.Acos(proj) * ((proj2 > 0f) ? 1f : -1f) * 180.0f / Mathf.PI;
            return angle;
        }

        public override void DoUpdate(Selector selector)
        {
            Vector3 curr_dir = transform.InverseTransformPoint(selector.Cursor.localPosition);
            curr_dir -= spin_dir * Vector3.Dot(spin_dir, curr_dir);
            //curr_dir.z = 0f;
            curr_dir.Normalize();

            transform.localRotation *= Quaternion.FromToRotation(grab_dir, curr_dir);

            dial_changed.Invoke(ComputeAngle());
        }
    }
}