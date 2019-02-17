// Gaze Input Module by Peter Koch <peterept@gmail.com>
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Vodgets
{


    // To use:
    // 1. Drag onto your EventSystem game object.
    // 2. Disable any other Input Modules (eg: StandaloneInputModule & TouchInputModule) as they will fight over selections.
    // 3. Make sure your Canvas is in world space and has a GraphicRaycaster (should by default).
    // 4. If you have multiple cameras then make sure to drag your VR (center eye) camera into the canvas.
    public class VodgetsInputModule : PointerInputModule
    {
        public float nearDistance = 0.05f;
        public float farDistance = 1000f;

        private PointerEventData pointerEvent;

#if USING_HANDRAY
        Camera fallbackCam = null;
        public Camera eventCamera
        {
            get
            {
                if (fallbackCam == null)
                {
                    var go = new GameObject(name + " FallbackCamera");
                    go.SetActive(false);
                    // place fallback camera at root to preserve world position
                    //go.transform.SetParent(transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    fallbackCam = go.AddComponent<Camera>();
                    fallbackCam.clearFlags = CameraClearFlags.Nothing;
                    fallbackCam.cullingMask = 0;
                    fallbackCam.orthographic = true;
                    fallbackCam.orthographicSize = 1;
                    fallbackCam.useOcclusionCulling = false;
#if !(UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0)
                    fallbackCam.stereoTargetEye = StereoTargetEyeMask.None;
#endif
                    fallbackCam.nearClipPlane = nearDistance;
                    fallbackCam.farClipPlane = farDistance;
                }

                return fallbackCam;
            }
        }
#endif

        //bool pressDown = false;

        //bool pressDownCurr = false;
        //bool hasFocus = false;

        bool firstFocus = false;
        public void DoFocus(bool state)
        {
            if ( state )
                firstFocus = true;

            //if (state)
            //{

            //}
            //else
            //{
            //    if (currentHandler != null)
            //    {
            //        DoRelease();
            //        //ExecuteEvents.ExecuteHierarchy(currentLookAtHandler, pointerEvent, ExecuteEvents.deselectHandler);

            //        currentHandler = null;

            //        //Debug.Log("Handler:" + ((currentLookAtHandler != null ) ? currentLookAtHandler.name : "null") );
            //    }
            //}

            //hasFocus = state;
        }

        // Required by inherited input modules.
        public override void Process()
        {

        }

        public void UpdateFocus(Vector2 cursor_pos)
        {
            if (pointerEvent == null)
                pointerEvent = new PointerEventData(eventSystem);

            //if (firstFocus)
            //{
            //    pointerEvent.delta = Vector2.zero;
            //    firstFocus = false;
            //}
            //else
            //{
            //    pointerEvent.delta = cursor_pos - pointerEvent.position;
            //}
            //pointerEvent.delta = Vector2.zero;

            pointerEvent.position = cursor_pos;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEvent, raycastResults);
            pointerEvent.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

            Debug.Log(pointerEvent.delta);

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {


                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != null && pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
                ProcessDrag(pointerEvent);

            } else
            {
                ProcessMove(pointerEvent);

            }
        }

#if BLAH
        void HandleSelection()
        {
            if (pointerEvent.pointerEnter != null)
            {
                // if the ui receiver has changed, reset the gaze delay timer
                GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEvent.pointerEnter);
                if (currentHandler != handler)
                {
                    //DoRelease();
                    currentHandler = handler;
                }


                if ( currentHandler != null )
                {
                    if (pressDownCurr != pressDown)
                    {
                        pressDownCurr = pressDown;
                        if (pressDownCurr)
                        {
                            ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.pointerDownHandler);

                            //ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.initializePotentialDrag);
                            //ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.beginDragHandler);

                        }
                        else
                        {
                            ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.pointerUpHandler);
                            ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.pointerClickHandler);
                            //ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.endDragHandler);

                        }
                    }

                    //ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.moveHandler);
                    //if (pressDownCurr)
                    //{
                    //    //pointerEvent.delta = new Vector2(0f, 5f);
                    //    ExecuteEvents.ExecuteHierarchy(currentHandler, pointerEvent, ExecuteEvents.dragHandler);
                    //}

                }
            }
            else
            {
                currentHandler = null;
            }


        }
#endif

        public void DoPress()
        {
            //pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            //pointerEvent.dragging = false;
            //pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            //pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            pointerEvent.pointerPress = ExecuteEvents.GetEventHandler<IPointerDownHandler>(pointerEvent.pointerEnter);
            if (pointerEvent.pointerPress != null )
            {
                //pointerEvent.pressPosition = pointerEvent.position;
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerDownHandler);
            }

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(pointerEvent.pointerEnter);
            if (pointerEvent.pointerDrag != null)
            {
                //ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
                //ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }
        }

        public void DoRelease()
        {
            GameObject handler = ExecuteEvents.GetEventHandler<IPointerUpHandler>(pointerEvent.pointerEnter);
            if (handler != null)
            {
                ExecuteEvents.ExecuteHierarchy(handler, pointerEvent, ExecuteEvents.pointerUpHandler);
            }

            handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEvent.pointerEnter);
            if (handler != null)
            {
                ExecuteEvents.ExecuteHierarchy(handler, pointerEvent, ExecuteEvents.pointerClickHandler);
            }

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                //ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;
            }

            pointerEvent.pointerPress = null;
        }
    }
}