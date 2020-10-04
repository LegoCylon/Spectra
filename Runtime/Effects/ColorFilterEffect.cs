using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(ColorFilterRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Color Filter")]
    public class ColorFilterEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        public ColorParameter RedMix = new ColorParameter { value = Color.red };
        public ColorParameter GreenMix = new ColorParameter { value = Color.green };
        public ColorParameter BlueMix = new ColorParameter { value = Color.blue };

        #endregion
    }

    public class ColorFilterRenderer : PostProcessEffectRenderer<ColorFilterEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Color Filter"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            sheet.properties.SetColor(name: "_RedMix", value: settings.RedMix);
            sheet.properties.SetColor(name: "_GreenMix", value: settings.GreenMix);
            sheet.properties.SetColor(name: "_BlueMix", value: settings.BlueMix);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}