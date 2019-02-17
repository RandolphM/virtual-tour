using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{ 
    /// <summary>
    /// Synopsis: This class handles sounds that are networked or
    /// standalone. to make a new sound. 
    /// 1. Create sound object 
    /// 2. Create a bool event
    /// 3. Create a function to handle the event
    /// 4. Register the event at Start()
    /// 5. Override the function in Vodgets that you want to 
    ///    access and invoke the function you created
    /// </summary>
    public class sound_interaction : Vodget
    {
        [SerializeField] private AudioSource sound;
        [SerializeField] private AudioSource click;

        bool touch_active = false;

        public BoolEvent onPlaySound = new BoolEvent();
        public BoolEvent onPressSound = new BoolEvent();

        public void PlaySound( bool state )
        {
            if (state && sound != null)
            {
                sound.Play();
            }
        }

        public void PressSound(bool state)
        {
            if (state && click != null)
                click.Play();
        }

        void Start()
        {
            RegisterState(onPlaySound, PlaySound);
            RegisterState(onPressSound, PressSound);
        }
        
        // VODGET FUNCTION
        public override void DoFocus(Selector cursor, bool state)
        {
            base.DoFocus(cursor, state);
            onPlaySound.Invoke(state); // FUNCTION WILL ONLY BE INVOKED IF THE STATE IS TRUE.
        }

        // VODGET FUNCTION
        public override void DoGrab(Selector cursor, bool state)
        {
            base.DoGrab(cursor, state);
            onPressSound.Invoke(state);
        }


    }
}
