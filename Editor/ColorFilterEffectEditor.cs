using System.Collections.Generic;
using System.Linq;
using Spectra.Runtime.Effects;
using UnityEditor;
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;

namespace Spectra.Editor
{
    [PostProcessEditor(typeof(ColorFilterEffect))]
    public sealed class ColorFilterEffectEditor : PostProcessEffectEditor<ColorFilterEffect>
    {
        #region Types
        private struct NamedMix
        {
            public Color RedMix;
            public Color GreenMix;
            public Color BlueMix;
        }
        #endregion

        #region Constants
// Some of the following color values are from https://github.com/PlanetCentauri/ColorblindFilter/blob/master/Code
        private static readonly SortedDictionary<string, NamedMix> sMixesByName =
            new SortedDictionary<string, NamedMix>
            {
                {
                    "Achromatomaly",
                    new NamedMix
                    {
                        RedMix = new Color(0.618f, 0.320f, 0.062f),
                        GreenMix = new Color(0.163f, 0.775f, 0.062f),
                        BlueMix = new Color(0.163f, 0.320f, 0.516f),
                    }
                },
                {
                    "Achromatopsia",
                    new NamedMix
                    {
                        RedMix = new Color(0.299f, 0.587f, 0.114f),
                        GreenMix = new Color(0.299f, 0.587f, 0.114f),
                        BlueMix = new Color(0.299f, 0.587f, 0.114f),
                    }
                },
                {
                    "Amber Monochrome",
                    new NamedMix
                    {
                        RedMix = new Color(0.213f, 0.715f, 0.072f),
                        GreenMix = new Color(0.213f, 0.715f, 0.072f),
                        BlueMix = new Color(0.000f, 0.000f, 0.000f),
                    }
                },
                {
                    "Blue Only",
                    new NamedMix
                    {
                        RedMix = new Color(0.000f, 0.000f, 0.000f),
                        GreenMix = new Color(0.000f, 0.000f, 0.000f),
                        BlueMix = new Color(0.000f, 0.000f, 1.000f),
                    }
                },
                {
                    "Blueness",
                    new NamedMix
                    {
                        RedMix = new Color(0.000f, 0.000f, 1.000f),
                        GreenMix = new Color(0.000f, 0.000f, 1.000f),
                        BlueMix = new Color(0.000f, 0.000f, 1.000f),
                    }
                },
                {
                    "Deuteranomaly",
                    new NamedMix
                    {
                        RedMix = new Color(0.800f, 0.200f, 0.000f),
                        GreenMix = new Color(0.258f, 0.742f, 0.000f),
                        BlueMix = new Color(0.000f, 0.142f, 0.858f),
                    }
                },
                {
                    "Deuteranopia",
                    new NamedMix
                    {
                        RedMix = new Color(0.625f, 0.375f, 0.000f),
                        GreenMix = new Color(0.700f, 0.300f, 0.000f),
                        BlueMix = new Color(0.000f, 0.300f, 0.700f),
                    }
                },
                {
                    "Grayscale",
                    new NamedMix
                    {
                        RedMix = new Color(0.213f, 0.715f, 0.072f),
                        GreenMix = new Color(0.213f, 0.715f, 0.072f),
                        BlueMix = new Color(0.213f, 0.715f, 0.072f),
                    }
                },
                {
                    "Green Monochrome",
                    new NamedMix
                    {
                        RedMix = new Color(0.000f, 0.000f, 0.000f),
                        GreenMix = new Color(0.213f, 0.715f, 0.072f),
                        BlueMix = new Color(0.000f, 0.000f, 0.000f),
                    }
                },
                {
                    "Green Only",
                    new NamedMix
                    {
                        RedMix = new Color(0.000f, 0.000f, 0.000f),
                        GreenMix = new Color(0.000f, 1.000f, 0.000f),
                        BlueMix = new Color(0.000f, 0.000f, 0.000f),
                    }
                },
                {
                    "Greenness",
                    new NamedMix
                    {
                        RedMix = new Color(0.000f, 1.000f, 0.000f),
                        GreenMix = new Color(0.000f, 1.000f, 0.000f),
                        BlueMix = new Color(0.000f, 1.000f, 0.000f),
                    }
                },
                {
                    "Protanomaly",
                    new NamedMix
                    {
                        RedMix = new Color(0.817f, 0.183f, 0.000f),
                        GreenMix = new Color(0.333f, 0.667f, 0.000f),
                        BlueMix = new Color(0.000f, 0.125f, 0.875f),
                    }
                },
                {
                    "Protanopia",
                    new NamedMix
                    {
                        RedMix = new Color(0.567f, 0.433f, 0.0f),
                        GreenMix = new Color(0.558f, 0.442f, 0.000f),
                        BlueMix = new Color(0.000f, 0.242f, 0.758f),
                    }
                },
                {
                    "Red Only",
                    new NamedMix
                    {
                        RedMix = new Color(1.000f, 0.000f, 0.000f),
                        GreenMix = new Color(0.000f, 0.000f, 0.000f),
                        BlueMix = new Color(0.000f, 0.000f, 0.000f),
                    }
                },
                {
                    "Redness",
                    new NamedMix
                    {
                        RedMix = new Color(1.000f, 0.000f, 0.000f),
                        GreenMix = new Color(1.000f, 0.000f, 0.000f),
                        BlueMix = new Color(1.000f, 0.000f, 0.000f),
                    }
                },
                {
                    "Sepia",
                    new NamedMix
                    {
                        RedMix = new Color(0.393f, 0.769f, 0.189f),
                        GreenMix = new Color(0.349f, 0.686f, 0.168f),
                        BlueMix = new Color(0.272f, 0.534f, 0.131f),
                    }
                },
                {
                    "Tritanomaly",
                    new NamedMix
                    {
                        RedMix = new Color(0.967f, 0.033f, 0.000f),
                        GreenMix = new Color(0.000f, 0.733f, 0.267f),
                        BlueMix = new Color(0.000f, 0.183f, 0.817f),
                    }
                },
                {
                    "Tritanopia",
                    new NamedMix
                    {
                        RedMix = new Color(0.950f, 0.050f, 0.00f),
                        GreenMix = new Color(0.000f, 0.433f, 0.567f),
                        BlueMix = new Color(0.000f, 0.475f, 0.525f),
                    }
                },
            };
        private static readonly string[] sMixNames = sMixesByName.Keys.ToArray();
        #endregion

        #region Fields
        private SerializedParameterOverride _Blend;
        private SerializedParameterOverride _RedMix;
        private SerializedParameterOverride _GreenMix;
        private SerializedParameterOverride _BlueMix;
        private int _SelectedMixIndex = -1;
        #endregion

        public override void OnEnable()
        {
            _Blend = FindParameterOverride(expr: x => x.Blend);
            _RedMix = FindParameterOverride(expr: x => x.RedMix);
            _GreenMix = FindParameterOverride(expr: x => x.GreenMix);
            _BlueMix = FindParameterOverride(expr: x => x.BlueMix);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(property: _Blend);
            PropertyField(property: _RedMix);
            PropertyField(property: _GreenMix);
            PropertyField(property: _BlueMix);

            _SelectedMixIndex = EditorGUILayout.Popup(selectedIndex: _SelectedMixIndex, displayedOptions: sMixNames,
                label: "Built-in Mix");
            using (new EditorGUI.DisabledScope(disabled: _SelectedMixIndex < 0 || _SelectedMixIndex >= sMixNames.Length))
            {
                if (GUILayout.Button("Apply Mix"))
                {
                    NamedMix namedMix = sMixesByName[key: sMixNames[_SelectedMixIndex]];
                    _RedMix.value.colorValue = namedMix.RedMix;
                    _GreenMix.value.colorValue = namedMix.GreenMix;
                    _BlueMix.value.colorValue = namedMix.BlueMix;
                }
            }
        }
    }
}