using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    // VodgetModes are components created and destroyed by vodgets.
    // The iterface is identical to Vodget but is not selectable as a Vodget.
    // This is useful to allow selectable Vodgets to provide support for different modes of operation by
    // adding and destroying VodgetMode components when modes of operation change. 
    public class VodgetMode : MonoBehaviour
    {

        public virtual void DoFocus(Selector cursor, bool state)
        {
            // OVERRIDE THIS VIRTUALLY to allow vodgets to highlight before DoGrab.
        }

        public virtual void DoGrab(Selector cursor, bool state)
        {
            // OVERRIDE THIS VIRTUALLY 
        }

        public virtual void DoUpdate(Selector cursor)
        {
            // OVERRIDE THIS VIRTUALLY 
        }
    }
}