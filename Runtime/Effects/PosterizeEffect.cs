using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(PosterizeRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Posterize")]
    public class PosterizeEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        [Range(min: 0f, max: 65535f), Tooltip(tooltip: "Color count.")]
        public IntParameter ColorCount = new IntParameter { value = 256 };
        #endregion
    }

    public class PosterizeRenderer : PostProcessEffectRenderer<PosterizeEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Posterize"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            sheet.properties.SetFloat(name: "_ColorCount", value: settings.ColorCount);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}