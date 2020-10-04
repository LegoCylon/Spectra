using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(SaturationRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Saturation")]
    public class SaturationEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        #endregion
    }

    public class SaturationRenderer : PostProcessEffectRenderer<SaturationEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Saturation"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}