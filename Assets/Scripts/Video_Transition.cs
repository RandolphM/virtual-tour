using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class Video_Transition : MonoBehaviour {

    public string StreamingAssets_VideoPath = null;

    Coroutine corout = null;

    IEnumerator FadePlay()
    {
        SteamVR_Fade.Start(Color.black, 1.0f);

        GameObject ozo_cam = GameObject.Find("OZO_Camera");
        PlayControl player = ozo_cam.GetComponentInChildren<PlayControl>();

        if(StreamingAssets_VideoPath != null)
        {
            if (File.Exists(StreamingAssets_VideoPath))
                player._videoFiles[0] = StreamingAssets_VideoPath;
            else
                player._videoFiles[0] = "asset://placeholder.vrmp4";
        }

        player._startPlay = true;
        player._play.SetVisible(false);

        yield return new WaitForSeconds(1.0f);

        Camera.allCameras[0].cullingMask = 0;
        player._play.SetVisible(true);
    }

    /*I'm Not Using This Right Now*/
    IEnumerator FadeStop()
    {
        SteamVR_Fade.Start(Color.clear, 1.0f);

        GameObject ozo_cam = GameObject.Find("OZO_Camera");
        PlayControl player = ozo_cam.GetComponentInChildren<PlayControl>();
        player._play.SetVisible(false);
        player._startPlay = false;
        player._play.Stop();

        Camera.allCameras[0].cullingMask = -1;

        yield return new WaitForSeconds(0.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        /*Fade Out and Play Video*/
        if(other.gameObject.tag == "Head")
        {
            print("ON PLAY TRIGGER ENTER");
            corout = StartCoroutine(FadePlay());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        /*Fade The Camera Back to World and Stop Video*/
        if (other.gameObject.tag == "Head")
        {
            print("ON STOP TRIGGER EXIT");
            if (corout != null)
                StopCoroutine(corout);

            SteamVR_Fade.Start(Color.clear, 1.0f);

            GameObject ozo_cam = GameObject.Find("OZO_Camera");
            PlayControl player = ozo_cam.GetComponentInChildren<PlayControl>();
            player._play.SetVisible(false);
            player._startPlay = false;
            player._play.Stop();

            Camera.allCameras[0].cullingMask = -1;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
