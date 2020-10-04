using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(BrightnessRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Brightness")]
    public class BrightnessEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        [Range(min: 0f, max: 100f), Tooltip(tooltip: "Brightness.")]
        public FloatParameter Brightness = new FloatParameter { value = 0.0f };
        #endregion
    }

    public class BrightnessRenderer : PostProcessEffectRenderer<BrightnessEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Brightness"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            sheet.properties.SetFloat(name: "_Brightness", value: settings.Brightness);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}