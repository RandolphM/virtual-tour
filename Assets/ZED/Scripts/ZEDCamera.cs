using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace sl
{
    namespace zed
    {
        public struct Resolution
        {
            public int width;
            public int height;
        };

        public struct TextureRequested
        {
            public int type;
            public int option;
        };

        public struct CamParameters
        {
            public float fx;
            public float fy;
            public float cx;
            public float cy;
            public float vFOV;
            public float hFOV;
            public float dFOV;
            public double[] disto;
        };

        public struct StereoParameters
        {
            public float baseline;
            public float Ty;
            public float Tz;
            public float convergence;
            public float Rx;
            public float Rz;
            public CamParameters leftCam;
            public CamParameters rightCam;
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct Recording_state
        {
            [FieldOffset(0)]
            public bool left;
            [FieldOffset(1)]
            public double current_compression_time; /*!< compression time for the current frame in ms */
            [FieldOffset(9)]
            public double current_compression_ratio; /*!< compression ratio (% of raw size) for the current frame*/
            [FieldOffset(17)]
            public double average_compression_time; /*!< average compression time in ms since beginning of recording*/
            [FieldOffset(25)]
            public double average_compression_ratio; /*!< compression ratio (% of raw size) since beginning of recording*/
        }

        public class ZEDCamera
        {
            public enum ZED_SELF_CALIBRATION_STATUS
            {
                SELF_CALIBRATION_NOT_CALLED,
                SELF_CALIBRATION_RUNNING,
                SELF_CALIBRATION_FAILED,
                SELF_CALIBRATION_SUCCESS
            };

            public enum MODE
            {
                NONE,
                PERFORMANCE,
                MEDIUM,
                QUALITY
            };

            public enum UNIT
            {
                MILLIMETER,
                METER,
                INCH,
                FOOT
            };

            public enum ERRCODE
            {
                SUCCESS, NO_GPU_COMPATIBLE, NOT_ENOUGH_GPUMEM, ZED_NOT_AVAILABLE,
                ZED_RESOLUTION_INVALID, ZED_SETTINGS_FILE_NOT_AVAILABLE, INVALID_SVO_FILE, RECORDER_ERROR,
                INVALID_COORDINATE_SYSTEM, ZED_WRONG_FIRMWARE, NO_NEW_FRAME, CUDA_ERROR_THROWN,
                ZED_NOT_INITIALIZED, LAST_ERRCODE
            };

            public enum ZEDResolution_mode
            {
                HD2K,
                HD1080,
                HD720,
                VGA
            };

            public enum SENSING_MODE
            {
                FILL,
                STANDARD
            };

            public enum TYPE_VIEW
            {
                GET_VIEW,
                RETRIEVE_IMAGE,
                RETRIEVE_MEASURE,
                NORMALIZE_MEASURE
            }

            public enum SIDE
            {
                LEFT,
                RIGHT,
                LEFT_GREY,
                RIGHT_GREY,
                LEFT_UNRECTIFIED,
                RIGHT_UNRECTIFIED,
                LEFT_UNRECTIFIED_GREY,
                RIGHT_UNRECTIFIED_GREY,
            };

            public enum ZEDCamera_settings
            {
                ZED_BRIGHTNESS,
                ZED_CONTRAST,
                ZED_HUE,
                ZED_SATURATION,
                ZED_GAIN,
                ZED_EXPOSURE
            };

            public enum VIEW_MODE
            {
                STEREO_ANAGLYPH, STEREO_DIFF, STEREO_SBS, STEREO_OVERLAY
            };

            public enum MEASURE
            {
                DISPARITY,
                DEPTH,
                CONFIDENCE,
                XYZ,
                XYZRGBA
            };

            public enum MAT_TRACKING_TYPE
            {
                PATH,
                POSE
            };

            public enum TRACKING_FRAME_STATE
            {
                TRACKING_FRAME_NORMAL, /*!< Not a keyframe, normal behavior \ingroup Enumerations*/
                TRACKING_FRAME_KEYFRAME, /*!< The tracking detect a new reference image \ingroup Enumerations*/
                TRACKING_FRAME_CLOSE, /*!< The tracking find a previously known area and optimize the trajectory\ingroup Enumerations*/
                TRACKING_FRAME_LAST
            }

            public enum SVO_COMPRESSION_MODE
            {
                RAW_BASED, /*!< RAW images, no compression) \ingroup Enumerations*/
                LOSSLESS_BASED, /*!< new Lossless, with png/zstd based compression : avg size = 42% of RAW) \ingroup Enumerations*/
                LOSSY_BASED /*!< new Lossy, with jpeg based compression : avg size = 22% of RAW) \ingroup Enumerations*/
            }

            const string nameDll = "sl_unitywrapper";
            private Dictionary<int, Dictionary<int, Texture2D>> textures;
            private List<TextureRequested> texturesRequested;
            private int widthImage;
            private int heightImage;
            private bool cameraIsReady = false;
            private Matrix4x4 projection = new Matrix4x4();
            private static ZEDCamera instance = null;

            [DllImport(nameDll, EntryPoint = "GetRenderEventFunc")]
            private static extern IntPtr GetRenderEventFunc();

            [DllImport(nameDll, EntryPoint = "dllz_create_live_camera")]
            private static extern void dllz_create_live_camera(int mode = (int)ZEDResolution_mode.HD1080, float fps = 0.0f, int linux_id = 0);

            [DllImport(nameDll, EntryPoint = "dllz_create_svo_camera")]
            private static extern void dllz_create_svo_camera(byte[] svo_path);

            [DllImport(nameDll, EntryPoint = "dllz_destroy_camera")]
            private static extern void dllz_destroy_camera();

            [DllImport(nameDll, EntryPoint = "dllz_init")]
            private static extern int dllz_init(int mode, int unit, bool verbose, int device, float minDist, bool disable, bool vflip);

            [DllImport(nameDll, EntryPoint = "dllz_grab")]
            private static extern int dllz_grab(int sensingMode, int computeMeasure, int computeDisparity);

            [DllImport(nameDll, EntryPoint = "dllz_reset_self_calibration")]
            private static extern bool dllz_reset_self_calibration();

            [DllImport(nameDll, EntryPoint = "dllz_enable_recording")]
            private static extern int dllz_enable_recording(byte[] video_filename, int compresssionMode);

            [DllImport(nameDll, EntryPoint = "dllz_record")]
            private static extern void dllz_record(ref Recording_state state);

            [DllImport(nameDll, EntryPoint = "dllz_stop_recording")]
            private static extern bool dllz_stop_recording();

            [DllImport(nameDll, EntryPoint = "dllz_enable_tracking")]
            private static extern bool dllz_enable_tracking(float[] position, bool enableAreaLearning, string areaDBpath);

            [DllImport(nameDll, EntryPoint = "dllz_stop_tracking")]
            private static extern void dllz_stop_tracking();

            [DllImport(nameDll, EntryPoint = "dllz_register_texture_view_type")]
            private static extern int dllz_register_texture_view_type(int option, IntPtr id);

            [DllImport(nameDll, EntryPoint = "dllz_register_texture_image_type")]
            private static extern int dllz_register_texture_image_type(int option, IntPtr id);

            [DllImport(nameDll, EntryPoint = "dllz_register_texture_measure_type")]
            private static extern int dllz_register_texture_measure_type(int option, IntPtr id);

            [DllImport(nameDll, EntryPoint = "dllz_register_texture_normalize_measure_type")]
            private static extern int dllz_register_texture_normalize_measure_type(int option, IntPtr id, float min = 0.0f, float max = 0.0f);

            [DllImport(nameDll, EntryPoint = "dllz_set_confidence_threshold")]
            private static extern void dllz_set_confidence_threshold(int threshold);

            [DllImport(nameDll, EntryPoint = "dllz_set_depth_clamp_value")]
            private static extern void dllz_set_depth_clamp_value(float distanceMax);

            [DllImport(nameDll, EntryPoint = "dllz_get_fps")]
            private static extern float dllz_get_fps();

            [DllImport(nameDll, EntryPoint = "dllz_get_width")]
            private static extern int dllz_get_width();

            [DllImport(nameDll, EntryPoint = "dllz_get_height")]
            private static extern int dllz_get_height();

            [DllImport(nameDll, EntryPoint = "dllz_get_parameters")]
            private static extern IntPtr dllz_get_parameters();

            [DllImport(nameDll, EntryPoint = "dllz_get_zed_Serial")]
            private static extern int dllz_get_zed_Serial();

            [DllImport(nameDll, EntryPoint = "dllz_get_zed_firmware")]
            private static extern int dllz_get_zed_firmware();

            [DllImport(nameDll, EntryPoint = "dllz_get_self_calibration_status")]
            private static extern int dllz_get_self_calibration_status();

            [DllImport(nameDll, EntryPoint = "dllz_get_tracking_confidence")]
            private static extern float dllz_get_tracking_confidence();

            [DllImport(nameDll, EntryPoint = "dllz_get_position_camera")]
            private static extern int dllz_get_position_camera(float[] position, int mat_type);

            [DllImport(nameDll, EntryPoint = "dllz_set_camera_settings_value")]
            private static extern void dllz_set_camera_settings_value(int mode, int value, int usedefault);

            [DllImport(nameDll, EntryPoint = "dllz_get_camera_settings_value")]
            private static extern int dllz_get_camera_settings_value(int mode);

            [DllImport(nameDll, EntryPoint = "dllz_is_zed_connected")]
            private static extern int dllz_is_zed_connected();

            [DllImport(nameDll, EntryPoint = "dllz_get_sdk_version")]
            private static extern IntPtr dllz_get_sdk_version();

            [DllImport(nameDll, EntryPoint = "dllz_set_flip")]
            private static extern void dllz_set_flip(bool flip);

            [DllImport(nameDll, EntryPoint = "dllz_set_fps")]
            private static extern bool dllz_set_fps(int fps);

            [DllImport(nameDll, EntryPoint = "dllz_set_svo_position")]
            private static extern bool dllz_set_svo_position(int frame);

            [DllImport(nameDll, EntryPoint = "dllz_get_confidence_threshold")]
            private static extern int dllz_get_confidence_threshold();

            [DllImport(nameDll, EntryPoint = "dllz_get_camera_timestamp")]
            private static extern ulong dllz_get_camera_timestamp();

            [DllImport(nameDll, EntryPoint = "dllz_get_tracking_frame_state")]
            private static extern uint dllz_get_tracking_frame_state();

            [DllImport(nameDll, EntryPoint = "dllz_get_svo_number_of_frames")]
            private static extern int dllz_get_svo_number_of_frames();

            [DllImport(nameDll, EntryPoint = "dllz_get_closest_depth_value")]
            private static extern float dllz_get_closest_depth_value();

            [DllImport(nameDll, EntryPoint = "dllz_save_area_learning_db")]
            private static extern bool dllz_save_area_learning_db(byte[] svo_path);

            /*
            [DllImport(nameDll, EntryPoint = "getTrackingFrameState")]
            private static extern int getTrackingFrameState();
             
            [DllImport(nameDll, EntryPoint = "saveAreaLearningDB")]
            private static extern bool saveAreaLearningDB(string areaDBpath);
            */

            private static string PtrToStringUtf8(IntPtr ptr) // aPtr is nul-terminated
            {
                if (ptr == IntPtr.Zero)
                {
                    return "";
                }
                int len = 0;
                while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
                    len++;
                if (len == 0)
                {
                    return "";
                }
                byte[] array = new byte[len];
                System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
                return System.Text.Encoding.ASCII.GetString(array);
            }

            private static byte[] StringUtf8ToByte(string str)
            {
                byte[] array = System.Text.Encoding.ASCII.GetBytes(str);
                return array;
            }

            /// <summary>
            /// Get a quaternion from a matrix with a minimum size of 3x3
            /// </summary>
            /// <param name="m">The matrix </param>
            /// <returns>A quaternion which contains the rotation</returns>
            public static Quaternion Matrix4ToQuaternion(Matrix4x4 m)
            {
                Quaternion q = new Quaternion();
                q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
                q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
                q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
                q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
                q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
                q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
                q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
                return q;
            }

            /// <summary>
            /// Format a matrix in the Unity format. This should only be used with projection matrix passing into shaders.
            /// </summary>
            /// <param name="m">The matrix</param>
            /// <returns></returns>
            public static Matrix4x4 FormatProjectionMatrix(Matrix4x4 m, RenderingPath renderingPath)
            {
                if (renderingPath == RenderingPath.DeferredShading)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        m[1, i] = -m[1, i];
                    }
                }
            
                return m;
            }

            /// <summary>
            /// Gets an instance of camera
            /// </summary>
            public static ZEDCamera GetInstance()
            {
                if (instance == null)
                {
                    instance = new ZEDCamera();
                }
                return instance;
            }

            private ZEDCamera()
            {
                textures = new Dictionary<int, Dictionary<int, Texture2D>>();
                texturesRequested = new List<TextureRequested>();
            }

            /// <summary>
            /// Create a live zed camera
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="fps"></param>
            /// <param name="linux_id"></param>
            public void CreateCamera(ZEDResolution_mode mode = ZEDResolution_mode.HD720, float fps = 0.0f, int linux_id = 0)
            {
                string infoSystem = SystemInfo.graphicsDeviceType.ToString().ToUpper();
                if (!infoSystem.Equals("DIRECT3D11"))
                {
                    throw new Exception("The graphic library ["+infoSystem+"] is not supported");
                }
                dllz_destroy_camera();

                dllz_create_live_camera((int)mode, fps, linux_id);
            }

            /// <summary>
            /// Create a svo camera
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="fps"></param>
            /// <param name="linux_id"></param>
            public void CreateCamera(string svoPath)
            {
                string infoSystem = SystemInfo.graphicsDeviceType.ToString().ToUpper();
                if (!infoSystem.Equals("DIRECT3D11"))
                {
                    throw new Exception("The graphic library [" + infoSystem + "] is not supported");
                }
                dllz_destroy_camera();

                dllz_create_svo_camera(StringUtf8ToByte(svoPath));
            }

            /// <summary>
            /// Stop all components of the ZED.
            /// To restart them you need to recreate a camera
            /// </summary>
            public void Destroy()
            {
                cameraIsReady = false;
                DestroyAllTexture();
                dllz_destroy_camera();
            }
            /// <summary>
            /// The init function must be called after the instantiation. The function checks if the ZED camera is plugged and opens it, if the graphics card is compatible, allocates the memory and launches the automatic calibration.
            /// </summary>
            /// <param name="mode_">defines the quality of the disparity map, affects the level of details and also the computation time.</param>
            /// <param name="unit_">define the unit metric for all the depth-distance values. Unity expects Meter, other unit can have unexpected behaviors</param>
            /// <param name="verbose_"> if set to true, it will output some information about the current status of initialization.</param>
            /// <param name="device_">defines the graphics card on which the computation will be done. The default value -1 search the more powerful usable GPU.</param>
            /// <param name="minDist_">specify the minimum depth information that will be computed, in the unit you previously define.</param>
            /// <param name="disable">if set to true, it will disable self-calibration and take the optional calibration parameters without optimizing them</param>
            /// <returns>ERRCODE : The error code gives information about the
            /// internal process, if SUCCESS is returned, the camera is ready to use.
            /// Every other code indicates an error and the program should be stopped.
            /// 
            /// For more details see sl::zed::ERRCODE.</returns>
            public ERRCODE Init(MODE mode_ = MODE.PERFORMANCE, UNIT unit_ = UNIT.METER, bool verbose_ = false, int device_ = -1, float minDist_ = -1, bool disable = false)
            {
                int v = dllz_init((int)mode_, (int)unit_, verbose_, device_, minDist_, disable, false);
                if (v == -1 || (ERRCODE)v != ERRCODE.SUCCESS)
                {
                    cameraIsReady = false;
                    throw new Exception("Error init camera, no zed available [" + ((ERRCODE)v).ToString() + "]");
                }
                cameraIsReady = true;
                widthImage = dllz_get_width();
                heightImage = dllz_get_height();

                FillProjectionMatrix();
            
                return ((ERRCODE)v);
            }

            private void FillProjectionMatrix(float zFar = 500, float zNear = 0.1f)
            {
                StereoParameters parameters = GetParameters();
                float fovx = Mathf.Atan(WidthImage / (parameters.leftCam.fx * 2.0f)) * 2.0f;
                float fovy = Mathf.Atan(HeightImage / (parameters.leftCam.fy * 2.0f)) * 2.0f;

                projection[0, 0] = 1.0f / Mathf.Tan(fovx * 0.5f);
                projection[0, 1] = 0;
                projection[0, 2] = 2 * ((WidthImage - 1 * parameters.leftCam.cx) / WidthImage) - 1.0f;
                projection[0, 3] = 0;

                projection[1, 0] = 0;
                projection[1, 1] = 1.0f / Mathf.Tan(fovy * 0.5f);
                projection[1, 2] = 2 * ((HeightImage - 1 * parameters.leftCam.cy) / HeightImage) - 1.0f;
                projection[1, 3] = 0;

                projection[2, 0] = 0;
                projection[2, 1] = 0;
                projection[2, 2] = -(zFar + zNear) / (zFar - zNear);
                projection[2, 3] = -(2.0f * zFar * zNear) / (zFar - zNear);

                projection[3, 0] = 0;
                projection[3, 1] = 0;
                projection[3, 2] = -1;
                projection[3, 3] = 0;
            }

            /// <summary>
            /// The function grabs a new image, rectifies it and computes the
            ///disparity map and optionally the depth map.
            ///The grabbing function is typically called in the main loop.
            /// </summary>
            /// <param name="sensingMode">defines the type of disparity map, more info : SENSING_MODE definition</param>
            /// <returns>the function returns false if no problem was encountered,
            /// true otherwise.</returns>
            public int Grab(SENSING_MODE sensingMode = SENSING_MODE.STANDARD)
            {
                AssertCameraIsReady();
                return dllz_grab((int)sensingMode, Convert.ToInt32(true), Convert.ToInt32(true));
            }

            /// <summary>
            ///  The reset function can be called at any time AFTER the Init function has been called.
            ///  It will reset and calculate again correction for misalignment, convergence and color mismatch.
            ///  It can be called after changing camera parameters without needing to restart your executable.
            /// </summary>
            ///
            /// <returns>ERRCODE : error boolean value : the function returns false if no problem was encountered,
            ///true otherwise.
            ///if no problem was encountered, the camera will use new parameters. Otherwise, it will be the old ones
            ///</returns>
            public bool ResetSelfCalibration()
            {
                AssertCameraIsReady();
                return dllz_reset_self_calibration();
            }

            public ERRCODE EnableRecording(string videoFileName, SVO_COMPRESSION_MODE compressionMode = SVO_COMPRESSION_MODE.LOSSLESS_BASED)
            {
                AssertCameraIsReady();
                return (ERRCODE)dllz_enable_recording(StringUtf8ToByte(videoFileName), (int)compressionMode);
            //int dllz_enable_recording(byte[] video_filename, int compresssionMode);
            }

            public Recording_state Record()
            {
                Recording_state state = new Recording_state();
                dllz_record(ref state);
                return state;
            }

            /// <summary>
            /// Stops the recording and closes the file.
            /// </summary>
            /// <returns></returns>
            public bool StopRecording()
            {
                AssertCameraIsReady();
                return dllz_stop_recording();
            }

            /// <summary>
            /// Useful if you use the camera upside down, this will flip the images so you can get the images in a normal way.
            /// </summary>
            /// <param name="flip"></param>
            public void SetFlip(bool flip)
            {
                AssertCameraIsReady();
                dllz_set_flip(flip);
            }

            /// <summary>
            /// Set a new frame rate for the camera, or the closest avaliable frame rate.
            /// </summary>
            /// <param name="fps"></param>
            /// <returns></returns>
            public bool SetFPS(int fps)
            {
                AssertCameraIsReady();
                return dllz_set_fps(fps);
            }

            /// <summary>
            /// Sets the position of the SVO file to a desired frame.
            /// </summary>
            /// <param name="frame"> the number of the desired frame to be decoded.</param>
            /// <returns></returns>
            public bool SetSVOPosition(int frame)
            {
                AssertCameraIsReady();
                return dllz_set_svo_position(frame);
            }

            /// <summary>
            /// Gets the current confidence threshold value for the disparity map (and by extension the depth map).
            /// </summary>
            /// <returns>current filtering value between 0 and 100.</returns>
            public int GetConfidenceThreshold()
            {
                AssertCameraIsReady();
                return dllz_get_confidence_threshold();
            }
            /// <summary>
            /// Get the timestamp at the time the frame has been extracted from USB stream. (should be called after a grab())
            /// </summary>
            /// <returns>Current Timestamp in ns. -1 is not available(SVO file without compression). 
            /// Note that new SVO file from SDK 1.0.0 (with compression) contains the camera timestamp for each frame.</returns>
            public ulong GetCameraTimeStamp()
            {
                AssertCameraIsReady();
                return dllz_get_camera_timestamp();
            }

            /// <summary>
            /// return the state of the current tracked frame
            /// </summary>
            /// <returns></returns>
            public TRACKING_FRAME_STATE GetTrackingFrameState()
            {
                AssertCameraIsReady();
                return (TRACKING_FRAME_STATE)dllz_get_tracking_frame_state();
            }

            /// <summary>
            /// Get the number of frames in the SVO file.
            /// </summary>
            /// <returns>SVO Style Only : the total number of frames in the SVO file(-1 if the SDK is not reading a SVO)</returns>
            public int GetSVONumberOfFrames()
            {
                AssertCameraIsReady();
                return dllz_get_svo_number_of_frames();
            }

            /// <summary>
            /// Get the closest measurable distance by the camera, according to the camera and the depth map parameters.
            /// </summary>
            /// <returns></returns>
            public float GetClosestDepthValue()
            {
                AssertCameraIsReady();
                return dllz_get_closest_depth_value();
            }

            /// <summary>
            /// save the area learning information in a file.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool SaveAreaLearningDB(string path)
            {
                AssertCameraIsReady();
                return dllz_save_area_learning_db(StringUtf8ToByte(path));
            }

            /// <summary>
            /// Initialize and Start the tracking functions
            /// </summary>
            /// <param name="position">position of the first camera, used as reference. By default it should be identity.</param>
            /// <param name="enableAreaLearning">define if the relocalization is enable or not.</param>
            /// <param name="areaDBpath"> define if and where a relocalization database from a previous run on the same scene has to be loaded.</param>
            /// <returns></returns>
            public bool EnableTracking(float[] position, bool enableAreaLearning = false, string areaDBpath = "")
            {
                AssertCameraIsReady();
                return dllz_enable_tracking(position, enableAreaLearning, areaDBpath);
            }

            /// <summary>
            /// stop the tracker, if you want to restart, call enableTracking()
            /// </summary>
            public void StopTracking()
            {
                AssertCameraIsReady();
                dllz_stop_tracking();
            }

            /*
           * Register a texture to the base, to not be destoyed and be able to be used again.
           * */
            private void RegisterTexture(Texture2D m_Texture, int type, int mode)
            {
                TextureRequested t = new TextureRequested();
                t.type = type;
                t.option = mode;
                texturesRequested.Add(t);
                textures[type].Add(mode, m_Texture);
            }

            /// <summary>
            /// Create or retrieve a texture of type View (from Camera::getView() of the C++ ZED SDK)
            /// </summary>
            /// <param name="mode">The view mode</param>
            /// <returns></returns>
            public Texture2D CreateTextureViewType(VIEW_MODE mode)
            {
                if (IsTextureExist((int)TYPE_VIEW.GET_VIEW, (int)mode))
                {
                    return textures[(int)TYPE_VIEW.GET_VIEW][(int)mode];
                }

                if (!cameraIsReady)
                    return null;

                Texture2D m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBA32, false);

                m_Texture.Apply();

                IntPtr idTexture = m_Texture.GetNativeTexturePtr();
                int error = dllz_register_texture_view_type((int)mode, idTexture);
                if (error != 0)
                {
                    Debug.Log("Error Cuda " + error);
                    // DestroyCamera();
                }
                if (!textures.ContainsKey((int)TYPE_VIEW.GET_VIEW))
                {
                    textures.Add((int)TYPE_VIEW.GET_VIEW, new Dictionary<int, Texture2D>());
                }
                RegisterTexture(m_Texture, (int)TYPE_VIEW.GET_VIEW, (int)mode);

                return m_Texture;
            }

            /// <summary>
            /// Create or retrieve a texture of type Image (from Camera::retrieveImage() of the C++ ZED SDK)
            /// </summary>
            /// <param name="mode"></param>
            /// <returns></returns>
            public Texture2D CreateTextureImageType(SIDE mode)
            {
                if (IsTextureExist((int)TYPE_VIEW.RETRIEVE_IMAGE, (int)mode))
                {
                    return textures[(int)TYPE_VIEW.RETRIEVE_IMAGE][(int)mode];
                }
                if (!cameraIsReady)
                    return null;

                Texture2D m_Texture;
                if (mode == ZEDCamera.SIDE.LEFT_GREY || mode == ZEDCamera.SIDE.RIGHT_GREY || mode == ZEDCamera.SIDE.LEFT_UNRECTIFIED_GREY || mode == ZEDCamera.SIDE.RIGHT_UNRECTIFIED_GREY)
                {
                    m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.Alpha8, false);
                }
                else
                {
                    m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBA32, false);
                }
                m_Texture.Apply();

                IntPtr idTexture = m_Texture.GetNativeTexturePtr();
                int error = dllz_register_texture_image_type((int)mode, idTexture);
                if (error != 0)
                {
                    Debug.Log("Error Cuda " + error);
                    //DestroyCamera();
                }
                if (!textures.ContainsKey((int)TYPE_VIEW.RETRIEVE_IMAGE))
                {
                    textures.Add((int)TYPE_VIEW.RETRIEVE_IMAGE, new Dictionary<int, Texture2D>());
                }
                RegisterTexture(m_Texture, (int)TYPE_VIEW.RETRIEVE_IMAGE, (int)mode);

                return m_Texture;
            }

            /// <summary>
            /// Create or retrieve a texture of type Measure (from Camera::retrieveMeasure() of the C++ ZED SDK)
            /// </summary>
            /// <param name="mode"></param>
            /// <returns></returns>
            public Texture2D CreateTextureMeasureType(MEASURE mode)
            {
                if (IsTextureExist((int)TYPE_VIEW.RETRIEVE_MEASURE, (int)mode))
                {
                    return textures[(int)TYPE_VIEW.RETRIEVE_MEASURE][(int)mode];
                }
                if (!cameraIsReady)
                    return null;

                Texture2D m_Texture;

                if (mode == MEASURE.XYZ)
                {
                    m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBAFloat, false);

                }
                else m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBA32, false);
                m_Texture.Apply();

                IntPtr idTexture = m_Texture.GetNativeTexturePtr();

                int error = dllz_register_texture_measure_type((int)mode, idTexture);

                if (error != 0)
                {
                    Debug.Log("Error Cuda " + error);
                    // DestroyCamera();
                }
                if (!textures.ContainsKey((int)TYPE_VIEW.RETRIEVE_MEASURE))
                {
                    textures.Add((int)TYPE_VIEW.RETRIEVE_MEASURE, new Dictionary<int, Texture2D>());
                }

                RegisterTexture(m_Texture, (int)TYPE_VIEW.RETRIEVE_MEASURE, (int)mode);

                return m_Texture;
            }

            /// <summary>
            /// Create or retrieve a texture of type Normalize Measure (from Camera::retrieveNormaliseMeasure() of the C++ ZED SDK)
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="min">defines the lower bound of the normalization, default : automatically found</param>
            /// <param name="max">defines the upper bound of the normalization, default : automatically found</param>
            /// <returns></returns>
            public Texture2D CreateTextureNormalizeMeasureType(MEASURE mode, float min = 0.0f, float max = 0.0f)
            {
                if (IsTextureExist((int)TYPE_VIEW.NORMALIZE_MEASURE, (int)mode))
                {
                    return textures[(int)TYPE_VIEW.NORMALIZE_MEASURE][(int)mode];
                }
                if (!cameraIsReady)
                    return null;

                Texture2D m_Texture;
                if (mode == MEASURE.XYZ)
                {
                    m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBAFloat, false);
                }
                else m_Texture = new Texture2D(WidthImage, HeightImage, TextureFormat.RGBA32, false);
                m_Texture.Apply();
                IntPtr idTexture = m_Texture.GetNativeTexturePtr();
                int error = dllz_register_texture_normalize_measure_type((int)mode, idTexture, min, max);
                if (error != 0)
                {
                    Debug.Log("Error Cuda " + error);
                    //DestroyCamera();
                }
                if (!textures.ContainsKey((int)TYPE_VIEW.NORMALIZE_MEASURE))
                {
                    textures.Add((int)TYPE_VIEW.NORMALIZE_MEASURE, new Dictionary<int, Texture2D>());
                }
                RegisterTexture(m_Texture, (int)TYPE_VIEW.NORMALIZE_MEASURE, (int)mode);

                return m_Texture;
            }

            /*
            * Destroy a texture and free the registers
            * 
            */
            private void DestroyTexture(int type, int option)
            {

                if (textures.ContainsKey(type) && textures[type].ContainsKey(option))
                {
                    textures[type][option] = null;
                    textures[type].Remove(option);
                    if (textures[type].Count == 0)
                    {
                        textures.Remove(type);
                    }
                }
            }

            /*
            * Destroy all textures
            * 
            */
            private void DestroyAllTexture()
            {
                if (cameraIsReady)
                {
                    foreach (TextureRequested t in texturesRequested)
                    {
                        DestroyTexture(t.type, t.option);
                    }
                    texturesRequested.Clear();
                }
            }

            /*
            * Destroy a texture created with CreateTextureViewType
            * 
            */
            private void DestroyTextureViewType(int option)
            {
                DestroyTexture((int)TYPE_VIEW.GET_VIEW, option);
            }

            /*
            * Destroy a texture created with CreateTextureImageType
            * 
            */
            private void DestroyTextureImageType(int option)
            {
                DestroyTexture((int)TYPE_VIEW.RETRIEVE_IMAGE, option);
            }

            /*
            * Destroy a texture created with CreateTextureMeasureType
            * 
            */
            private void DestroyTextureMeasureType(int option)
            {
                DestroyTexture((int)TYPE_VIEW.RETRIEVE_MEASURE, option);
            }

            /*
            * Destroy a texture created with CreateTextureNormalizeMeasureType
            * 
            */
            private void DestroyTextureNormalizeMeasureType(int option)
            {
                DestroyTexture((int)TYPE_VIEW.NORMALIZE_MEASURE, option);
            }

            /// <summary>
            /// Retrieves a texture previously created
            /// </summary>
            /// <param name="type"></param>
            /// <param name="mode"></param>
            /// <returns></returns>
            public Texture2D GetTexture(TYPE_VIEW type, int mode)
            {
                if (IsTextureExist((int)type, mode))
                {
                    return textures[(int)type][mode];
                }
                return null;
            }

            /*
             * Check if a texture exists
             * */
            private bool IsTextureExist(int type, int mode)
            {
                if (cameraIsReady)
                    return textures.ContainsKey((int)type) && textures[type].ContainsKey((int)mode);
                return false;
            }

            public int WidthImage
            {
                get
                {
                    return widthImage;
                }
            }
            public int HeightImage
            {
                get
                {
                    return heightImage;
                }
            }

            public bool CameraIsReady
            {
                get
                {
                    return cameraIsReady;
                }
            }

            public Matrix4x4 Projection
            {
                get
                {
                    return projection;
                }
            }

            public void SetConfidenceThreshold(int threshold)
            {
                AssertCameraIsReady();
                dllz_set_confidence_threshold(threshold);
            }

            public void SetDepthClampValue(float distanceMax)
            {
                AssertCameraIsReady();
                dllz_set_depth_clamp_value(distanceMax);
            }

            public float GetFPS()
            {
                return dllz_get_fps();
            }

            /// <summary>
            /// Get the parameters of the Camera
            /// </summary>
            /// <returns></returns>
            public StereoParameters GetParameters()
            {
                StereoParameters param = new StereoParameters();
                param.leftCam.disto = new double[5];
                param.rightCam.disto = new double[5];
                //if (!cameraIsReady) return null;

                float[] v = new float[30];
                Marshal.Copy(dllz_get_parameters(), v, 0, 30);
                param.baseline = v[0];
                param.Ty = v[1];
                param.Tz = v[2];
                param.convergence = v[3];
                param.Rx = v[4];
                param.Rz = v[5];
                param.leftCam.fx = v[6];
                param.leftCam.fy = v[7];
                param.leftCam.cx = v[8];
                param.leftCam.cy = v[9];
                param.leftCam.vFOV = v[10];
                param.leftCam.hFOV = v[11];
                param.leftCam.dFOV = v[12];

                for (int i = 0; i < 5; ++i)
                {
                    param.leftCam.disto[i] = v[13 + i];
                }
                param.rightCam.fx = v[18];
                param.rightCam.fy = v[19];
                param.rightCam.cx = v[20];
                param.rightCam.cy = v[21];
                param.rightCam.vFOV = v[22];
                param.rightCam.hFOV = v[23];
                param.rightCam.dFOV = v[24];

                for (int i = 0; i < 5; ++i)
                {
                    param.rightCam.disto[i] = v[25 + i];
                }
                return param;
            }

            /// <summary>
            /// Gets the ZED Serial Number
            /// </summary>
            /// <returns>Returns the ZED Serial Number (as uint) (Live or SVO).</returns>
            public int GetZEDSerial()
            {
                return dllz_get_zed_Serial();
            }

            /// <summary>
            /// Gets the zed firmware
            /// </summary>
            /// <returns></returns>
            public int GetZEDFirmware()
            {
                return dllz_get_zed_firmware();
            }

            public float GetFOV()
            {
                return Mathf.Atan(HeightImage / (GetParameters().leftCam.fy * 2.0f)) * 2.0f;
            }

            /// <summary>
            /// Gets the status calibration
            /// </summary>
            /// <returns></returns>
            public ZED_SELF_CALIBRATION_STATUS GetSelfCalibrationStatus()
            {
                return (ZED_SELF_CALIBRATION_STATUS)dllz_get_self_calibration_status();
            }

            /// <summary>
            /// Return a confidence metric of the tracking [0-100], 0 means that the tracking is lost
            /// </summary>
            /// <returns></returns>
            public float GetTrackingConfidence()
            {
                AssertCameraIsReady();
                return dllz_get_tracking_confidence();
            }

            /// <summary>
            ///  return the position of the camera and the current state of the Tracker
            /// </summary>
            /// <param name="position">the matrix containing the position of the camera</param>
            /// <param name="mat_type">define if the function return the path (the cumulate displacement of the camera) or juste the pose (the displacement from the previous position).</param>
            /// <returns></returns>
            public int GetPositionCamera(float[] position, MAT_TRACKING_TYPE mat_type = MAT_TRACKING_TYPE.POSE)
            {
                AssertCameraIsReady();
                return dllz_get_position_camera(position, (int)mat_type);
            }

            /// <summary>
            /// Set settings of the camera
            /// </summary>
            /// <param name="settings">The setting which will be changed</param>
            /// <param name="value">The value</param>
            /// <param name="usedefault">will set default (or automatic) value if set to true (value (int) will not be taken into account)</param>
            public void SetCameraSettings(ZEDCamera_settings settings, int value, bool usedefault = false)
            {
                dllz_set_camera_settings_value((int)settings, value, Convert.ToInt32(usedefault));
            }

            /// <summary>
            /// Get the value from a setting of the camera
            /// </summary>
            /// <param name="settings"></param>
            public int GetCameraSettings(ZEDCamera_settings settings)
            {
                return dllz_get_camera_settings_value((int)settings);
            }

            /// <summary>
            /// The function checks if ZED cameras are connected, can be called before instantiating a Camera object
            /// </summary>
            /// <remarks> On Windows, only one ZED is accessible so this function will return 1 even if multiple ZED are connected.</remarks>
            /// <returns>the number of connected ZED</returns>
            public static bool IsZedConnected()
            {
                return Convert.ToBoolean(dllz_is_zed_connected());
            }

            /// <summary>
            /// The function return the version of the currently installed ZED SDK
            /// </summary>
            /// <returns>ZED SDK version as a string with the following format : MAJOR.MINOR.PATCH</returns>
            public static string GetSDKVersion()
            {
                return PtrToStringUtf8(dllz_get_sdk_version());
            }

            private void AssertCameraIsReady()
            {
                 if (!cameraIsReady)
                    throw new Exception("Error init camera, no init called");
            }

            public void Render()
            {
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }
    } // namespace zed
} // namespace sl