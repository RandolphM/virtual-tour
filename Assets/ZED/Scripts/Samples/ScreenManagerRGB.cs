using UnityEngine;
using System.Collections;

public class ScreenManagerRGB : MonoBehaviour {

    public GameObject screen;

    private Material mat;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        mat = screen.GetComponent<Renderer>().material;

        sl.zed.ZEDCamera zedCamera = sl.zed.ZEDCamera.GetInstance();
        if (zedCamera.CameraIsReady && gameObject.transform.parent.GetComponent<ZEDManager>().tracking)
        {
            mainCamera.ResetProjectionMatrix();
            mainCamera.projectionMatrix = zedCamera.Projection;
            scale(screen.gameObject, zedCamera.GetFOV());
        }
        else
        {
            scale(screen.gameObject, mainCamera.fieldOfView * Mathf.Deg2Rad);
        }
        mat.SetTexture("_MainTex", zedCamera.CreateTextureImageType(sl.zed.ZEDCamera.SIDE.LEFT));
    }

    private void scale(GameObject screen, float fov)
    {
        float height = Mathf.Tan(0.5f * fov) * Mathf.Abs(Mathf.Sqrt(screen.transform.localPosition.sqrMagnitude)) * 2;
        screen.transform.localScale = new Vector3(height * mainCamera.aspect, height, 1);
    }
}
