using UnityEngine;

namespace Vodgets
{

    public class SelectorMgr : MonoBehaviour
    {
        public enum SelectorType { Direct, HandRay, EyeHandRay, HeadRay, None };
        public enum DominantEyeType { Lefteye, Righteye };

        public SelectorType selector_type = SelectorType.Direct;
        public DominantEyeType dominant_eye = DominantEyeType.Righteye;

        public SteamVR_TrackedController left_controller = null;
        public SteamVR_TrackedController right_controller = null;
        public SteamVR_TrackedObject head = null;

        private SelectorType curr_selector_type = SelectorType.None;
        private DominantEyeType curr_dominant_eye = DominantEyeType.Righteye;

        // Update is called once per frame
        void Update()
        {
            if (selector_type != curr_selector_type)
            {
                {
                    Selector selector = left_controller.GetComponent<Selector>();
                    if (selector)
                        Destroy(selector);
                }
                {
                    Selector selector = right_controller.GetComponent<Selector>();
                    if (selector)
                        Destroy(selector);
                }
                {
                    Selector selector = head.GetComponent<Selector>();
                    if (selector)
                        Destroy(selector);
                }

                switch (selector_type)
                {
                    case SelectorType.Direct:
                        left_controller.gameObject.AddComponent<DirectSelector>();
                        right_controller.gameObject.AddComponent<DirectSelector>();
                        break;

                    case SelectorType.HandRay:
                        left_controller.gameObject.AddComponent<HandRaySelector>();
                        right_controller.gameObject.AddComponent<HandRaySelector>();
                        break;
                    case SelectorType.EyeHandRay:
                        EyeHandRaySelector lselector = left_controller.gameObject.AddComponent<EyeHandRaySelector>();
                        EyeHandRaySelector rselector = right_controller.gameObject.AddComponent<EyeHandRaySelector>();
                        lselector.isRightEyeDominant = (dominant_eye == DominantEyeType.Righteye);
                        rselector.isRightEyeDominant = (dominant_eye == DominantEyeType.Righteye);
                        break;
                    case SelectorType.HeadRay:
                        head.gameObject.AddComponent<HeadRaySelector>();
                        break;
                }

                curr_selector_type = selector_type;
            }

            if (curr_dominant_eye != dominant_eye && curr_selector_type == SelectorType.EyeHandRay)
            {
                EyeHandRaySelector lselector = left_controller.gameObject.GetComponent<EyeHandRaySelector>();
                EyeHandRaySelector rselector = right_controller.gameObject.GetComponent<EyeHandRaySelector>();
                if (lselector != null)
                    lselector.isRightEyeDominant = (dominant_eye == DominantEyeType.Righteye);
                if (rselector != null)
                    rselector.isRightEyeDominant = (dominant_eye == DominantEyeType.Righteye);

                curr_dominant_eye = dominant_eye;
            }

        }
    }
}

