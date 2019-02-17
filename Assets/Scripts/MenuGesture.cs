using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//[RequireComponent(typeof(SteamVR_TrackedController))]

public class MenuGesture : MonoBehaviour
{

    public GameObject worldMenu = null;
    public GameObject Menu = null;

    bool isOpen = false;

    public float controllerRot;

    // Use this for initialization
    void Start()
    {
    }

    void OpenMenu(bool _isActive = false, GameObject _toInstantiate = null)
    {
        if (_isActive == false && _toInstantiate != null)
            Instantiate(worldMenu);
    }
    // Update is called once per frame
    void Update()
    {
        

        controllerRot = this.transform.eulerAngles.z;
        //print(this.transform.localPosition);

        if(controllerRot >= 80 && controllerRot < 110)
        {
            worldMenu.SetActive(true);
            Menu.SetActive(true);
        }
        else
        {
            worldMenu.SetActive(false);
            Menu.SetActive(false);
        }
    }
}
