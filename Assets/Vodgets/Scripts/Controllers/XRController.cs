using UnityEngine;
using UnityEngine.VR;

namespace Vodgets
{
    class XRButtonSource
    {
        Controller controller;
        Selector.ButtonHandler handler;
        public XRButtonSource(Controller c, Selector.ButtonHandler b)
        {
            controller = c;
            handler = b;
            switch (handler.button)
            {
                case Selector.Button.Trigger:
                    //steam_controller.TriggerClicked += OnButtonDown;
                    //steam_controller.TriggerUnclicked += OnButtonUp;
                    break;
                case Selector.Button.Menu:
                    //steam_controller.MenuButtonClicked += OnButtonDown;
                    //steam_controller.MenuButtonUnclicked += OnButtonUp;
                    break;
                case Selector.Button.PadClicked:
                    //steam_controller.PadClicked += OnButtonDown;
                    //steam_controller.PadUnclicked += OnButtonUp;
                    break;
                case Selector.Button.PadTouched:
                    //steam_controller.PadTouched += OnButtonDown;
                    //steam_controller.PadUntouched += OnButtonUp;
                    break;
                case Selector.Button.Gripped:
                    //steam_controller.Gripped += OnButtonDown;
                    //steam_controller.Ungripped += OnButtonUp;
                    break;
            }
        }

        ~XRButtonSource()
        {
            switch (handler.button)
            {
                case Selector.Button.Trigger:
                    //steam_controller.TriggerClicked -= OnButtonDown;
                    //steam_controller.TriggerUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.Menu:
                    //steam_controller.MenuButtonClicked -= OnButtonDown;
                    //steam_controller.MenuButtonUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.PadClicked:
                    //steam_controller.PadClicked -= OnButtonDown;
                    //steam_controller.PadUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.PadTouched:
                    //steam_controller.PadTouched -= OnButtonDown;
                    //steam_controller.PadUntouched -= OnButtonUp;
                    break;
                case Selector.Button.Gripped:
                    //steam_controller.Gripped -= OnButtonDown;
                    //steam_controller.Ungripped -= OnButtonUp;
                    break;
            }
        }

        protected virtual void OnButtonDown(object sender, ClickedEventArgs e)
        {
            handler.DoButtonDown();
            controller.SetButton(handler.button, true);
        }

        protected virtual void OnButtonUp(object sender, ClickedEventArgs e)
        {
            handler.DoButtonUp();
            controller.SetButton(handler.button, false);
        }
    }

    public class XRController : Controller
    {
        public UnityEngine.XR.XRNode node;

        XRButtonSource trigger_src = null;
        XRButtonSource menu_src = null;
        XRButtonSource padClicked_src = null;
        XRButtonSource padTouched_src = null;
        XRButtonSource gripped_src = null;

        private void Awake()
        {
            Selector s = GetComponent<Selector>();
            // Wire up events
            trigger_src = new XRButtonSource(this, s.GetHandler(Selector.Button.Trigger));
            menu_src = new XRButtonSource(this, s.GetHandler(Selector.Button.Menu));
            padClicked_src = new XRButtonSource(this, s.GetHandler(Selector.Button.PadClicked));
            padTouched_src = new XRButtonSource(this, s.GetHandler(Selector.Button.PadTouched));
            gripped_src = new XRButtonSource(this, s.GetHandler(Selector.Button.Gripped));
        }

        private void OnDestroy()
        {
            trigger_src = null;
            menu_src = null;
            padClicked_src = null;
            padTouched_src = null;
            gripped_src = null;
        }

        private void Update()
        {
            if (UnityEngine.XR.XRDevice.isPresent)
            {
                transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(node);
                transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(node);

                System.Collections.Generic.List<UnityEngine.XR.XRNodeState> nodeStates = new System.Collections.Generic.List<UnityEngine.XR.XRNodeState>();
                UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);
            }
        }

        public override Vector3 GetVelocity()
        {


            //if (device == null)
            //{
            //    device = SteamVR_Controller.Input((int)trackedObj.index);
            //}
            //if (device != null)
            //    return device.velocity;

            return Vector3.zero;
        }

        public override Vector3 GetAngularVelocity()
        {
            //if (device == null)
            //{
            //    device = SteamVR_Controller.Input((int)trackedObj.index);
            //}
            //if (device != null)
            //    return device.angularVelocity.normalized * device.angularVelocity.magnitude;

            return Vector3.zero;
        }

        public override void Rumble(ushort microsecs)
        {
            //SteamVR_Controller.Input((int)controller.controllerIndex).TriggerHapticPulse(microsecs);
        }
    }
}
