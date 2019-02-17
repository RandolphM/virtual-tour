
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(OZOPlayer))]
public class OZOPlayerEditor : Editor
{
    private bool clipFoldout = true;
    private int MAX_CLIP_AREAS = 32;
	MonoScript script;

	public override void OnInspectorGUI()
    {
        OZOPlayer player = (OZOPlayer)target;

		EditorGUI.BeginDisabledGroup(true);
		MonoScript ozoplayer = MonoScript.FromMonoBehaviour(player);
		MonoScript script = EditorGUILayout.ObjectField("Script", ozoplayer, typeof(MonoScript), false) as MonoScript;
		script.GetType();
		EditorGUI.EndDisabledGroup();

		Undo.RecordObject(player, "OZO Player settings"); //enable undo for everything

        {
            string licenseId = EditorGUILayout.TextField("OZO Player SDK License ID", player.licenseId);
            if (licenseId != player.licenseId)
            {
                player.licenseId = licenseId;
            }
            EditorGUILayout.Space();
        }
        {
            //Camera Settings
            EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            Color clearColor = EditorGUILayout.ColorField("OZO Render Clear Color", player.OZORendererClearColor);
            if (clearColor != player.OZORendererClearColor)
            {
                player.OZORendererClearColor = clearColor;
                player.SetClearColor(clearColor); //the SDK
            }
            float cameraDepth = EditorGUILayout.FloatField("OZO Camera Depth", player.OZOCameraDepth);
            if (cameraDepth != player.OZOCameraDepth)
            {
                player.OZOCameraDepth = cameraDepth;
                if (null != player.OZOViewCameras)
                {
                    foreach (Camera cam in player.OZOViewCameras)
                    {
                        cam.depth = cameraDepth;
                    }
                }
            }
            Vector2 cameraClipPlanes = EditorGUILayout.Vector2Field("OZO Camera Clip Planes", player.OZOCameraClipPlanes);
            if (cameraClipPlanes != player.OZOCameraClipPlanes)
            {
                player.OZOCameraClipPlanes = cameraClipPlanes;
                if (null != player.OZOViewCameras)
                {
                    foreach (Camera cam in player.OZOViewCameras)
                    {
                        cam.nearClipPlane = cameraClipPlanes.x;
                        cam.farClipPlane = cameraClipPlanes.x;
                    }
                }
            }
            CameraClearFlags flags = (CameraClearFlags)EditorGUILayout.EnumPopup("OZO Camera Clear Flags", player.OZOCameraClearFlags);
            if (flags != player.OZOCameraClearFlags)
            {
                player.OZOCameraClearFlags = flags;
                if (null != player.OZOViewCameras)
                {
                    foreach (Camera cam in player.OZOViewCameras)
                    {
                        cam.clearFlags = flags;
                    }
                }
            }
            Undo.RecordObject(player.OZOCameraMaterial, "Changed Material Settings");
            Material mat = player.OZOCameraMaterial;
            player.OZOCameraMaterial = (Material)EditorGUILayout.ObjectField("OZO Camera Material", player.OZOCameraMaterial, typeof(Material), true);
            if(mat!= player.OZOCameraMaterial)
            {
                if (null != player.OZOViewRenderer) //runtime params
                {
                    foreach (OZORender r in player.OZOViewRenderer)
                    {
                        r.material = null;
                        r.material = new Material(player.OZOCameraMaterial);
                        r.material.SetTexture("_MainTex", r.renderTexture);
                        r.material.SetTexture("_DepthTex", r.renderTextureDepth);
                    }
                }
            }

            EditorGUILayout.BeginVertical();
            if (null != player.OZOCameraMaterial)
            {
                Shader sh = player.OZOCameraMaterial.shader;
                if (sh.name != "OZO/OZODefault")
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(The following setting changes will be stored also in runtime)", EditorStyles.miniLabel);

                    for (int i = 0; i < ShaderUtil.GetPropertyCount(sh); ++i)
                    {
                        ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(sh, i);
                        if (ShaderUtil.ShaderPropertyType.Range == type)
                        {
                            string name = ShaderUtil.GetPropertyName(sh, i);
                            float value = player.OZOCameraMaterial.GetFloat(name);
                            float selValue = EditorGUILayout.Slider(name, value, ShaderUtil.GetRangeLimits(sh, i, 1), ShaderUtil.GetRangeLimits(sh, i, 2));
                            if (value != selValue)
                            {
                                player.OZOCameraMaterial.SetFloat(name, selValue);
                                if (null != player.OZOViewRenderer) //runtime params
                                {
                                    foreach (OZORender r in player.OZOViewRenderer)
                                    {
                                        r.material.SetFloat(name, selValue);
                                    }
                                }
                            }
                        }
                        else if (ShaderUtil.ShaderPropertyType.Color == type)
                        {
                            string name = ShaderUtil.GetPropertyName(sh, i);
                            Color color = player.OZOCameraMaterial.GetColor(name);
                            Color selColor = EditorGUILayout.ColorField(name, color);

                            if (color != selColor)
                            {
                                player.OZOCameraMaterial.SetColor(name, selColor);
                                if (null != player.OZOViewRenderer) //runtime params
                                {
                                    foreach (OZORender r in player.OZOViewRenderer)
                                    {
                                        r.material.SetColor(name, selColor);
                                    }
                                }
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            //Clip areas
            SerializedProperty clipProperty = serializedObject.FindProperty("clipAreas");
            if (null != clipProperty)
            {
                clipFoldout = EditorGUILayout.Foldout(clipFoldout, "Clip Areas: " + player.clipAreas.Count);
                if (clipFoldout)
                {
                    EditorGUI.indentLevel++;

                    bool modified = false;
                    EditorGUILayout.LabelField("(Clip Areas  define the areas of the video that will be rendered, if none the whole video will be rendered)", EditorStyles.miniLabel);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Areas", "" + player.clipAreas.Count);
                    //buttons
                    GUI.enabled = player.clipAreas.Count < MAX_CLIP_AREAS;
                    if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
                    {
                        OZO.ClipArea area = new OZO.ClipArea();
                        area.centerLongitude = 0.0f;
                        area.centerLatitude = 0.0f;
                        area.spanLongitude = 90.0f;
                        area.spanLatitude = 90.0f;
                        area.opacity = 1.0f;
                        player.clipAreas.Add(area);
                        modified = true;
                    }
                    GUI.enabled = player.clipAreas.Count > 0;
                    if ( GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        if (0 < player.clipAreas.Count)
                        {
                            player.clipAreas.RemoveAt(player.clipAreas.Count - 1);
                            modified = true;
                        }
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    //draw property fields
                    int numAreas = clipProperty.arraySize;
                    for (int areaIdx = 0; areaIdx < numAreas; ++areaIdx)
                    {
                        SerializedProperty areaProp = clipProperty.GetArrayElementAtIndex(areaIdx);
                        //EditorGUILayout.PropertyField(areaProp);
                        if (areaProp.hasChildren)
                        {
                            EditorGUILayout.LabelField("Area #" + areaIdx, EditorStyles.boldLabel);
                            string root = areaProp.propertyPath;
                            EditorGUI.indentLevel++;
                            SerializedProperty it = areaProp.Copy();
                            bool hasMore = it.Next(true);
                            while (hasMore)
                            {
                                //only children
                                if (!it.propertyPath.Contains(root))
                                {
                                    break;
                                }
                                string type = it.type;
                                if ("float" == type)
                                {
                                    EditorGUILayout.PropertyField(it, EditorStyles.miniFont);
                                    if (player.clipAreas.Count > areaIdx)
                                    {
                                        var prop = typeof(OZO.ClipArea).GetField(it.name);
                                        float val = (float)prop.GetValue(player.clipAreas[areaIdx]);
                                        if(val != it.floatValue)
                                        {
                                            prop.SetValue(player.clipAreas[areaIdx], it.floatValue);
                                            modified = true;
                                        }
                                    }
                                }
                                it.Next(false);
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                    EditorGUI.indentLevel--;
                    if(modified)
                    {
                        player.SetClipAreas(player.clipAreas.ToArray());
                    }
                }
            }
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
