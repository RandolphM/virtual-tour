using UnityEngine;
using UnityEngine.Events;

namespace Vodgets
{
    public class Slider : Vodget
    {
        public Vector3 from_pos;
        public Vector3 to_pos;
        public float min_val;
        public float max_val;
        public float curr_val;

        Vector3 dir;
        float dist; 

        [System.Serializable]
        public class SliderEvent : UnityEvent<float> { }

        [SerializeField]
        public SliderEvent slider_changed;

        float grab_proj = 0f;

        private void Start()
        {
            dir = (to_pos - from_pos ).normalized;
            dist = (to_pos - from_pos ).magnitude;
            slider_changed.Invoke(ComputeVal());
        }

        public override void DoGrab(Selector selector, bool state)
        {
            Vector3 dpos = transform.InverseTransformPoint(selector.Cursor.localPosition) - from_pos;
            grab_proj = Vector3.Dot(dpos, dir);
        }

        float ComputeVal()
        {
            float curr_handle = Vector3.Dot(transform.localPosition - from_pos, dir);
            curr_val = min_val + (curr_handle / dist) * (max_val - min_val);
            return curr_val;
        }

        public override void DoUpdate(Selector selector)
        {   
            Vector3 dpos = transform.InverseTransformPoint(selector.Cursor.localPosition) - from_pos;
            float curr_proj = Vector3.Dot(dpos, dir);
            float dproj = curr_proj - grab_proj;

            float curr_handle = Vector3.Dot(transform.localPosition - from_pos, dir);
            float next_handle = Mathf.Clamp(curr_handle + dproj, 0f, dist);
            dproj = next_handle - curr_handle;

            transform.localPosition += (dir * dproj);

            curr_val = min_val + (next_handle / dist) * (max_val - min_val);
            slider_changed.Invoke(ComputeVal());
        }

    }
}