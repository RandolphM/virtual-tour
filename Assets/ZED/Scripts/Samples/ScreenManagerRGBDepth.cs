//using UnityEngine;

//[RequireComponent(typeof(Camera))]
//public class ScreenManagerRGBDepth : MonoBehaviour {

//    public GameObject screen;

//    private Material matRGB;
//    private Camera mainCamera;
//    public sl.zed.ZEDCamera.SIDE side;
//    Texture2D camZedLeft;
//    Texture2D depthXYZZed;

//    void Start()
//    {
//        mainCamera = GetComponent<Camera>();
//        //Set textures to the shader
//        matRGB = screen.GetComponent<Renderer>().material;
//        sl.zed.ZEDCamera zedCamera = sl.zed.ZEDCamera.GetInstance();
//        //Create two textures and fill them with the ZED computed images
//        if (side == sl.zed.ZEDCamera.SIDE.LEFT_GREY || side == sl.zed.ZEDCamera.SIDE.RIGHT_GREY || side == sl.zed.ZEDCamera.SIDE.LEFT_UNRECTIFIED_GREY || side == sl.zed.ZEDCamera.SIDE.RIGHT_UNRECTIFIED_GREY)
//        {
//            matRGB.SetInt("_isGrey", 1);
//        } else
//        {
//            matRGB.SetInt("_isGrey", 0);

//        }
//        camZedLeft = zedCamera.CreateTextureImageType(side);
//        depthXYZZed = zedCamera.CreateTextureMeasureType(sl.zed.ZEDCamera.MEASURE.XYZ);
//        matRGB.SetTexture("_CameraTex", camZedLeft);
//        matRGB.SetTexture("_DepthXYZTex", depthXYZZed);
//        Matrix4x4 projectionMatrixQuad = zedCamera.Projection;
//        projectionMatrixQuad[0, 2] = 0;
//        projectionMatrixQuad[1, 2] = 0;

//        matRGB.SetMatrix("_ProjectionMatrix", sl.zed.ZEDCamera.FormatProjectionMatrix(projectionMatrixQuad, mainCamera.actualRenderingPath));
//        //Set a new projection matrix to this camera, it's used by the tracking
//        if (zedCamera.CameraIsReady && gameObject.transform.parent.GetComponent<ZEDManager>().tracking)
//        {
//            mainCamera.ResetProjectionMatrix();
//            mainCamera.projectionMatrix = zedCamera.Projection;
//            scale(screen.gameObject, zedCamera.GetFOV());
//        } else
//        {
//            scale(screen.gameObject, mainCamera.fieldOfView*Mathf.Deg2Rad);
//        }
//    }

//    /// <summary>
//    /// Scale a screen in front of the camera, where all the textures will be rendrered.
//    /// </summary>
//    /// <param name="screen"></param>
//    /// <param name="fov"></param>
//    private void scale(GameObject screen, float fov)
//    {
//        float height = Mathf.Tan(0.5f * fov) * Mathf.Abs(Mathf.Sqrt(screen.transform.localPosition.sqrMagnitude)) * 2;
//        screen.transform.localScale = new Vector3(height * mainCamera.aspect, height, 1);
//    }
//}


using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenManagerRGBDepth : MonoBehaviour
{

    public GameObject screen;

    private Material matRGB;
    private Camera mainCamera;
    public sl.zed.ZEDCamera.SIDE side;
    Texture2D camZedLeft;
    Texture2D depthXYZZed;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        //Set textures to the shader
        matRGB = screen.GetComponent<Renderer>().material;
        sl.zed.ZEDCamera zedCamera = sl.zed.ZEDCamera.GetInstance();
        //Create two textures and fill them with the ZED computed images
        if (side == sl.zed.ZEDCamera.SIDE.LEFT_GREY || side == sl.zed.ZEDCamera.SIDE.RIGHT_GREY || side == sl.zed.ZEDCamera.SIDE.LEFT_UNRECTIFIED_GREY || side == sl.zed.ZEDCamera.SIDE.RIGHT_UNRECTIFIED_GREY)
        {
            matRGB.SetInt("_isGrey", 1);
        }
        else
        {
            matRGB.SetInt("_isGrey", 0);

        }
        camZedLeft = zedCamera.CreateTextureImageType(side);
        depthXYZZed = zedCamera.CreateTextureMeasureType(sl.zed.ZEDCamera.MEASURE.XYZ);
        matRGB.SetTexture("_CameraTex", camZedLeft);
        matRGB.SetTexture("_DepthXYZTex", depthXYZZed);
        Matrix4x4 projectionMatrixQuad = zedCamera.Projection;
        projectionMatrixQuad[0, 2] = 0;
        projectionMatrixQuad[1, 2] = 0;

        matRGB.SetMatrix("_ProjectionMatrix", sl.zed.ZEDCamera.FormatProjectionMatrix(projectionMatrixQuad, mainCamera.actualRenderingPath));

        //Set a new projection matrix to this camera, it's used by the tracking
        if (zedCamera.CameraIsReady && gameObject.transform.parent.GetComponent<ZEDManager>().tracking)
        {
            mainCamera.ResetProjectionMatrix();
            mainCamera.fieldOfView = (zedCamera.GetFOV() / Mathf.PI) * 180.0f;
            mainCamera.projectionMatrix = zedCamera.Projection;
            scale(screen.gameObject, zedCamera.GetFOV());
        }
        else
        {
            scale(screen.gameObject, mainCamera.fieldOfView * Mathf.Deg2Rad);
        }
    }

    /// <summary>
    /// Scale a screen in front of the camera, where all the textures will be rendrered.
    /// </summary>
    /// <param name="screen"></param>
    /// <param name="fov"></param>
    private void scale(GameObject screen, float fov)
    {
        screen.transform.localPosition = new Vector3(0, 0, 1.0f / GlobalScale.scale);
        float height = Mathf.Tan(0.5f * fov) * Mathf.Abs(Mathf.Sqrt(screen.transform.localPosition.sqrMagnitude)) * 2;
        screen.transform.localScale = new Vector3(height * mainCamera.aspect, -height, -1);
    }

    /*Added This For Constant Appropriate Scale*/
    private void Update()
    {
        matRGB.SetFloat("light_x", GlobalScale.light_position[0]);
        matRGB.SetFloat("light_y", GlobalScale.light_position[1]);
        matRGB.SetFloat("light_z", GlobalScale.light_position[2]);
        matRGB.SetFloat("human_scale", GlobalScale.scale);
        matRGB.SetFloat("A_GREEN_WEIGHT", GlobalScale.A_GREEN_WEIGHT);
        matRGB.SetFloat("B_GREEN_WEIGHT", GlobalScale.B_GREEN_WEIGHT);
        matRGB.SetFloat("GREEN_THRESH", GlobalScale.GREEN_THRESH);
        matRGB.SetFloat("time_step", Time.time);

        //if (zedCamera.CameraIsReady && gameObject.transform.parent.GetComponent<ZEDManager>().tracking)
        //{
        //    mainCamera.ResetProjectionMatrix();
        //    mainCamera.projectionMatrix = zedCamera.Projection;
        //    scale(screen.gameObject, zedCamera.GetFOV());
        //}
        //else
        //{
        scale(screen.gameObject, mainCamera.fieldOfView * Mathf.Deg2Rad);
        // }
    }
}
