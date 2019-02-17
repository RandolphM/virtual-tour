using UnityEngine;
using UnityEngine.Events;

namespace Vodgets
{

    public class Button : Vodget
    {
        [System.Serializable]
        public class ButtonEvent : UnityEvent<bool> { }

        public ButtonEvent button_changed;

        public override void DoGrab(Selector selector, bool state)
        {
            button_changed.Invoke(state);
        }
    }
}