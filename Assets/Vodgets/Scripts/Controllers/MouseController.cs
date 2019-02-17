using UnityEngine;

namespace Vodgets
{
    [RequireComponent(typeof(Selector))]
    public class MouseController : Controller
    {
        Selector selector = null;
        private void Awake()
        {
            selector = GetComponent<Selector>();
        }

        private void OnMouseDown()
        {
           
        }

        private void OnMouseUp()
        {
            
        }

        private void OnMouseUpAsButton()
        {
            
        }

        public override Vector3 GetVelocity()
        {
            return Vector3.zero;
        }

        public override Vector3 GetAngularVelocity()
        {
            return Vector3.zero;
        }

        public override void Rumble(ushort microsecs)
        {
        }
    }
}
