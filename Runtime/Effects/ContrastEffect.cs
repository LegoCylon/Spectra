using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(ContrastRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Contrast")]
    public class ContrastEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        [Range(min: -10f, max: 10f), Tooltip(tooltip: "Contrast.")]
        public FloatParameter Contrast = new FloatParameter { value = 0.0f };
        #endregion
    }

    public class ContrastRenderer : PostProcessEffectRenderer<ContrastEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Contrast"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            sheet.properties.SetFloat(name: "_Contrast", value: settings.Contrast);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}