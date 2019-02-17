using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    public delegate void ButtonEventHandler( bool state );

    public abstract class Controller : MonoBehaviour {
        public abstract Vector3 GetVelocity();
        public abstract Vector3 GetAngularVelocity();
        public abstract void Rumble(ushort microsecs);

        //public enum Button { Trigger, Menu, PadClicked, PadTouched, Gripped }
        public void SetButton( Selector.Button button, bool state )
        {
            switch (button)
            {
                case Selector.Button.Trigger:
                    if (TriggerClicked != null)
                        TriggerClicked(state);
                    break;
                case Selector.Button.Menu:
                    if (MenuClicked != null)
                        MenuClicked(state);
                    break;
                case Selector.Button.PadClicked:
                    if (PadClicked != null)
                        PadClicked(state);
                    break;
                case Selector.Button.PadTouched:
                    if (PadTouched != null)
                        PadTouched(state);
                    break;
                case Selector.Button.Gripped:
                    if (Gripped != null)
                        Gripped(state);
                    break;
            }

        }

        public event ButtonEventHandler TriggerClicked;
        public event ButtonEventHandler MenuClicked;
        public event ButtonEventHandler PadClicked;
        public event ButtonEventHandler PadTouched;
        public event ButtonEventHandler Gripped;
    }
}
