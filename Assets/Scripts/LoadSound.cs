using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSound : MonoBehaviour {

    public AnotationManager anoManager = null;
    private void OnEnable()
    {
        anoManager.PlayAudio();
    }

}
