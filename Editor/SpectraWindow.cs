using System;
using System.Collections.Generic;
using System.Linq;
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
            public bool IsPostProcessEffect => string.IsNullOrEmpty(value: ReplaceTag);
            public bool IsReplacementShader => !string.IsNullOrEmpty(value: ReplaceTag);
            #endregion

            #region Fields
            public string ShaderName;
            public Action<SceneView> BeforeSceneViewGUI;
            public string ReplaceTag;
            #endregion
        }

        [RequireComponent(typeof(Camera)), ExecuteInEditMode, ImageEffectAllowedInSceneView]
        private class RenderHook : MonoBehaviour
        {
            #region Fields
            private Material _Material;
            #endregion

            private void OnDestroy() => DestroyMaterial();

            private void OnRenderImage(RenderTexture src, RenderTexture dest)
            {
                SpectraWindow window = Resources.FindObjectsOfTypeAll<SpectraWindow>().FirstOrDefault();

                AcquireMaterial(window: window);

                if (window != null &&
                    (Camera.current.cameraType & window._CameraMask) == Camera.current.cameraType &&
                    _Material != null)
                {
                    Graphics.Blit(source: src, dest: dest, mat: _Material);

                }
                else
                {
                    Graphics.Blit(source: src, dest: dest);
                }
            }

            private void AcquireMaterial (SpectraWindow window)
            {
                if (window == default)
                {
                    return;
                }

                bool isPostProcessEffect =
                    window._Shader != null && window._ActiveFilter != null && window._ActiveFilter.IsPostProcessEffect;
                if (_Material != null && (!isPostProcessEffect || _Material.shader != window._Shader))
                {
                    DestroyMaterial();
                }

                if (isPostProcessEffect && _Material == null)
                {
                    _Material = new Material(shader: window._Shader);
                }
            }

            private void DestroyMaterial ()
            {
                if (_Material != null)
                {
                    DestroyImmediate(obj: _Material);
                    _Material = null;
                }
            }
        }
        #endregion

        #region Constants
        private const string cReplaceTag = "RenderType";

        private static readonly List<Filter> sFilters = new List<Filter>
            {
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Albedo",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Achromatomaly",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Achromatopsia",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Deuteranomaly",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Deuteranopia",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Protanomaly",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Protanopia",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Tritanomaly",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Tritanopia",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Grayscale",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Sepia",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Red",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Green",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Blue",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Hue",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Saturation",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Posterize",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/CRT",
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Ghost",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Holodeck",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Occlusion",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Overdraw",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Transparent",
                    BeforeSceneViewGUI = sceneView => ClearCamera(sceneView: sceneView, clearColor: Color.black),
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Vertex Colors",
                    ReplaceTag = cReplaceTag,
                },
                new Filter
                {
                    ShaderName = "Hidden/Spectra/Vertex Normals",
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
                        label: filter.ShaderName.Replace(oldValue: "Hidden/Spectra/", newValue: string.Empty),
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
                bool wantHook = _ActiveFilter != null && _ActiveFilter.IsPostProcessEffect;
                RenderHook hook = mainCamera.GetComponent<RenderHook>();
                if (wantHook)
                {
                    if (!hook)
                    {
                        hook = mainCamera.gameObject.AddComponent<RenderHook>();
                        hook.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                    }
                }
                else
                {
                    DestroyImmediate(obj: hook);
                }

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
