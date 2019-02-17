using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if USING_HIGHLIGHTING
using HighlightingSystem;
#endif

namespace Vodgets
{
    public class Vodget : MonoBehaviour
    {
        //[System.Serializable]
        public enum NetworkMode { Standalone, Networked };
        public NetworkMode networkMode = NetworkMode.Standalone;

        public enum OwnershipMode { OnFocus, OnGrabbed };


        protected BoolEvent onGrab = new BoolEvent();
        protected BoolEvent onFocus = new BoolEvent();

        protected void RegisterState( BoolEvent evt, UnityAction<bool> call, OwnershipMode mode = OwnershipMode.OnGrabbed)
        {
            if ( networkMode == NetworkMode.Standalone )
            {
                evt.AddListener(call);
            }
 #if USING_PHOTON
           else
            {
                PhotonBool photon = gameObject.GetComponent<PhotonBool>();
                if ( photon == null )
                    photon = gameObject.AddComponent<PhotonBool>();

                photon.AddClient(evt, call);

                if (mode == OwnershipMode.OnGrabbed )
                    onGrab.AddListener(photon.ChangeOwnership);
                else
                    onFocus.AddListener(photon.ChangeOwnership);
            }
#endif
        }

        protected void RegisterState( Vector3Event evt, UnityAction<Vector3> call, OwnershipMode mode = OwnershipMode.OnGrabbed)
        {
            if (networkMode == NetworkMode.Standalone)
            {
                evt.AddListener(call);
            }
#if USING_PHOTON
            else
            {
                PhotonVector3 photon = gameObject.GetComponent<PhotonVector3>();
                if ( photon == null )
                    photon = gameObject.AddComponent<PhotonVector3>();

                photon.AddClient(evt, call);
                
                if (mode == OwnershipMode.OnGrabbed)
                    onGrab.AddListener(photon.ChangeOwnership);
                else
                    onFocus.AddListener(photon.ChangeOwnership);
            }
#endif
        }

        public virtual void DoFocus(Selector cursor, bool state)
        {
            // OVERRIDE THIS VIRTUALLY to allow vodgets to highlight before DoGrab.
            if (state)
            {
#if USING_HIGHLIGHTING
                Highlighter h = transform.gameObject.GetComponent<Highlighter>();
                if (h != null)
                    h.ConstantOn(0.1f);
#endif
                cursor.Rumble(3999);
            }
            else
            {
#if USING_HIGHLIGHTING
                Highlighter h = transform.gameObject.GetComponent<Highlighter>();
                if (h != null)
                    h.ConstantOff(0.1f);
#endif
            }

            if ( onFocus != null )
                onFocus.Invoke(state);
        }

        public virtual void DoFocusUpdate(Selector cursor)
        {
            // OVERRIDE THIS VIRTUALLY 
        }

        public virtual void DoButton(Selector cursor, Selector.Button button, bool state )
        {

        }

        public virtual void DoGrab(Selector cursor, bool state)
        {
            // OVERRIDE THIS VIRTUALLY 
            if ( onGrab != null )
                onGrab.Invoke(state);
        }

        public virtual void DoUpdate(Selector cursor)
        {
            // OVERRIDE THIS VIRTUALLY 
        }
    }
}