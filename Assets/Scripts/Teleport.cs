using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    public class Teleport : Vodget
    {

        //input//
        public Transform VRperson_pos = null;
        public Transform eye_position = null;
        public GameObject arrow_indicator;
        public GameObject Skin;

        //variable
        GameObject other_arrow;
        Selector curr_selector = null;
        Selector oth_selector = null;

        // Use this for initialization
        void Start()
        {
            other_arrow = Instantiate(arrow_indicator, Vector3.zero, Quaternion.identity);
            arrow_indicator.SetActive(false);
            Skin.SetActive(false);
        }

        

        public override void DoFocus(Selector cursor, bool state)
        {
           
            base.DoFocus(cursor, state);


            //To show the location of where the person/group is going to move//
            if (state)
            {
                //show if ether/both of the cursor is on the grid//
                if (curr_selector != null/* && curr_selector != cursor*/)
                {
                    oth_selector = cursor;
                    other_arrow.SetActive(true);
                    return;
                }
                           
                //activate the grid to show where is allowed to move//
                curr_selector = cursor;
                Skin.SetActive(true);
                arrow_indicator.SetActive(true);
                
            }
            else
            {
                //Check if the other cursor is active or not active on the grid//
                if (oth_selector == null )
                {
                    //deactivate the grid and other objects//
                    Skin.SetActive(false);
                    arrow_indicator.SetActive(false);
                    curr_selector = null;
                }
                else
                {
                    //Gave ownership to other cursor 
                    other_arrow.SetActive(false);
                    curr_selector = oth_selector;
                    oth_selector = null;
                }

            }

        }
        public override void DoFocusUpdate(Selector cursor)
        {
            //update the arrow position so the arrow indicator can have//
            //the same position as the cursor.//
            arrow_indicator.transform.position = curr_selector.Cursor.localPosition;
            arrow_indicator.transform.Rotate(0, 2.5f, 0);
            if (oth_selector != null)
            {
                other_arrow.transform.position = oth_selector.Cursor.localPosition;
                other_arrow.transform.Rotate(0, 2.5f, 0);
            }

        }

        public override void DoGrab(Selector cursor, bool state)
        {
            base.DoGrab(cursor, state);

            if (state)
            {
                //Fades the screen black//
                SteamVR_Fade.Start(Color.clear, 0f);
                SteamVR_Fade.Start(Color.black, 2.5f);
                //Moves player to new position//
                Vector3 new_position = cursor.Cursor.localPosition - eye_position.localPosition;
                new_position.y = 0.0f;
                VRperson_pos.position = new_position;
                //Fades to show the player their new position//
                SteamVR_Fade.Start(Color.black, 0f);
                SteamVR_Fade.Start(Color.clear, 2.5f);
            }
        }
    }
}
