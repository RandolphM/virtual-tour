using UnityEngine;
using System.Collections;
using UnityEditor;

public class ZEDEditor : EditorWindow
{
    private const int cbrightness = 4;
    private const int ccontrast = 4;
    private const int chue = 0;
    private const int csaturation = 4;

    private int brightness = 4;
    private int contrast = 4;
    private int hue = 0;
    private int saturation = 4;

    private int gain;
    private int exposure;
    //private float fps;
    private bool groupAuto;
    Color defaultColor;
    static GUIStyle style = new GUIStyle();

    private sl.zed.ZEDCamera zedCamera;

    int tab = 0;


    bool isInit = false;

    private GUILayoutOption[] optionsButton = { GUILayout.MaxWidth(100) };

    static sl.zed.StereoParameters parameters = new sl.zed.StereoParameters();

    public ZEDEditor()
    {
        zedCamera = sl.zed.ZEDCamera.GetInstance();
    }


    [MenuItem("Window/ZEDEditor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ZEDEditor window = (ZEDEditor)EditorWindow.GetWindow(typeof(ZEDEditor));

        style.normal.textColor = Color.red;
        style.fontSize = 15;
        style.margin.left = 5;

        parameters = new sl.zed.StereoParameters();
        parameters.leftCam.disto = new double[5];
        parameters.rightCam.disto = new double[5];

        window.Show();

    }

    void OnFocus()
    {
        if (zedCamera.CameraIsReady)
        {
            parameters = zedCamera.GetParameters();

            brightness = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_BRIGHTNESS);
            contrast = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_CONTRAST);
            hue = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_HUE);
            saturation = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_SATURATION);
            gain = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN);
            exposure = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE);
            //fps = ZEDCamera.GetInstance().GetFPS();
        }
    }



    void FirstInit()
    {
        if (!isInit)
        {
            if (zedCamera.CameraIsReady)
            {
                isInit = true;

                brightness = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_BRIGHTNESS);
                contrast = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_CONTRAST);
                hue = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_HUE);
                saturation = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_SATURATION);
                gain = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN);
                exposure = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE);

                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN, gain, true);
                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE, exposure, true);
                parameters = zedCamera.GetParameters();
            }
        }
    }

    void CameraSettingsView()
    {
        GUILayout.Label("Mode", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
        GUILayout.Label("ZED FPS: " + zedCamera.GetFPS());
        GUILayout.Label("Resolution " + zedCamera.WidthImage + " x " + zedCamera.HeightImage);
        GUILayout.EndVertical();

        GUILayout.Label("Settings", EditorStyles.boldLabel);

        
        EditorGUI.BeginChangeCheck();
        brightness = EditorGUILayout.IntSlider("Brightness", brightness, 0, 8);
        contrast = EditorGUILayout.IntSlider("Contrast", contrast, 0, 8);
        hue = EditorGUILayout.IntSlider("Hue", hue, 0, 11);
        saturation = EditorGUILayout.IntSlider("Saturation", saturation, 0, 8);

        groupAuto = EditorGUILayout.BeginToggleGroup("Manual", groupAuto);
        gain = EditorGUILayout.IntSlider("Gain", gain, 0, 100);
        exposure = EditorGUILayout.IntSlider("Exposure", exposure, 0, 100);
        EditorGUILayout.EndToggleGroup();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset", optionsButton))
        {
            brightness = cbrightness;
            contrast = ccontrast;
            hue = chue;
            saturation = csaturation;

            zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_BRIGHTNESS, brightness, false);
            zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_CONTRAST, contrast, false);
            zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_HUE, hue, false);
            zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_SATURATION, saturation, false);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (zedCamera.CameraIsReady)
        {
            if (EditorGUI.EndChangeCheck())
            {
                //fps = ZEDCamera.GetInstance().GetFPS();
                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_BRIGHTNESS, brightness, false);
                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_CONTRAST, contrast, false);
                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_HUE, hue, false);
                zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_SATURATION, saturation, false);

                if (groupAuto)
                {
                    zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN, gain, false);
                    zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE, exposure, false);
                }
                else
                {
                    zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN, gain, true);
                    zedCamera.SetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE, exposure, true);
                    gain = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_GAIN);
                    exposure = zedCamera.GetCameraSettings(sl.zed.ZEDCamera.ZEDCamera_settings.ZED_EXPOSURE);
                }
            }
        }
    }

    void LabelHorizontal(string name, float value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name);
        GUILayout.Box(value.ToString());
        GUILayout.EndHorizontal();
    }

    void CalibrationSettingsView()
    {
        if (parameters.leftCam.disto == null)
        {
            parameters.leftCam.disto = new double[5];
            parameters.rightCam.disto = new double[5];
        }
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Left camera", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        LabelHorizontal("fx", parameters.leftCam.fx);
        LabelHorizontal("fy", parameters.leftCam.fy);
        LabelHorizontal("cx", parameters.leftCam.cx);
        LabelHorizontal("cy", parameters.leftCam.cy);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("k1", (float)parameters.leftCam.disto[0]);
        LabelHorizontal("k2", (float)parameters.leftCam.disto[1]);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("vFOV", parameters.leftCam.vFOV);
        LabelHorizontal("hFOV", parameters.leftCam.hFOV);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("Right camera", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        GUILayout.BeginVertical(EditorStyles.helpBox);

        LabelHorizontal("fx", parameters.rightCam.fx);
        LabelHorizontal("fy", parameters.rightCam.fy);
        LabelHorizontal("cx", parameters.rightCam.cx);
        LabelHorizontal("cy", parameters.rightCam.cy);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("k1", (float)parameters.rightCam.disto[0]);
        LabelHorizontal("k2", (float)parameters.rightCam.disto[1]);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        LabelHorizontal("vFOV", parameters.rightCam.vFOV);
        LabelHorizontal("hFOV", parameters.rightCam.hFOV);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Stereo", EditorStyles.boldLabel);

        GUILayout.BeginVertical(EditorStyles.helpBox);
        LabelHorizontal("Baseline", parameters.baseline);
        LabelHorizontal("Convergence", parameters.convergence);
        GUILayout.EndVertical();

        GUILayout.Label("Optional", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        LabelHorizontal("Rx", parameters.Rx);
        LabelHorizontal("Rz", parameters.Rz);
        GUILayout.EndVertical();
        
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void OnGUI()
    {
        FirstInit();
        defaultColor = GUI.color;
        if (zedCamera.CameraIsReady)
            GUI.color = Color.green;
        else GUI.color = Color.red;
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.FlexibleSpace();
        if (zedCamera.CameraIsReady)
        {
            style.normal.textColor = Color.black;
            GUILayout.Label("Connected", style);
        }
        else
        {
            style.normal.textColor = Color.black;
            GUILayout.Label("Not Connected", style);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.color = defaultColor;
        EditorGUI.BeginChangeCheck();
        tab = GUILayout.Toolbar(tab, new string[] { "Camera", "Calibration" });
        if (EditorGUI.EndChangeCheck())
        {


            if (zedCamera.CameraIsReady)
            {
                parameters = zedCamera.GetParameters();
              
            }
        }
        switch (tab)
        {
            case 0:
                CameraSettingsView();
                break;
            case 1:

                CalibrationSettingsView();
                break;
            default:
                CameraSettingsView();
                break;
        }
    }
}
