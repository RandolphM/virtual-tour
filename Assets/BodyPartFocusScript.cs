/// <summary> Written by Benjamin Milian
/// Synopsis: this script handles the on hover "more information" GambeObject
/// when hovering over a body part that contains a collider with a specific
/// tag. By clicking the GameObject when in focus the GameObject is then
/// activated to be viewed.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vodgets
{
    public class BodyPartFocusScript : Vodget
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private GameObject text;
        [SerializeField] private GameObject moreInfoBox;

        // make sure the info isn't visible
        // and stop the particles at start
        void Start()
        {
            text.gameObject.SetActive(false);
            particles.Stop();
        }

        // check the state | set active on focus of body part | deactivate if no hovering over
        public override void DoFocus(Selector cursor, bool state)
        {
            base.DoFocus(cursor, state);

            if (state)
            {
                text.gameObject.SetActive(true);
                particles.Play();
                Debug.Log("Body Part Found");
            }
            else
            {
                text.gameObject.SetActive(false);
                particles.Stop();
            }
        }

        // check to see if there is something to grab | check the tag | set active when button pressed
        public override void DoGrab(Selector cursor, bool state)
        {
            base.DoGrab(cursor, state);

            if (state && cursor.ColliderObj.tag == "ChildHead")
            {
                moreInfoBox.SetActive(true);
            }
            else
            {
                moreInfoBox.SetActive(false);
            }

        }
    }
}