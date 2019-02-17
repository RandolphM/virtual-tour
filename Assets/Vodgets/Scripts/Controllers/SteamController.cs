using UnityEngine;

namespace Vodgets
{
    class SteamVRButtonSource
    {
        Controller controller;
        SteamVR_TrackedController steam_controller;
        Selector.ButtonHandler handler;
        public SteamVRButtonSource(Controller c, SteamVR_TrackedController sc, Selector.ButtonHandler b)
        {
            controller = c;
            steam_controller = sc;
            handler = b;
            switch (handler.button)
            {
                case Selector.Button.Trigger:
                    steam_controller.TriggerClicked += OnButtonDown;
                    steam_controller.TriggerUnclicked += OnButtonUp;
                    break;
                case Selector.Button.Menu:
                    steam_controller.MenuButtonClicked += OnButtonDown;
                    steam_controller.MenuButtonUnclicked += OnButtonUp;
                    break;
                case Selector.Button.PadClicked:
                    steam_controller.PadClicked += OnButtonDown;
                    steam_controller.PadUnclicked += OnButtonUp;
                    break;
                case Selector.Button.PadTouched:
                    steam_controller.PadTouched += OnButtonDown;
                    steam_controller.PadUntouched += OnButtonUp;
                    break;
                case Selector.Button.Gripped:
                    steam_controller.Gripped += OnButtonDown;
                    steam_controller.Ungripped += OnButtonUp;
                    break;
            }
        }

        ~SteamVRButtonSource()
        {
            switch (handler.button)
            {
                case Selector.Button.Trigger:
                    steam_controller.TriggerClicked -= OnButtonDown;
                    steam_controller.TriggerUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.Menu:
                    steam_controller.MenuButtonClicked -= OnButtonDown;
                    steam_controller.MenuButtonUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.PadClicked:
                    steam_controller.PadClicked -= OnButtonDown;
                    steam_controller.PadUnclicked -= OnButtonUp;
                    break;
                case Selector.Button.PadTouched:
                    steam_controller.PadTouched -= OnButtonDown;
                    steam_controller.PadUntouched -= OnButtonUp;
                    break;
                case Selector.Button.Gripped:
                    steam_controller.Gripped -= OnButtonDown;
                    steam_controller.Ungripped -= OnButtonUp;
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

    [RequireComponent(typeof(SteamVR_TrackedController), typeof(SteamVR_TrackedObject), typeof(Selector))]
    public class SteamController : Controller
    {
        // SteamVR default implementation
        // The Controller property is added to allow vodgets to access other controller state and events.
        // Note: Vodgets should be aware that some selectors like the head ray may return null.
        SteamVR_TrackedController controller = null;
        SteamVR_TrackedObject trackedObj = null;
        SteamVR_Controller.Device device = null;

        SteamVRButtonSource trigger_src = null;
        SteamVRButtonSource menu_src = null;
        SteamVRButtonSource padClicked_src = null;
        SteamVRButtonSource padTouched_src = null;
        SteamVRButtonSource gripped_src = null;

        private void Awake()
        {
            controller = GetComponent<SteamVR_TrackedController>();
            trackedObj = GetComponent<SteamVR_TrackedObject>();
            Selector s = GetComponent<Selector>();

            // Wire up events
            trigger_src = new SteamVRButtonSource(this, controller, s.GetHandler(Selector.Button.Trigger));
            menu_src = new SteamVRButtonSource(this, controller, s.GetHandler(Selector.Button.Menu));
            padClicked_src = new SteamVRButtonSource(this, controller, s.GetHandler(Selector.Button.PadClicked));
            padTouched_src = new SteamVRButtonSource(this, controller, s.GetHandler(Selector.Button.PadTouched));
            gripped_src = new SteamVRButtonSource(this, controller, s.GetHandler(Selector.Button.Gripped));
        }

        private void OnDestroy()
        {
            trigger_src = null;
            menu_src = null;
            padClicked_src = null;
            padTouched_src = null;
            gripped_src = null;
            controller = null;
        }

        public override Vector3 GetVelocity()
        {
            if (device == null)
            {
                device = SteamVR_Controller.Input((int)trackedObj.index);
            }
            if (device != null)
                return device.velocity;

            return Vector3.zero;
        }

        public override Vector3 GetAngularVelocity()
        {
            if (device == null)
            {
                device = SteamVR_Controller.Input((int)trackedObj.index);
            }
            if (device != null)
                return device.angularVelocity.normalized * device.angularVelocity.magnitude;

            return Vector3.zero;
        }

        public override void Rumble(ushort microsecs)
        {
            SteamVR_Controller.Input((int)controller.controllerIndex).TriggerHapticPulse(microsecs);
        }
    }
}
