/// <summary> Written by Benjamin Milian
/// Synopsis: Class handles playback options using the 
/// AnotationManager to use in the animation event manager.
/// *** ANIMATION EVENT MANAGER ONLY ACCEPTS STRINGS AND INTS AS PARAMETERS ***
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    /// <summary>
    /// ***** HOW TO USE THE ANIMATION EVENT CLASS AND KEY EVENTS IN THE ANIMATION WINDOW *****
    /// 1. CREATE A FUNCTION IN THIS CLASS
    /// 2. DECLARE ANY VARIABLES WITH INHERITANCE OR ACCESS THAT YOU MIGHT NEED
    ///    IN THIS CASE I AM USING THE ANNOTATIONMANAGER TO ACCES PUBLIC DATA
    ///    BE SURE TO CREATE MUTATORS AND ACCESSORS WITHIN ANY CLASS TAHT YOU WANT
    ///    TO ACCESS PRIVATE DATA. PREFFERABLY ACCESSORS.
    /// 3. CLICK ON THE CECO TOUR MODELS (THEY CONTAIN ANIMATIONS)
    /// 4. ONCE YOU CLICK THE MODEL(S) THE ANIMATION WINDOW WILL REFRESH AND YOU WILL
    ///    SEE KEY FRAMES.
    /// 5. ON THE RIGHT SIDE OF THE ANIMATION WINDOWS AT THE TOP THERE WILL BE A STRIP WITH TIME BUFFER
    ///    CLICK ANYWHERE WITHIN THERE TO SHOW YOU THE MODEL AT THAT SPECIFIC TIME.
    ///    AS YOU WILL SEE I HAVE LEFT KEY EVENTS AT SPECIFIC TIMES. USE THAT AS A BASE TO WHAT YOU CAN DO
    /// 6. CREATING AN EVENT: SCROLL UP OR DOWN WHILE IN THE ANIMATION WINDOW TO SPREAD THE BUFFER FURTHER 
    ///    APART AND TO ACCESS MORE PRECISE POINTS.
    ///    CLICK THE TIME BUFFER AT THE SPECIFIC POINT IN TIME THAT YOU WOULD LIKE TO ADD A FUNCTION EVENT.
    ///    NOW ON THE LEFT SIDE OF THE ANIMATION WINDOW YOU WILL SEE BUTTONS THERE WILL BE ONE TO THE MOST
    ///    RIGHT WITH A PLUS BUTTON (WHEN HOVERING OVER IT AN "ADD EVENT" NOTIFICATION TEXT WILL POP UP) 
    ///    CLICK THAT A MARKER SHOULD POP INTO PLACE WHERE SPECIFICALLY WHERE YOU CLICKED THE TIME BUFFER
    ///    TO BE.
    /// 7. CLICK THE MARKER THAT HAS JUST BEEN CREATED. IN THE INSPECTOR PANEL THERE SHOULD BE FUNCTION A
    ///    FUNCTION DROPDOWNA AND AN ARGUMENT LIST TO FOLLOW IT BELOW.
    /// 8. CHOOSE THE FUNCTION THAT YOU CREATED FROM THE DROPDOWN LIST OR CHOOSE ANY OF THE PROVIDED ONE
    ///    IF YOU ARE JUST TESTING.
    /// 9. AND FILL THE CORRESPONDING PARAMETERS (REMEMBER STRINGS AND INTS ARE THE ONLY PARAMETERS ALLOWED
    ///    IN THE CURRENT BUILD OF UNITY)
    /// 10. CONGRATS YOU MADE A KEY TIME EVENT!!!!!
    /// </summary>
    /// 
    public AnotationManager anoManager = null;

    void Pause()
    {
        anoManager.Pause();
    }

    void PauseRewind()
    {
        anoManager.PauseRewind();
    }

    void Show()
    {
        anoManager.View(anoManager.Canvas);
    }

    void StepText(int index)
    {
        anoManager.SetText(index);
    }

    void ShowAnnotations(int _index)
    {
        anoManager.anoObjList[_index].SetActive(true);
    }

    void HideAnnotations(int _index)
    {
        anoManager.anoObjList[_index].SetActive(false);
    }

    void HideallAnotations()
    {
        anoManager.HideAllObject();
    }

}
