/// <summary> Written by Benjamin Milian
/// Synopsis: Class handles the callbacks for the buttons
/// being used on the presentation canvas. This is also networked.
/// 
///  Comment should guide you on whats happening and how to create
///  your own features using *this* class.
///  
/// ***** REFER TO THE SOUND_INTERACTION SCRIPT SYNOPSIS IF *****
/// ***** YOU DONT UNDERSTAND WHAT IS GOING ON HERE OR ASK  *****
/// ***** DAN TO EXPLAIN THE VODGETS ARCHITECTURE TO YOU.   *****
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vodgets
{
    public class ButtonBehavior : Vodget
    {
        // 1.create the event 
        public BoolEvent onButtonPress = new BoolEvent();
        public BoolEvent onButtonFocus = new BoolEvent();


        private void Start()
        {
            // 2.register the event with the functions 
            //it is going to callback
            RegisterState(onButtonPress, Press);
            RegisterState(onButtonFocus, HighLight);
        }

        // 3.create the callback function
        public void Press(bool state)
        {
            if (state)
            {
                Debug.Log("Press was Inkvoked");
                // gets the button's onclick() (function) and invokes it (presses the button)
                this.gameObject.GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();
            }
        }

        // when DoFocus callback is called this changes the color of the text
        public void HighLight(bool state)
        {
            if (state)
            {
                Debug.Log("Focusing On: " + this.gameObject.GetComponentInChildren<Selectable>().name);
                this.gameObject.GetComponentInChildren<Text>().color = Color.red;
                //if (interfaceHover != null)
                //    interfaceHover.Play();
            }
            else
                this.gameObject.GetComponentInChildren<Text>().color = Color.white;

        }

        public override void DoGrab(Selector cursor, bool state)
        {
            base.DoGrab(cursor, state);
            onButtonPress.Invoke(state);
        }

        public override void DoFocus(Selector cursor, bool state)
        {
            Debug.Log("DoFocus Callback Called");
            base.DoFocus(cursor, state);
            onButtonFocus.Invoke(state);
        }
    }
}