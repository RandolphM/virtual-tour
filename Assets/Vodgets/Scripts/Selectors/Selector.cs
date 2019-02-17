using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    //[RequireComponent(typeof(Controller))]
    public class Selector : MonoBehaviour {

        public enum Button { Trigger, Menu, PadClicked, PadTouched, Gripped }

        // The Cursor property provides vodgets with the emulated controller world position and rotation.
        protected Srt cursor = new Srt();
        public Srt Cursor
        {
            get { return cursor; }
        }

        protected virtual void SetCursor()
        {
            // Developer should override in all inherited classes.
        }

        public void CopyFocusObjs(List<GameObject> objs)
        {
            // Save focus objects to grabbed objects persistent during grab.
            foreach (GameObject f in focus_objs)
                if (f != null)
                    objs.Add(f);
        }

        public class ButtonHandler
        {
            public Button button;
            protected Selector selector = null;
            protected List<GameObject> objs = new List<GameObject>();

            //public delegate void ButtonEvt();
            //public event ButtonEvt ButtonPressed;

            public ButtonHandler(Selector s, Button b)
            {
                selector = s;
                button = b;
            }

            public virtual void DoButtonDown()
            {
                if (ButtonClicked != null)
                    ButtonClicked(true);

                // Save focus objects to grabbed objects persistent during grab.
                selector.CopyFocusObjs(objs);
                selector.SetCursor();

                // Notify grab on all grabbed objects.
                foreach (GameObject f in objs)
                {
                    Vodget[] vlist = f.GetComponents<Vodget>();
                    foreach (Vodget v in vlist)
                        v.DoButton(selector, button, true);
                }
            }

            public virtual void DoButtonUp()
            {
                if (ButtonClicked != null)
                    ButtonClicked(false);

                selector.SetCursor();

                // Notify grab release on all grabbed objects
                foreach (GameObject f in objs)
                {
                    if (f != null)
                    {
                        Vodget[] vlist = f.GetComponents<Vodget>();
                        foreach (Vodget v in vlist)
                            v.DoButton(selector, button, false);
                    }
                }

                // Clear all grabbing state
                ClearObjs();
            }

            protected virtual void ClearObjs()
            {
                objs.Clear();
            }

            public event ButtonEventHandler ButtonClicked;

        }

        // All Selectors with Controllers use the trigger button to control grabbing.
        // Note: Selectors without controllers must implement the DoGrab methods directly.
        public class GrabButtonHandler : ButtonHandler
        {
            public GrabButtonHandler(Selector s, Button b) : base(s, b)
            {
            }

            public override void DoButtonDown()
            {
                base.DoButtonDown();

                if (objs.Count > 0)
                {
                    selector.isGrabbing = true;
                    foreach (GameObject f in objs)
                    {
                        Vodget[] vlist = f.GetComponents<Vodget>();
                        foreach (Vodget v in vlist)
                            v.DoGrab(selector, true);
                    }
                }
            }

            protected override void ClearObjs()
            {
                if (selector.isGrabbing)
                {
                    selector.isGrabbing = false;
                    if (objs.Count > 0)
                    {
                        foreach (GameObject f in objs)
                        {
                            Vodget[] vlist = f.GetComponents<Vodget>();
                            foreach (Vodget v in vlist)
                                v.DoGrab(selector, false);
                        }
                    }

                    // Notify de-focus on grabbed objects no longer in focus objects.
                    foreach (GameObject f in objs)
                    {
                        if (f != null)
                        {
                            if (!selector.focus_objs.Contains(f))
                            {
                                Vodget[] vlist = f.GetComponents<Vodget>();
                                foreach (Vodget v in vlist)
                                    v.DoFocus(selector, false);
                            }
                        }
                    }

                    // Notify focus on focus objects not found in grabbed objects
                    foreach (GameObject f in selector.focus_objs)
                    {
                        if (f != null)
                        {
                            if (!objs.Contains(f))
                            {
                                Vodget[] vlist = f.GetComponents<Vodget>();
                                foreach (Vodget v in vlist)
                                    v.DoFocus(selector, true);
                            }
                        }
                    }
                }
                base.ClearObjs();
            }

            public void DoGrabbedUpdate()
            {
                if (objs.Count > 0)
                {
                    selector.SetCursor();

                    foreach (GameObject f in objs)
                    {
                        // Object can be deleted while grabbing.
                        if (f != null)
                        {
                            Vodget[] vlist = f.GetComponents<Vodget>();
                            foreach (Vodget v in vlist)
                                v.DoUpdate(selector);
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            controller = GetComponent<Controller>();
        }

        private void OnDestroy()
        {
            trigger_handler = null;
            menu_handler = null;
            padClicked_handler = null;
            padTouched_handler = null;
            gripped_handler = null;
            controller = null;
        }

        Controller controller = null;

        // The FocusObj is the current focus object with vodgets.
        List<GameObject> focus_objs = new List<GameObject>();
        protected bool isGrabbing = false;

        // The ColliderObj is the object that either direct or ray collision occured on for vodget selection.
        GameObject collider_obj = null;
        public GameObject ColliderObj
        {
            get { return collider_obj; }
        }

        GrabButtonHandler trigger_handler = null;
        ButtonHandler menu_handler = null;
        ButtonHandler padClicked_handler = null;
        ButtonHandler padTouched_handler = null;
        ButtonHandler gripped_handler = null;

        public ButtonHandler GetHandler( Button b )
        {
            // Note: Create handlers on demand to avoid constructor chase condition 
            // that might occur when a controller initializes before a selector.
            switch (b)
            {
                case Button.Trigger:
                    if ( trigger_handler == null )
                        trigger_handler = new GrabButtonHandler(this, Button.Trigger);
                    return trigger_handler;
                case Button.Menu:
                    if (menu_handler == null)
                        menu_handler = new ButtonHandler(this, Button.Menu);
                    return menu_handler;
                case Button.PadClicked:
                    if (padClicked_handler == null)
                        padClicked_handler = new ButtonHandler(this, Button.PadClicked);
                     return padClicked_handler;
                case Button.PadTouched:
                    if (padTouched_handler == null)
                        padTouched_handler = new ButtonHandler(this, Button.PadTouched);
                    return padTouched_handler;
                case Button.Gripped:
                    if (gripped_handler == null)
                        gripped_handler = new ButtonHandler(this, Button.Gripped);
                    return gripped_handler;
                default:
                    return null;
            }
        }

        public Vector3 GetVelocity()
        {
            return transform.parent.TransformVector(controller.GetVelocity());
        }

        public Vector3 GetAngularVelocity()
        {
            return transform.parent.TransformVector(controller.GetAngularVelocity());
        }

        public void Rumble( ushort microsecs )
        {
            controller.Rumble(microsecs);
        }

        protected void DoFocusUpdate()
        {
            if (focus_objs.Count > 0)
            {
                SetCursor();

                foreach (GameObject f in focus_objs)
                {
                    // Object can be deleted while grabbing.
                    if (f != null)
                    {
                        Vodget[] vlist = f.GetComponents<Vodget>();
                        foreach (Vodget v in vlist)
                            v.DoFocusUpdate(this);
                    }
                }
            }
        }

        // All grabbing Selectors update their emulated cursor before notifying the vodgets of their new world location.
        protected void DoGrabbedUpdate()
        {
            trigger_handler.DoGrabbedUpdate();
        }

        protected GameObject GetVodgetObj( GameObject obj )
        {
            if ( obj == null )
            {
                //Debug.Log("GetVodgetObj: NULL");
                return null;
            }
            Transform t = obj.transform;
            while (t != null )
            {
                Vodget[] vlist = t.gameObject.GetComponents<Vodget>();
                if (vlist.Length > 0)
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }

        // Used by DirectSelector instead of DetectVodget
        // Direct selectors can focus on multiple gameobjs and must add/remove them separately.
        protected void AddFocus(GameObject c_obj)
        {
            GameObject found_focus = GetVodgetObj(c_obj);

            if (found_focus == null)
                return;

            if ( !focus_objs.Contains( found_focus ) )
            {
                focus_objs.Add(found_focus);

                if (!isGrabbing)
                {
                    Vodget[] vlist = found_focus.GetComponents<Vodget>();
                    foreach (Vodget v in vlist)
                        v.DoFocus(this, true);
                }
            }
        }

        // Used by DirectSelector instead of DetectVodget
        // Direct selectors can focus on multiple gameobjs and must add/remove them separately.
        protected void RemoveFocus(GameObject c_obj)
        {
            GameObject found_focus = GetVodgetObj(c_obj);

            if (found_focus == null)
                return;

            if (focus_objs.Remove(found_focus))
            {
                if (!isGrabbing)
                {
                    Vodget[] vlist = found_focus.GetComponents<Vodget>();
                    foreach (Vodget v in vlist)
                        v.DoFocus(this, false);
                }
            }
        }

        // Ray selectors call DetectVodget while attempting to find new vodgets.
        // Ray selectors only focus on a single gameobj at a time.
        protected void DetectVodget(GameObject obj)
        {
            GameObject found_focus = GetVodgetObj(obj);

            // The search for vodgets from any collider object always returns the same gameobject.
            if (found_focus == null || ! focus_objs.Contains(found_focus) )
            {
                // Send defocus immediately when not grabbing. 
                if (!isGrabbing)
                {
                    SetCursor();

                    foreach (GameObject f in focus_objs)
                    {
                        if (f != null)
                        {
                            Vodget[] vlist = f.GetComponents<Vodget>();
                            foreach (Vodget v in vlist)
                                v.DoFocus(this, false);
                        }
                    }
                }
                focus_objs.Clear();

                // Search up the tree to find current vodgets.
                collider_obj = obj; // This should be deprecated in favor of curr_vodgets.

                if (found_focus != null)
                {
                    focus_objs.Add(found_focus);

                    if ( ! isGrabbing )
                    {
                        // When the collider changes while not grabbing update the cursor for any events.
                        SetCursor();

                        Vodget[] vlist = found_focus.GetComponents<Vodget>();
                        foreach (Vodget v in vlist)
                            v.DoFocus(this, true);
                    }
                }

            }
        }
    }

}