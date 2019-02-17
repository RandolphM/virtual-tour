using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    public class DirectSelector : Selector
    {
        static Object cursor_prefab = null;
        GameObject cursor_obj = null;

        // The DirectSelectorTip can be used to offset the tracked controller grab location.
        Transform tip = null;
        public void SetTip( Transform t )
        {
            tip = t;

            if ( cursor_obj != null )
            {
                Destroy(cursor_obj);
                cursor_obj = null;
            }
        }

        protected override void SetCursor()
        {
            if (tip != null)
                cursor.Set(tip.position, transform.rotation, Vector3.one);
            else
                cursor.Set(transform.position, transform.rotation, Vector3.one);
        }

        private void OnEnable()
        {
            if (tip == null)
            {
                if (cursor_prefab == null)
                    cursor_prefab = Resources.Load("cursor_green_jack");
                if (cursor_prefab != null)
                {
                    cursor_obj = (GameObject)GameObject.Instantiate(cursor_prefab);
                    cursor_obj.transform.SetParent(transform, false);
                    cursor_obj.transform.localPosition = Vector3.zero;
                    cursor_obj.transform.localRotation = Quaternion.identity;
                }
            }

        }

        private void OnDisable()
        {
            //ReleaseController();

            if (cursor_obj != null)
                Destroy(cursor_obj);
        }

        public float debouncingDelay = 0.25f;
        private List<GameObject> debounce_objs = new List<GameObject>();

        // Defer focus removal for a period of time. 
        protected IEnumerator DebounceOnExit(GameObject debounce_obj)
        {
            debounce_objs.Add(debounce_obj);

            // Defer decision to exit for a slight delay.
            yield return new WaitForSeconds(debouncingDelay);

            // If the debounce_obj is still in the list then no re-entry was detected and we exicute the normal exit.
            if (debounce_objs.Remove(debounce_obj))
                RemoveFocus(debounce_obj);
        }

        // The trigger and rigidbody can either be on the tracked controller or a child of
        // tracked controller that has the DirectSelectorTip component. 
        // Trigger enter and exit are public methods to allow DirectSelectorTip to provide 
        // collision notifications to the selector.
        public virtual void OnTriggerEnter(Collider other)
        {
            GameObject debounce_obj = GetVodgetObj(other.gameObject);
            if ( debounce_obj != null )
            {
                debounce_objs.Remove(debounce_obj);
                AddFocus(other.gameObject);
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            GameObject debounce_obj = GetVodgetObj(other.gameObject);
            if (debounce_obj != null)
            {
                // Start a new debounce delay.
                StartCoroutine(DebounceOnExit(debounce_obj));
            }
        }

        private void FixedUpdate()
        {
            if (isGrabbing)
                DoGrabbedUpdate();
            else
                DoFocusUpdate();
        }
    }
}
