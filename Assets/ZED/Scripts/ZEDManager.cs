//using UnityEngine;
//using System;

//public class ZEDManager : MonoBehaviour {
//    public sl.zed.ZEDCamera zedCamera;
//    private float[] pos;
//    private Matrix4x4 matrix;

//    public sl.zed.ZEDCamera.ZEDResolution_mode resolutionMode = sl.zed.ZEDCamera.ZEDResolution_mode.HD1080;
//    public float requestedFPS = 0.0f;
//    public sl.zed.ZEDCamera.MODE computingMode = sl.zed.ZEDCamera.MODE.PERFORMANCE;

//    public bool tracking = true;
//    public bool rendering = true;

//    // Use this for initialization
//    void Awake () {
//        zedCamera = sl.zed.ZEDCamera.GetInstance();
//        //Create a camera
//        zedCamera.CreateCamera(resolutionMode, requestedFPS);
//        Debug.Log("ZED SDK Version " + sl.zed.ZEDCamera.GetSDKVersion());

//        //Initialize the camera in performance mode.
//        sl.zed.ZEDCamera.ERRCODE e = zedCamera.Init(computingMode);
//        if (e != sl.zed.ZEDCamera.ERRCODE.SUCCESS)
//        {
//            throw new Exception("Initialization failed " + e.ToString());
//        }

//        pos = IdentityMatrix();
//        if (this.tracking)
//        {
//            //Enables the tracking
//            if (!(this.tracking = zedCamera.EnableTracking(pos, false)))
//                throw new Exception("Error, tracking not available");
//        }
//    }

//	// Update is called once per frame
//	void Update () {
//        if (zedCamera != null)
//        {
//            //Grab is needed to the tracking and the rendering
//            if (zedCamera.Grab(sl.zed.ZEDCamera.SENSING_MODE.FILL) == 0)
//            {
//                //Fill the different textures
//                if(rendering)
//                    zedCamera.Render();

//                if (this.tracking)
//                {
//                    zedCamera.GetPositionCamera(pos, sl.zed.ZEDCamera.MAT_TRACKING_TYPE.PATH);

//                    for (int i = 0; i < 4; ++i)
//                    {
//                        for (int j = 0; j < 4; ++j)
//                        {
//                            matrix[i, j] = pos[i * 4 + j];
//                        }
//                    }

//                    Vector4 v4 = matrix.GetColumn(3);
//                    Vector3 translate = new Vector3(v4.x, v4.y, v4.z);
//                    Quaternion rotation = sl.zed.ZEDCamera.Matrix4ToQuaternion(matrix);

//                    transform.localRotation = rotation;
//                    transform.localPosition = translate;
//                }
//            }
//        }
//    }


//    private float[] IdentityMatrix()
//    {
//        float[] pos = new float[16];

//        for (int i = 0; i < 16; ++i)
//        {
//            pos[i] = 0;
//        }
//        pos[0] = pos[5] = pos[10] = pos[15] = 1;
//        return pos;
//    }

//    void OnApplicationQuit()
//    {
//        if (zedCamera != null)
//            zedCamera.Destroy();
//        zedCamera = null;
//    }
//}


using UnityEngine;
using System;



public class ZEDManager : MonoBehaviour
{

    static bool init = false;

    public sl.zed.ZEDCamera zedCamera;
    private float[] pos;
    private Matrix4x4 matrix;

    public sl.zed.ZEDCamera.ZEDResolution_mode resolutionMode = sl.zed.ZEDCamera.ZEDResolution_mode.HD1080;
    public float requestedFPS = 0.0f;
    public sl.zed.ZEDCamera.MODE computingMode = sl.zed.ZEDCamera.MODE.PERFORMANCE;

    public bool tracking = true;
    public bool rendering = true;

    //System.Threading.Thread grab_thread = null;
    //volatile bool running = true;
    //volatile bool isCalculated = false;
    //System.Threading.Mutex protect;

    //void grab()
    //{
    //    while (running)
    //    {
    //        if (isCalculated == false && zedCamera.Grab(sl.zed.ZEDCamera.SENSING_MODE.FILL) == 0)
    //        {
    //            isCalculated = true;
    //        }
    //    }
    //}

    // Use this for initialization
    void Awake()
    {
        zedCamera = sl.zed.ZEDCamera.GetInstance();

        //Initialize the camera in performance mode.

        if (!init)
        {
            //Create a camera
            zedCamera.CreateCamera(resolutionMode, requestedFPS);
            Debug.Log("ZED SDK Version " + sl.zed.ZEDCamera.GetSDKVersion());

            sl.zed.ZEDCamera.ERRCODE e = zedCamera.Init(computingMode);
            if (e != sl.zed.ZEDCamera.ERRCODE.SUCCESS)
            {
                throw new Exception("Initialization failed " + e.ToString());
            }

            init = true;
        }

        pos = IdentityMatrix();
        if (this.tracking)
        {
            //Enables the tracking
            if (!(this.tracking = zedCamera.EnableTracking(pos, false)))
                throw new Exception("Error, tracking not available");
        }

        //grab_thread = new System.Threading.Thread(grab);
        //grab_thread.Start();
        //System.Threading.Mutex protect = new System.Threading.Mutex();
    }

    // Update is called once per frame
    void Update()
    {
        if (zedCamera != null)
        {
            //Grab is needed to the tracking and the rendering
            if (zedCamera.Grab(sl.zed.ZEDCamera.SENSING_MODE.FILL) == 0)
            {
                //Fill the different textures
                if (rendering/* && isCalculated*/)
                {
                    zedCamera.Render();
                    //isCalculated = false;
                }
                    

                if (this.tracking)
                {
                    zedCamera.GetPositionCamera(pos, sl.zed.ZEDCamera.MAT_TRACKING_TYPE.PATH);

                    for (int i = 0; i < 4; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            matrix[i, j] = pos[i * 4 + j];
                        }
                    }

                    Vector4 v4 = matrix.GetColumn(3);
                    Vector3 translate = new Vector3(v4.x, v4.y, v4.z);
                    Quaternion rotation = sl.zed.ZEDCamera.Matrix4ToQuaternion(matrix);

                    //transform.localRotation = rotation;
                    //transform.localPosition = translate;
                }
            }
        }
    }


    private float[] IdentityMatrix()
    {
        float[] pos = new float[16];

        for (int i = 0; i < 16; ++i)
        {
            pos[i] = 0;
        }
        pos[0] = pos[5] = pos[10] = pos[15] = 1;
        return pos;
    }

    void OnApplicationQuit()
    {
        //running = false;
        //grab_thread.Join();

        if (zedCamera != null)
            zedCamera.Destroy();

        zedCamera = null;
    }
}
