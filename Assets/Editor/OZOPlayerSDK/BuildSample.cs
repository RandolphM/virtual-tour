using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildSample
{

	[MenuItem("File/Build/Build All")]
	static void BuildAll()
	{
		Debug.Log("Starting Build All");

		BuildAndroid();
		BuildAndroidDaydream();
		BuildAndroidGearVRController();
		BuildIOS();
		BuildVive();
		BuildRift();
	}


	[MenuItem("File/Build/Build Android")]
	static void BuildAndroid()
	{
		Debug.Log("Starting Android Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";

#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#pragma warning restore CS0618 // Type or member is obsolete

		var scenes = new[] { mainScene };
		BuildPipeline.BuildPlayer(scenes, "Build/OZOSamplePlayerUnity.apk", BuildTarget.Android, BuildOptions.None);
	}

	[MenuItem("File/Build/Build Android Daydream")]
	static void BuildAndroidDaydream()
	{
		Debug.Log("Starting Android Daydream Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";
		string mainSave = mainPath + "_DayDream.unity";

		UnityEngine.SceneManagement.Scene scn = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
		UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scn, true);
		var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene);

		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#pragma warning restore CS0618 // Type or member is obsolete

		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/GoogleVR/Prefabs/Controller/GvrControllerMain.prefab", typeof(GameObject));
		PrefabUtility.InstantiatePrefab(prefab);

		Debug.Log("Save project: " + mainSave);
		UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, mainSave);
		var scenes = new[] { mainSave };

		BuildPipeline.BuildPlayer(scenes, "Build/OZOSamplePlayerUnityDaydream.apk", BuildTarget.Android, BuildOptions.None);
	}

	[MenuItem("File/Build/Build Android GearVR Controller")]
	static void BuildAndroidGearVRController()
	{
		Debug.Log("Starting Android Gear VR Controller Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";
		string mainSave = mainPath + "_GVR_Controller.unity";

		UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene);

		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#pragma warning restore CS0618 // Type or member is obsolete

		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/OvrAvatar/Content/Prefabs/LocalAvatar.prefab", typeof(GameObject));
		PrefabUtility.InstantiatePrefab(prefab);

		Debug.Log("Save project: " + mainSave);
		UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, mainSave);
		var scenes = new[] { mainSave };

		BuildPipeline.BuildPlayer(scenes, "Build/OZOSamplePlayerUnityGearVRController.apk", BuildTarget.Android, BuildOptions.None);
	}

	[MenuItem("File/Build/Build iOS")]
	public static void BuildIOS()
	{
		Debug.Log("Starting iOS Build");
		var scenes = new[] { "Assets/Scenes/main.unity" };
#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
#pragma warning restore CS0618 // Type or member is obsolete
		BuildPipeline.BuildPlayer(scenes, "Build/iOS", BuildTarget.iOS, BuildOptions.None);
	}

	[MenuItem("File/Build/Build Vive")]

	static void BuildVive()
	{
		Debug.Log("Starting Vive Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";
		string mainSave = mainPath + "_VIVE.unity";

		var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene);

#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
#pragma warning restore CS0618 // Type or member is obsolete

		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/SteamVR/Prefabs/[CameraRig].prefab", typeof(GameObject));
		if(prefab)
		{
			GameObject o = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			Transform head = o.transform.Find("Camera (head)");
			if (head != null)
			{
				Transform eye = head.Find("Camera (eye)");
				if (eye != null)
				{
					eye.GetComponent<Camera>().enabled = false;
				}
			}
			var objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			GameObject mainCam = null;
			foreach (var obj in objs)
			{
				if (obj.name == "CameraRoot")
				{
					mainCam = obj.transform.GetChild(0).gameObject;
					break;
				}
			}

			Transform right = o.transform.Find("Controller (right)");
			if (right != null)
			{
				Component[] components = right.GetComponents<Component>();
				foreach (var comp in components)
				{
					var prop = comp.GetType().GetField("origin");
					if (prop != null)
					{
						prop.SetValue(comp, mainCam.transform.parent);
						break;
					}
				}
			}
			//TODO: find a way to do this! (or wait for SteamVR update)
			Type type = Type.GetType("SteamVR_UpdatePoses");
			if (type != null)
			{
				mainCam.AddComponent(type);
			}
		}

		UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, mainSave);

		var scenes = new[] { mainSave };
		BuildPipeline.BuildPlayer(scenes, "Build/VIVE/OZOSamplePlayerUnityVive.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
	}


	[MenuItem("File/Build/Build Rift")]
	static void BuildRift()
	{
		Debug.Log("Starting Rift Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";
		string mainSave = mainPath + "_RIFT.unity";

		var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene);

#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
#pragma warning restore CS0618 // Type or member is obsolete

		GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/OvrAvatar/Content/Prefabs/LocalAvatar.prefab", typeof(GameObject));
		PrefabUtility.InstantiatePrefab(prefab);

		UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, mainSave);
		var scenes = new[] { mainSave };
		BuildPipeline.BuildPlayer(scenes, "Build/RIFT/OZOSamplePlayerUnityRift.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
	}

	[MenuItem("File/Build/Build UWP")]
	static void BuildUWP()
	{
		Debug.Log("Starting UWP Build");
		string mainPath = "Assets/Scenes/main";
		string mainScene = mainPath + ".unity";
		string mainSave = mainPath + "_UWP.unity";

		var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(mainScene);

#pragma warning disable CS0618 // Type or member is obsolete
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
#pragma warning restore CS0618 // Type or member is obsolete

		UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, mainSave);

		var scenes = new[] { mainSave };
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = scenes;
		buildPlayerOptions.locationPathName = "Build/UWP/OZOSamplePlayerUnityUWP.exe";
		buildPlayerOptions.target = BuildTarget.WSAPlayer;
		buildPlayerOptions.targetGroup = BuildTargetGroup.WSA;
		buildPlayerOptions.options = BuildOptions.None;
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}

}


