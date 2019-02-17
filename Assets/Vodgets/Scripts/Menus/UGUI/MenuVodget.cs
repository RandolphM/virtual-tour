using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Vodgets
{
    [RequireComponent(typeof(Canvas))]
    public class MenuVodget : Vodget {

        VodgetsInputModule input_module;
        Canvas canvas;

        private void Start()
        {
            canvas = GetComponent<Canvas>();

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>();
            }
            if (eventSystem == null)
            {
                eventSystem = new GameObject("[EventSystem]").AddComponent<EventSystem>();
            }
            if (eventSystem == null)
            {
                Debug.LogWarning("EventSystem not found or create fail!");
                return;
            }

            input_module = eventSystem.gameObject.GetComponent<VodgetsInputModule>();
            if (input_module == null)
            {
                input_module = eventSystem.gameObject.AddComponent<VodgetsInputModule>();
                input_module.ActivateModule();
            }

        }

        public override void DoFocus(Selector cursor, bool state)
        {
            base.DoFocus(cursor, state);

            if (input_module != null)
                input_module.DoFocus(state);
        }

        public override void DoFocusUpdate(Selector cursor)
        {
            base.DoFocusUpdate(cursor);
            if (input_module != null)
                input_module.UpdateFocus(canvas.worldCamera.WorldToScreenPoint(cursor.Cursor.localPosition));
        }

        public override void DoButton(Selector cursor, Selector.Button button, bool state)
        {
            base.DoButton(cursor, button, state);

            if (button == Selector.Button.Gripped)
            {
                if (input_module != null)
                {
                    input_module.UpdateFocus(canvas.worldCamera.WorldToScreenPoint(cursor.Cursor.localPosition));
                    if (state)
                        input_module.DoPress();
                    else
                        input_module.DoRelease();
                }
            }
        }

        public override void DoGrab(Selector cursor, bool state)
        {
            base.DoGrab(cursor, state);
            //if (input_module != null)
            //{
            //    input_module.UpdateFocus(cursor.transform.position, cursor.transform.rotation);
            //    if (state)
            //        input_module.DoPress();
            //    else
            //        input_module.DoRelease();
            //}
        }

        //public override void DoUpdate(Selector cursor)
        //{
        //    base.DoUpdate(cursor);
        //    if (input_module != null)
        //        input_module.UpdateFocus(cursor.transform.position, cursor.transform.rotation);
        //}
    }

}
