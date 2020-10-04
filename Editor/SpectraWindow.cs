using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Spectra.Editor
{
    public class SpectraWindow : EditorWindow
    {
        #region Types
        private class Filter
        {
            #region Properties
            public bool IsReplacementShader => !string.IsNullOrEmpty(value: ReplaceTag);
            #endregion

            #region Fields
            public string ShaderName;
            public Action<SceneView> BeforeSceneViewGUI;
            public string ReplaceTag;
            #endregion
        }
        #endregion

        #region Constants
        private const string cReplaceTag = "RenderType";

        private static readonly List<Filter> sFilters = new List<Filter>
            {
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Albedo",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Ghost",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Holodeck",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Occlusion",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Overdraw",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Transparent",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Vertex Colors",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Spectra/Replacement/Vertex Normals",
                    ReplaceTag = cReplaceTag,
                },
            };

        private static readonly PropertyInfo sCameraRectProperty = typeof(SceneView).GetProperty(
            name: "cameraRect",
            bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo sGetMainCameraMethod = typeof(SceneView).GetMethod(
            name: "GetMainCamera",
            bindingAttr: BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo sRepaintAllPlayModeViewMethod = typeof(EditorWindow).Assembly
            .GetType("UnityEditor.PlayModeView")
            .GetMethod(name: "RepaintAll", bindingAttr: BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly SceneView.CameraMode sShadedSceneViewCameraMode = new SceneView.CameraMode()
        {
            drawMode = DrawCameraMode.Textured,
            name = "Shaded",
            section = "Shading Mode",
        };

        [SerializeField] private string _ActiveFilterShaderName;
        [SerializeField] private CameraType _CameraMask = CameraType.SceneView;
        [SerializeField] private Vector2 _ScrollPos;

        private Filter _ActiveFilter;
        private Shader _Shader;

        #endregion

        protected void OnEnable ()
        {
            SceneView.beforeSceneGui += OnBeforeSceneGUI;

            ApplyFilter(filter: sFilters.Find(match: filter =>
                string.Equals(a: _ActiveFilterShaderName, b: filter.ShaderName)));
        }

        protected void OnDisable ()
        {
            SceneView.beforeSceneGui -= OnBeforeSceneGUI;
        }

        protected void OnGUI ()
        {
            using (EditorGUILayout.ScrollViewScope scrollViewScope =
                new EditorGUILayout.ScrollViewScope(scrollPosition: _ScrollPos))
            {
                _ScrollPos = scrollViewScope.scrollPosition;

                bool changed = false;

                CameraType wasCameraMask = _CameraMask;
                _CameraMask = (CameraType)EditorGUILayout.EnumFlagsField(enumValue: _CameraMask, label: "Camera Types");
                changed |= _CameraMask != wasCameraMask;

                Filter desiredFilter = _ActiveFilter;
                foreach (Filter filter in sFilters)
                {
                    bool wasToggled = _ActiveFilter == filter;
                    bool nowToggled = EditorGUILayout.ToggleLeft(
                        label: filter.ShaderName.Replace(oldValue: "Spectra/", newValue: string.Empty),
                        value: wasToggled);
                    if (wasToggled != nowToggled)
                    {
                        changed = true;
                        desiredFilter = nowToggled ? filter : null;
                    }
                }

                if (changed)
                {
                    ApplyFilter(filter: desiredFilter);
                }
            }
        }

        [MenuItem("Window/Rendering/Spectra")]
        private static SpectraWindow GetWindow () => GetWindow<SpectraWindow>(title: "Spectra");

        private static Rect ClearCamera (SceneView sceneView, Color clearColor)
        {
            Rect cameraRect = GetCameraRect(sceneView: sceneView);

            Rect groupSpaceCameraRect = new Rect(x: 0, y: 0, width: cameraRect.width, height: cameraRect.height);
            Rect groupSpaceCameraRectInPixels = EditorGUIUtility.PointsToPixels(rect: groupSpaceCameraRect);

            CameraClearFlags oldClearFlags = sceneView.camera.clearFlags;
            Color oldClearColor = sceneView.camera.backgroundColor;

            sceneView.camera.clearFlags = CameraClearFlags.SolidColor;
            sceneView.camera.backgroundColor = clearColor;
            Handles.ClearCamera(position: groupSpaceCameraRectInPixels, camera: sceneView.camera);
            sceneView.camera.clearFlags = oldClearFlags;
            sceneView.camera.backgroundColor = oldClearColor;

            return cameraRect;
        }

        private static Rect GetCameraRect (SceneView sceneView) => (Rect)sCameraRectProperty.GetValue(obj: sceneView);

        private static Camera GetMainCamera () =>
            sGetMainCameraMethod.Invoke(obj: null, parameters: Array.Empty<object>()) as Camera;

        private void ApplyFilter (Filter filter)
        {
            _ActiveFilter = filter;
            _ActiveFilterShaderName = filter != null ? filter.ShaderName : null;

            _Shader = _ActiveFilter != null ? Shader.Find(name: _ActiveFilter.ShaderName) : null;

            GetShaderReplacement(filter: _ActiveFilter, shader: out Shader replacementShader,
                replacementTag: out string replacementTag);

            if (GetMainCamera() is Camera mainCamera)
            {
                if ((_CameraMask & CameraType.Game) == CameraType.Game)
                {
                    mainCamera.SetReplacementShader(shader: replacementShader, replacementTag: replacementTag);
                }
                else
                {
                    mainCamera.SetReplacementShader(shader: null, replacementTag: string.Empty);
                }
            }

            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                if (_ActiveFilter != null)
                {
                    sceneView.cameraMode = sShadedSceneViewCameraMode;
                }

                if ((_CameraMask & CameraType.SceneView) == CameraType.SceneView)
                {
                    sceneView.SetSceneViewShaderReplace(shader: replacementShader, replaceString: replacementTag);
                }
                else
                {
                    sceneView.SetSceneViewShaderReplace(shader: null, replaceString: string.Empty);
                }

                sceneView.Repaint();
            }

            sRepaintAllPlayModeViewMethod.Invoke(obj: null, parameters: Array.Empty<object>());
        }

        private void GetShaderReplacement(Filter filter, out Shader shader, out string replacementTag)
        {
            if (filter != null && filter.IsReplacementShader)
            {
                shader = _Shader;
                replacementTag = filter.ReplaceTag;
            }
            else
            {
                shader = null;
                replacementTag = string.Empty;
            }
        }

        private void OnBeforeSceneGUI (SceneView sceneView)
        {
            if (_ActiveFilter != null && _ActiveFilter.BeforeSceneViewGUI != null)
            {
                _ActiveFilter.BeforeSceneViewGUI(obj: sceneView);
            }
        }
    }
}
