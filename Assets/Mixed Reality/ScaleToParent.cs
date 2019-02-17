using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

static class GlobalScale
{
    public static float[] light_position = { 0.0f, 0.0f, 0.0f };
    public static float scale = 1.0f;
    public static float A_GREEN_WEIGHT = 1.0f;
    public static float B_GREEN_WEIGHT = 1.0f;
    public static float GREEN_THRESH = 0.0f;
}

public class ScaleToParent : MonoBehaviour {

    public SteamVR_TrackedController left_controller = null;
    public SteamVR_TrackedController right_controller = null;
    bool isLeftMenu = false;
    bool isRightMenu = false;

    bool isDown = false;
    bool isUp = false;
    bool isLeft = false;
    bool isRight = false;

    bool isRUp = false;
    bool isRDown = false;

    void SaveGreen()
    {
        FileStream fs = new FileStream("greensave.bin", FileMode.Truncate);
        if(fs!=null)
        {
            BinaryWriter w = new BinaryWriter(fs);
            w.Write(GlobalScale.A_GREEN_WEIGHT);
            w.Write(GlobalScale.B_GREEN_WEIGHT);
            w.Write(GlobalScale.GREEN_THRESH);
            w.Close();
            fs.Close();
            SteamVR_Controller.Input((int)right_controller.controllerIndex).TriggerHapticPulse(3999);
        }
        
    }

    bool LoadGreen()
    {
        FileStream fs = new FileStream("greensave.bin", FileMode.OpenOrCreate);

        if(fs != null)
        {
            BinaryReader r = new BinaryReader(fs);
            GlobalScale.A_GREEN_WEIGHT = r.ReadSingle();
            GlobalScale.B_GREEN_WEIGHT = r.ReadSingle();
            GlobalScale.GREEN_THRESH = r.ReadSingle();
            r.Close();
            fs.Close();
            SteamVR_Controller.Input((int)right_controller.controllerIndex).TriggerHapticPulse(3999);
            return true;
        }
        else
        {
            //GlobalScale.A_GREEN_WEIGHT = 1.0f;

            return false;
        }
    }

    // Use this for initialization
    void Start () {
        left_controller.PadTouched += HandleLeftPadClicked;
        right_controller.PadTouched += HandleRightPadClicked;

        right_controller.PadClicked += HandleRightPadPressed;

        left_controller.PadUntouched += HandleLeftPadUnClicked;
        right_controller.PadUntouched += HandleRightPadUnClicked;

        left_controller.MenuButtonClicked += HandleLeftMenunDown;
        left_controller.MenuButtonUnclicked += HandleLeftMenunUp;

        right_controller.MenuButtonClicked += HandleRightMenunDown;
        right_controller.MenuButtonUnclicked += HandleRightMenunUp;

        bool ret = LoadGreen();
        if(!ret)
            Debug.Log("File Fail");
        else
            Debug.Log("File Success");
    }

    //private void HandleLeftPadPressed(object sender, ClickedEventArgs e)
    //{
    //    if(isLeftMenu)
    //}

    private void HandleRightPadPressed(object sender, ClickedEventArgs e)
    {
        if(isLeftMenu)
        {
            SaveGreen();
            Debug.Log("Save File");
            SteamVR_Controller.Input((int)left_controller.controllerIndex).TriggerHapticPulse(3999);

        }
    }

    private void HandleLeftPadClicked(object sender, ClickedEventArgs e)
    {
        isDown = false;
        isUp = false;
        isLeft = false;
        isRight = false;

        if (isRightMenu)
        {
            if (e.padY > 0.5)
                isUp = true;//GlobalScale.A_GREEN_WEIGHT += 0.0025f;

            if (e.padY < -0.5)
                isDown = true;//GlobalScale.A_GREEN_WEIGHT -= 0.0025f;

            if (e.padX > 0.5)
                isRight = true;//GlobalScale.B_GREEN_WEIGHT += 0.0025f;

            if (e.padX < -0.5)
                isLeft = true;//GlobalScale.B_GREEN_WEIGHT -= 0.0025f;
        }
    }

    private void HandleRightPadClicked(object sender, ClickedEventArgs e)
    {
        isRUp = false;
        isRDown = false;

        if (isLeftMenu)
        {
            if (e.padY > 0.5)
                isRUp = true;//GlobalScale.GREEN_THRESH += 0.0025f;

            if (e.padY < -0.5)
                isRDown = true;//GlobalScale.GREEN_THRESH -= 0.0025f;
        }
    }

    private void HandleLeftPadUnClicked(object sender, ClickedEventArgs e)
    {
        isDown = false;
        isUp = false;
        isLeft = false;
        isRight = false;
    }

    private void HandleRightPadUnClicked(object sender, ClickedEventArgs e)
    {
        isRUp = false;
        isRDown = false;
    }

    private void HandleLeftMenunDown(object sender, ClickedEventArgs e)
    {
        SteamVR_Controller.Input((int)left_controller.controllerIndex).TriggerHapticPulse(3999);
        isLeftMenu = true;
    }

    private void HandleLeftMenunUp(object sender, ClickedEventArgs e)
    {
        isLeftMenu = false;
    }

    private void HandleRightMenunDown(object sender, ClickedEventArgs e)
    {
        isRightMenu = true;
    }

    private void HandleRightMenunUp(object sender, ClickedEventArgs e)
    {
        isRightMenu = false;
    }

    // Update is called once per frame
    void Update ()
    {

        if(isRDown)
            GlobalScale.GREEN_THRESH -= Time.deltaTime*0.1f;

        if (isRUp)
            GlobalScale.GREEN_THRESH += Time.deltaTime*0.1f;

        if (isDown)
            GlobalScale.A_GREEN_WEIGHT -= Time.deltaTime * 0.1f;

        if (isUp)
            GlobalScale.A_GREEN_WEIGHT += Time.deltaTime * 0.1f;

        if (isLeft)
            GlobalScale.B_GREEN_WEIGHT -= Time.deltaTime * 0.1f;

        if (isRight)
            GlobalScale.B_GREEN_WEIGHT += Time.deltaTime * 0.1f;

        Transform p = this.transform.parent;

        if(p != null)
        {
            GlobalScale.scale = p.lossyScale.z;
            //print(GlobalScale.scale);
        }
    }
}
