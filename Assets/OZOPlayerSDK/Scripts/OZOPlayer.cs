//Copyright © 2017 Nokia Corporation and/or its subsidiary(-ies). All rights reserved.

//#define ENABLE_DEBUG
//#define ENABLE_DEBUG_LOG //use debug delegate to get native debug printout to Unity console

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define UNITY_GLES_RENDERER
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace OZO
{
	public enum VideoPlaybackState
	{
		INVALID = -1,      /// Invalid enum

		IDLE,              /// Video playback is idle, no file has been loaded
		LOADING,           /// Loading a video is in progress
		PAUSED,            /// Video playback is paused
		BUFFERING,         /// Video playback is buffering, not used currently
		PLAYING,           /// Video playback is in progress
		END_OF_FILE,       /// Video has reached the end of the file or stream
		STREAM_NOT_FOUND,  ///< Cannot find the stream on the server
		CONNECTION_ERROR,  /// Connection error
		STREAM_ERROR,      /// Stream error

		COUNT              /// Enum max value
	};

	public enum ErrorCodes
	{
		OK = 0,                      /// Operation succeeded normally
		NO_CHANGE,                   /// Operation didn't have an effect
		END_OF_FILE,                 /// End of file has been reached
		FILE_PARTIALLY_SUPPORTED,    /// The file was opened but only part of the contents can be played

		// generic errors
		OUT_OF_MEMORY = 100,         /// Operation failed due to lack of memory
		OPERATION_FAILED,            /// Generic failure

		// state errors
		INVALID_STATE = 200,         /// Operation was not due to invalid state
		ITEM_NOT_FOUND,              /// Operation failed due to a missing item
		BUFFER_OVERFLOW,             /// Buffer has overflown

		// input not accepted
		NOT_SUPPORTED = 300,         /// Action or file is not supported
		INVALID_DATA,                /// Data is invalid

		// operational errors
		FILE_NOT_FOUND = 400,        /// File was not found
		FILE_OPEN_FAILED,            /// The file was found but it can't be accessed
		FILE_NOT_SUPPORTED,          /// The file is an MP4 but in incorrect format

		NETWORK_ACCESS_FAILED,       /// Failed to access network

		COUNT
	};

	//This used internally by the API to get events properly synced with the render thread
	enum RenderEvent
	{
		GFX_INIT = 0x31337,
		GFX_RENDER,
		GFX_STATE_UPDATE,
		GFX_MAX_EVENTS
	};

	public enum Feature
	{
		INVALID = -1,   /// Invalid enum
		HEVC,           /// H265 / HEVC support
		DEPTH,          /// Depth data
		UHD_PER_EYE,    /// 4k per eye
		COUNT,          /// Enum max value
	};

	public enum InitializingState
	{
		NOT_STARTED = 0,
		INITIALIZING,
		INIT_OK,
		INIT_ERROR
	};

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void PlaybackObserverDelegate(OZO.VideoPlaybackState state);

	[Serializable]
	public struct ClipArea
	{
		[SerializeField]
		[Range(-90.0f, 90.0f)]
		public float centerLatitude;       // degrees, [-90, 90]

		[SerializeField]
		[Range(-180.0f, 180.0f)]
		public float centerLongitude;      // degrees, [-180, 180]

		[SerializeField]
		[Range(0.0f, 180.0f)]
		public float spanLatitude;         // degrees, [0, 180]

		[SerializeField]
		[Range(0.0f, 360.0f)]
		public float spanLongitude;        // degrees, [0, 360]

		[SerializeField]
		[Range(0, 1)]
		public float opacity;              // [0, 1]
	};
}


public interface IOZOPlayer
{
	//Init has to be called from Awake(), analytics and watermark might not have effect, depending on the license
	bool Init(bool useAnalytics, bool useWatermark, bool allowExclusiveModeAudio);
	string GetVersion();
	bool IsFeatureSupported(OZO.Feature feature);
	event OZO.PlaybackObserverDelegate OnPlayModeChanged;
	OZO.VideoPlaybackState GetCurrentVideoPlaybackState();
	OZO.ErrorCodes GetLastError();
	bool LoadVideo(string filepath);
	bool Play();
	void Pause();
	void Stop();
	bool IsSeekable();
	bool SeekTo(UInt64 milliSeconds);
	UInt64 ElapsedTime();
	UInt64 Duration();
	void SetAudioVolume(float volume);
	void SetViewRotationOffset(Quaternion rotation);
	void SetProjectionMatrix(Matrix4x4 m, int eyeIdx);
	void SetEyeDistance(float distanceBetweenEyes);
	void SetVisible(bool visible);
	void SetMonoscopicRendering(bool monoscopic);
	void SetClearColor(Color clearColor);
	void SetClipAreas(OZO.ClipArea[] clipAreas);

	bool CreateAuxiliaryRenderTarget(RenderTexture renderTexture);
	int GetAuxiliaryVideoPlaybackState();
	bool LoadAuxiliaryVideo([MarshalAs(UnmanagedType.LPStr)] string filepath);
	bool PlayAuxiliary();
	void PauseAuxiliary();
	void StopAuxiliary();
	bool SeekToAuxiliary(UInt64 aMilliSeconds);
	UInt64 ElapsedTimeAuxiliary();
	UInt64 DurationAuxiliary();
	void SetAudioVolumeAuxiliary(float gain);
}

/// <summary>
/// The interface to the OZO Player SDK
/// </summary>
public class OZOPlayer : MonoBehaviour, IOZOPlayer
{
	public event OZO.PlaybackObserverDelegate OnPlayModeChanged;

	//SDK properties
	private Quaternion rotationOffset = Quaternion.identity;
	OZO.VideoPlaybackState currentState = OZO.VideoPlaybackState.INVALID;

	[SerializeField]
	public string licenseId = "INSERT_YOUR_OZO_LICENSE_ID_HERE";

	//Camera
	[HideInInspector] //drawn with custom editor
	public float OZOCameraDepth = 1.0f;
	[HideInInspector] //drawn with custom editor
	public Vector2 OZOCameraClipPlanes = new Vector2(0.01f, 100.0f);
	[HideInInspector] //drawn with custom editor
	public CameraClearFlags OZOCameraClearFlags = CameraClearFlags.Color;
	[HideInInspector] //drawn with custom editor
	public Color OZORendererClearColor = Color.clear;

	[SerializeField]
	[HideInInspector]
	public System.Collections.Generic.List<OZO.ClipArea> clipAreas;

	[HideInInspector] //drawn with custom editor
	public Material OZOCameraMaterial;

	private int width = -1;
	private int height = -1;
	private IntPtr renderFunc = IntPtr.Zero;
	private bool camerasCreated = false;
	private bool initializeRequested = false;
	private bool initialized = false;

	[NonSerialized]
	public Camera[] OZOViewCameras; //< eye camera root game objects

	[NonSerialized]
	public OZORender[] OZOViewRenderer;

	private OZOInitInfo initInfo;

	//////////////////////////////////////////////////////////////////////////////
	/// Interface
	//////////////////////////////////////////////////////////////////////////////
	public bool Init(bool useAnalytics, bool useWatermark, bool allowExclusiveModeAudio)
	{
		bool res = false;
		if (licenseId == "INSERT_YOUR_OZO_LICENSE_ID_HERE")
		{
			Debug.LogError("OZOPlayerSDK - Init failure, please give a valid license id to OZOPlayer.\n");
#if UNITY_EDITOR
			Selection.activeGameObject = transform.gameObject;
			EditorUtility.DisplayDialog("License ID missing", "Please insert a valid license id to OZOPlayer", "Ok");
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		else
		{
#if ENABLE_DEBUG
			Debug.Log("OZOPlayerSDK - Initializing\n");
#endif
			CreateUnityOZOCameras();
			InitProjections();

			//only single camera needed for non-VR
#if UNITY_2017_2_OR_NEWER
			if (!UnityEngine.XR.XRDevice.isPresent)
#else
			if (!UnityEngine.VR.VRDevice.isPresent)
#endif
			{
				OZOViewCameras[0].transform.GetComponent<OZORender>().enabled = false;
			}
#if ENABLE_DEBUG
			Debug.Log("OZOPlayerSDK - Initialize SDK\n");
#endif
			InitializeSDK(licenseId, useAnalytics, useWatermark, allowExclusiveModeAudio);
			initializeRequested = false;
			res = true;
		}
		return res;
	}

	public string GetVersion()
	{
		return OZO_GetVersionNumber();
	}

	public bool IsFeatureSupported(OZO.Feature feature)
	{
		return OZO_IsFeatureSupported(feature);
	}

	public OZO.InitializingState IsInitialized()
	{
		return OZO_IsInitialized();
	}

	public OZO.VideoPlaybackState GetCurrentVideoPlaybackState()
	{
		return currentState; //only the updater consumes states (one state per frame)
	}

	public OZO.ErrorCodes GetLastError()
	{
		return OZO_GetLastError();
	}

	public bool LoadVideo(string filepath)
	{
		return OZO_LoadVideo(filepath);
	}

	public bool Play()
	{
		return OZO_Play();
	}

	public void Pause()
	{
		OZO_Pause();
	}

	public void Stop()
	{
		OZO_Stop();
	}

	public bool IsSeekable()
	{
		return OZO_IsSeekable();
	}

	public bool SeekTo(UInt64 milliSeconds)
	{
		return OZO_SeekTo(milliSeconds);
	}

	public UInt64 ElapsedTime()
	{
		return OZO_ElapsedTime();
	}

	public UInt64 Duration()
	{
		return OZO_Duration();
	}

	public void SetAudioVolume(float volume)
	{
		OZO_SetAudioVolume(volume);
	}

	public void SetViewRotationOffset(Quaternion rotation)
	{
		rotationOffset = rotation;
	}

	public void SetProjectionMatrix(Matrix4x4 m, int eyeIdx)
	{
		float[] projectionMatrix =
		{
				m.m00, m.m01, m.m02, m.m03,
				m.m10, m.m11, m.m12, m.m13,
				m.m20, m.m21, m.m22, m.m23,
				m.m30, m.m31, m.m32, m.m33
			};
		OZO_SetProjectionMatrix(projectionMatrix, eyeIdx);
	}

	public void SetEyeDistance(float distanceBetweenEyes)
	{
		OZO_SetEyeDistance(distanceBetweenEyes * 0.5f, 0);
		OZO_SetEyeDistance(distanceBetweenEyes * 0.5f, 1);
	}

	public void SetVisible(bool visible)
	{
		if (camerasCreated)
		{
			OZOViewCameras[0].enabled = visible;
			OZOViewCameras[1].enabled = visible;
			OZOViewRenderer[0].renderingEnabled = visible;
			OZOViewRenderer[1].renderingEnabled = visible;
		}
	}

	public void SetMonoscopicRendering(bool monoscopic)
	{
		if (camerasCreated)
		{
			OZO_SetMonoscopicRendering(monoscopic);
		}
	}

	public void SetClearColor(Color clearColor)
	{
		OZO_SetClearColor(clearColor.r, clearColor.g, clearColor.b, clearColor.a);
	}

	public void SetClipAreas(OZO.ClipArea[] clipAreas)
	{
		float[] clipAreaParams = new float[clipAreas.Length * typeof(OZO.ClipArea).GetFields().Length];

		int index = 0;
		foreach (OZO.ClipArea area in clipAreas)
		{
			foreach (var param in typeof(OZO.ClipArea).GetFields())
			{
				object value = param.GetValue(area);
				if (value.GetType().IsValueType)
				{
					clipAreaParams[index++] = (float)param.GetValue(area);
				}
			}
		}
		OZO_SetClipAreas(clipAreaParams, clipAreas.Length, typeof(OZO.ClipArea).GetFields().Length);
	}

	///AUX
	public bool CreateAuxiliaryRenderTarget(RenderTexture renderTexture)
	{
		IntPtr rt = renderTexture.GetNativeTexturePtr();
		return OZO_CreateAuxiliaryRenderTarget(rt, (uint)renderTexture.width, (uint)renderTexture.height);
	}
	public int GetAuxiliaryVideoPlaybackState()
	{
		return OZO_GetAuxiliaryVideoPlaybackState();
	}
	public bool LoadAuxiliaryVideo(string filepath)
	{
		OZO_LoadAuxiliaryVideo(filepath);
		return true;
	}
	public bool PlayAuxiliary()
	{
		return OZO_PlayAuxiliary();
	}
	public void PauseAuxiliary()
	{
		OZO_PauseAuxiliary();
	}
	public void StopAuxiliary()
	{
		OZO_StopAuxiliary();
	}
	public bool SeekToAuxiliary(UInt64 aMilliSeconds)
	{
		return OZO_SeekToAuxiliary(aMilliSeconds);
	}
	public UInt64 ElapsedTimeAuxiliary()
	{
		return OZO_ElapsedTimeAuxiliary();
	}
	public UInt64 DurationAuxiliary()
	{
		return OZO_DurationAuxiliary();
	}
	public void SetAudioVolumeAuxiliary(float volume)
	{
		OZO_SetAudioVolumeAuxiliary(volume);
	}

	//////////////////////////////////////////////////////////////////////////////
	/// Unity Scene
	//////////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		//Clear the Unity data
		OZOViewRenderer = null;
		OZOViewCameras = null;

#if ENABLE_DEBUG
		Debug.Log("OZOPlayerSDK - Shutting down\n");
#endif
		OZO_DeinitSDK();
#if ENABLE_DEBUG
		Debug.Log("OZOPlayerSDK - Shut down\n");
#endif
		RemoveCameras();
		if (initInfo != null && initInfo.drmHandler != System.IntPtr.Zero)
		{
			DRM_deleteDRMHandler(initInfo.drmHandler);
		}
	}

	private void Update()
	{
		if (IntPtr.Zero != renderFunc)
		{
			if (initialized)
			{
				UpdatePlaybackMode();
#if UNITY_2017_2_OR_NEWER
				UpdateHMD(Time.timeSinceLevelLoad, UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head));
#else
				UpdateHMD(Time.timeSinceLevelLoad, UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head));
#endif
				GL.IssuePluginEvent(renderFunc, (int)OZO.RenderEvent.GFX_STATE_UPDATE);
				GL.IssuePluginEvent(renderFunc, (int)OZO.RenderEvent.GFX_RENDER);
			}
			else if (!initializeRequested)
			{
				initializeRequested = true;
				//on certain platforms the initialization needs to be synchronized with the rendering
				GL.IssuePluginEvent(renderFunc, (int)OZO.RenderEvent.GFX_INIT);
			}
			//cache the initialization info
			else if (OZO.InitializingState.INIT_OK == IsInitialized())
			{
				initialized = true;
				Matrix4x4 m0 = OZOViewCameras[0].GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
				Matrix4x4 m1 = OZOViewCameras[1].GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

				//now initialized and ready to set the projection matrices
				SetProjectionMatrix(m0.transpose, 0);
				SetProjectionMatrix(m1.transpose, 1);
				SetEyeDistance(OZOViewCameras[0].stereoSeparation);
#if UNITY_2017_2_OR_NEWER
				if (!UnityEngine.XR.XRSettings.enabled)
#else
				if (!UnityEngine.VR.VRSettings.enabled)
#endif
				{
					float width = Screen.width;
					float height = Screen.height;
					Matrix4x4 m = Matrix4x4.Perspective(90.0f, width / height, OZOViewCameras[0].nearClipPlane, OZOViewCameras[0].farClipPlane).transpose;
					SetProjectionMatrix(m, 0);
					SetProjectionMatrix(m, 1);
				}
			}
			else if (OZO.InitializingState.INIT_ERROR == IsInitialized())
			{
				Debug.LogError("SDK Init failure");
			}
		}
	}

	public void UpdatePlaybackMode()
	{
		if (OnPlayModeChanged != null)
		{
			OZO.VideoPlaybackState state = OZO_GetCurrentVideoPlaybackState();
			if (state != currentState)
			{
				currentState = state;
				OnPlayModeChanged(currentState);
			}
		}
	}

	private void UpdateHMD(float time, Quaternion orientation)
	{
		OZO_SetTime(Time.timeSinceLevelLoad);
		Quaternion q = rotationOffset * orientation;
		OZO_SetHMDOrientation(-q.x, -q.y, q.z, q.w);
	}

	private void RemoveCameras()
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			Destroy(transform.GetChild(i));
		}
	}

	/// <summary>
	/// Dynamically generates the stereoscopic cameras, textures and projection.
	/// </summary>
	private void CreateUnityOZOCameras()
	{
		if (!camerasCreated)
		{
			OZOViewRenderer = new OZORender[2];
			OZOViewCameras = new Camera[2];

			for (int i = 0; i < 2; ++i)
			{
				string cameraName = "OZOEye" + ((0 == i) ? "Left" : "Right");
				if (transform.Find(cameraName))
				{
					continue;
				}
				GameObject camObject = new GameObject(cameraName);
				OZOViewCameras[i] = camObject.AddComponent<Camera>();
				OZOViewCameras[i].nearClipPlane = OZOCameraClipPlanes.x;
				OZOViewCameras[i].farClipPlane = OZOCameraClipPlanes.y;
				OZOViewCameras[i].depthTextureMode = DepthTextureMode.Depth;

				OZOViewCameras[i].useOcclusionCulling = true;
				OZOViewCameras[i].stereoTargetEye = (0 == i) ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
				camObject.transform.SetParent(transform);

				OZOViewCameras[i].depth = OZOCameraDepth;
				OZOViewCameras[i].clearFlags = OZOCameraClearFlags;
				OZOViewCameras[i].cullingMask = 0;
				OZOViewRenderer[i] = camObject.AddComponent<OZORender>();
			}
			camerasCreated = true;
		}
	}

	private void InitProjections()
	{
		//initialize projection
#if UNITY_2017_2_OR_NEWER
		if (UnityEngine.XR.XRSettings.enabled)
#else
		if (UnityEngine.VR.VRSettings.enabled)
#endif
		{
			width = OZOViewCameras[0].pixelWidth;
			height = OZOViewCameras[0].pixelHeight;
			if (width != OZOViewCameras[1].pixelWidth || height != OZOViewCameras[1].pixelHeight)
			{
				Debug.Log("Error: Mismatching camera sizes\n");
			}
		}
		else
		{
			width = Screen.width;
			height = Screen.height;
		}
	}

	private IntPtr GetRenderFunc()
	{
		return OZO_GetRenderEventFunc();
	}

	private void InitializeSDK(string licenseId, bool useAnalytics, bool useWatermark, bool allowExclusiveModeAudio)
	{
#if ENABLE_DEBUG_LOG
#if !UNITY_IPHONE
		SetupDebugging();
#endif
#endif

#if ENABLE_DEBUG
		Debug.Log("OZOPlayerSDK: [AnalyticsEnabled:" + useAnalytics.ToString() + "]\n");
#endif
		{
			for (int i = 0; i < 2; ++i)
			{
				OZOViewRenderer[i].material = new Material(OZOCameraMaterial);
				OZOViewRenderer[i].renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
				OZOViewRenderer[i].renderTexture.Create();

				OZOViewRenderer[i].renderTextureDepth = new RenderTexture(width, height, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.sRGB);
				OZOViewRenderer[i].renderTextureDepth.Create();

				OZOViewRenderer[i].material.SetTexture("_MainTex", OZOViewRenderer[i].renderTexture);
				OZOViewRenderer[i].material.SetTexture("_DepthTex", OZOViewRenderer[i].renderTextureDepth);

				OZOViewRenderer[i].eyeDiff = OZOViewCameras[i].stereoSeparation * ((0 == i) ? 0.5f : -0.5f);
			}

			initInfo = new OZOInitInfo();
			initInfo.texturePtr0 = OZOViewRenderer[0].renderTexture.GetNativeTexturePtr();
			initInfo.texturePtr1 = OZOViewRenderer[1].renderTexture.GetNativeTexturePtr();
			initInfo.depthTexturePtr0 = OZOViewRenderer[0].renderTextureDepth.GetNativeDepthBufferPtr();
			initInfo.depthTexturePtr1 = OZOViewRenderer[1].renderTextureDepth.GetNativeDepthBufferPtr();

			initInfo.width = width;
			initInfo.height = height;
			initInfo.storagePath = Application.persistentDataPath;
#if (UNITY_ANDROID && !UNITY_EDITOR)
            Debug.Log("InitInfo: Android\n");
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject assetManager = jo.Call<AndroidJavaObject>("getAssets");

            initInfo.activity = jo.GetRawObject();
            initInfo.assetManager = assetManager.GetRawObject();
            initInfo.cachePath = Application.temporaryCachePath;

#if ENABLE_DEBUG
            Debug.Log("InitInfo: activity: " + initInfo.activity + "\n");
            Debug.Log("InitInfo: assetManager: " + initInfo.assetManager + "\n");
            Debug.Log("InitInfo: cachePath: " + initInfo.cachePath + "\n");
#endif

#else
			initInfo.assetPath = Application.streamingAssetsPath;
#endif

            // create a custom DRM handler
            initInfo.drmHandler = LoadDrmLibrary(ref initInfo);

            initInfo.useAnalytics = useAnalytics ? 1 : 0;
			initInfo.useWatermark = useWatermark ? 1 : 0;
			initInfo.licenseId = licenseId;
			initInfo.allowExclusiveModeAudio = allowExclusiveModeAudio ? 1 : 0;

			//managed handle lock (to keep the memory in the struct available)
			float[] colors = new float[4] { OZORendererClearColor.r, OZORendererClearColor.g, OZORendererClearColor.b, OZORendererClearColor.a };
			GCHandle colorHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);
			initInfo.clearColor = Marshal.UnsafeAddrOfPinnedArrayElement(colors, 0);

			// Call unmanaged code
			bool result = InitSDK(ref initInfo);

			//free handles
			colorHandle.Free();

#if ENABLE_DEBUG
			Debug.Log("OZOPlayerSDK - initialized: " + result + "\n");
#endif
			if (result)
			{
#if ENABLE_DEBUG
				for (int i = 0; i < (int)OZO.Feature.COUNT; ++i)
				{
					Debug.Log("OZOPlayerSDK - this device supports: " + ((OZO.Feature)i).ToString() + ": " + IsFeatureSupported((OZO.Feature)i).ToString());
				}
#endif
				//start the rendering
				renderFunc = GetRenderFunc();

				if (0 < clipAreas.Count)
				{
					SetClipAreas(clipAreas.ToArray());
				}
			}
			else
			{
				Debug.LogError("OZOPlayerSDK - Error Initializing OZO Player SDK");
			}
		}
	}

	//Create the textures on the Unity side
	public bool InitSDK(ref OZOInitInfo initInfo)
	{
		bool res = false;
		try
		{
			res = OZO_InitSDK(ref initInfo);
		}
		catch (DllNotFoundException e)
		{
			Debug.LogError("OZOPlayerSDK - library, or one of its dependencies is missing.\n" + e.ToString());
		}
		return res;
	}

    public IntPtr LoadDrmLibrary(ref OZOInitInfo initInfoRef)
    {
        try
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
            return DRM_createDRMHandler(initInfoRef.activity);
#else
            return DRM_createDRMHandler();
#endif
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError("UnityDRMSample - library, or one of its dependencies is missing.\n" + e.ToString());
        }
        return System.IntPtr.Zero;
    }

    /////////////////////////////////////////////////////////////////
    // External DLL
    /////////////////////////////////////////////////////////////////
#if UNITY_IPHONE && !UNITY_EDITOR
    private const System.String OZO_PLAY_NATIVE = "__Internal";
    private const System.String DRM_SAMPLE_NATIVE = "__Internal";
#elif (UNITY_ANDROID && !UNITY_EDITOR)
    private const System.String OZO_PLAY_NATIVE = "OZOPlayerUnity";
    private const System.String DRM_SAMPLE_NATIVE = "UnityDRMSample";
#else
    private const System.String OZO_PLAY_NATIVE = "OZOPlayerUnity";
    private const System.String DRM_SAMPLE_NATIVE = "UnityDRMSample";
#endif
    private const System.String OZO_DBG = "OZO: ";

	//////////////////////////////////////////////////////////////////////////
	// Player Interface
	//////////////////////////////////////////////////////////////////////////
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class OZOInitInfo
	{
		public System.IntPtr texturePtr0;
		public System.IntPtr texturePtr1;
		public System.IntPtr depthTexturePtr0;
		public System.IntPtr depthTexturePtr1;
		[MarshalAs(UnmanagedType.LPStr)]
		public string licenseId;
		[MarshalAs(UnmanagedType.LPStr)]
		public string storagePath;
		public System.IntPtr clearColor;
		public System.IntPtr drmHandler;
#if (UNITY_ANDROID && !UNITY_EDITOR)
        public System.IntPtr activity;
        public System.IntPtr assetManager;
        [MarshalAs(UnmanagedType.LPStr)]
        public string cachePath;
#else
		[MarshalAs(UnmanagedType.LPStr)]
		public string assetPath;
#endif
		public int width;
		public int height;
		public int useAnalytics;
		public int useWatermark;
		public int allowExclusiveModeAudio;
	}

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_InitSDK(ref OZOInitInfo info);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_DeinitSDK();

	[DllImport(OZO_PLAY_NATIVE, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.LPStr)]
	private static extern string OZO_GetVersionNumber();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_IsFeatureSupported(OZO.Feature feature);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern OZO.InitializingState OZO_IsInitialized();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern IntPtr OZO_GetRenderEventFunc();

	//////////////////////////////////////////////////////////////////////////
	// Runtime parameters
	//////////////////////////////////////////////////////////////////////////
	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetTime(float t);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetProjectionMatrix(float[] projectionMatrix, int eyeIdx);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetEyeDistance(float distanceBetweenEyes, int eyeIdx);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetHMDOrientation(float x, float y, float z, float w);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetMonoscopicRendering(bool monoscopic);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetClearColor(float r, float g, float b, float a);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetClipAreas(float[] clipAreaParams, int numAreas, int numParams);

	//////////////////////////////////////////////////////////////////////////
	// PlaybackControls
	//////////////////////////////////////////////////////////////////////////
	[DllImport(OZO_PLAY_NATIVE)]
	public static extern void OZO_SetVideoPlaybackObserver(OZO.PlaybackObserverDelegate fp);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern OZO.VideoPlaybackState OZO_GetCurrentVideoPlaybackState();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern OZO.ErrorCodes OZO_GetLastError();

	[DllImport(OZO_PLAY_NATIVE)]
	public static extern bool OZO_LoadVideo([MarshalAs(UnmanagedType.LPStr)] string filepath);

	[DllImport(OZO_PLAY_NATIVE)]
	public static extern bool OZO_Play();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_Pause();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_Stop();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_IsSeekable();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_SeekTo(UInt64 milliSeconds);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern UInt64 OZO_ElapsedTime();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern UInt64 OZO_Duration();

	//////////////////////////////////////////////////////////////////////////
	// AUXILIARY CONTROLS
	//////////////////////////////////////////////////////////////////////////
	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_CreateAuxiliaryRenderTarget(System.IntPtr texturePtr, uint width, uint height);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern int OZO_GetAuxiliaryVideoPlaybackState();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_LoadAuxiliaryVideo([MarshalAs(UnmanagedType.LPStr)] string filepath);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_PlayAuxiliary();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_PauseAuxiliary();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_StopAuxiliary();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern bool OZO_SeekToAuxiliary(UInt64 aMilliSeconds);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern UInt64 OZO_ElapsedTimeAuxiliary();

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern UInt64 OZO_DurationAuxiliary();

	//////////////////////////////////////////////////////////////////////////
	/// Audio
	//////////////////////////////////////////////////////////////////////////

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetAudioVolume(float volume);

	[DllImport(OZO_PLAY_NATIVE)]
	private static extern void OZO_SetAudioVolumeAuxiliary(float volume);
	/// FOR DRM
	//////////////////////////////////////////////////////////////////////////
#if (UNITY_ANDROID && !UNITY_EDITOR)
    [DllImport(DRM_SAMPLE_NATIVE)]
    private static extern IntPtr DRM_createDRMHandler(IntPtr jobject);
#else
	[DllImport(DRM_SAMPLE_NATIVE)]
	private static extern IntPtr DRM_createDRMHandler();
#endif

	[DllImport(DRM_SAMPLE_NATIVE)]
	private static extern IntPtr DRM_deleteDRMHandler(IntPtr handler);

	//////////////////////////////////////////////////////////////////////////
	/// FOR DEBUG
	//////////////////////////////////////////////////////////////////////////
#if ENABLE_DEBUG_LOG
#if !UNITY_IPHONE
	public void SetupDebugging()
	{
		DebugDelegate debugDelegate = new DebugDelegate(DebugCallback);
		IntPtr delegatePtr = Marshal.GetFunctionPointerForDelegate(debugDelegate);
		OZO_SetDebugFunction(delegatePtr);
	}

	[DllImport(OZO_PLAY_NATIVE)]
	public static extern void OZO_SetDebugFunction(IntPtr fp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void DebugDelegate(string str);

	static void DebugCallback(string str)
	{
		Debug.Log("Unity [native] " + OZO_DBG + str + "\n");
	}
#endif
#endif
}
