using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTest : MonoBehaviour {
    public string describe = string.Empty;

    public void ButtonChanged( bool state )
    {
        Debug.Log(describe + state);
    }
}
