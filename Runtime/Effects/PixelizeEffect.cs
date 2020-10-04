using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Spectra.Runtime.Effects
{
    [Serializable]
    [PostProcess(renderer: typeof(PixelizeRenderer),
        eventType: PostProcessEvent.AfterStack,
        menuItem: "Spectra/Pixelize")]
    public class PixelizeEffect : PostProcessEffectSettings
    {
        #region Fields
        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Filter intensity.")]
        public FloatParameter Blend = new FloatParameter { value = 1.0f };

        [Range(min: 0f, max: 10f), Tooltip(tooltip: "Pixel aspect ratio.")]
        public FloatParameter PixelAspectRatio = new FloatParameter { value = 0.125f };

        [Range(min: 1f, max: 1000f), Tooltip(tooltip: "Pixelization.")]
        public FloatParameter Pixelization = new FloatParameter { value = 150.0f };

        [Range(min: 0f, max: 1f), Tooltip(tooltip: "Scanline fade.")]
        public FloatParameter ScanlineFade = new FloatParameter { value = 0.05f };

        [Range(min: 0f, max: 10f), Tooltip(tooltip: "Vignette fade.")]
        public FloatParameter VignetteFade = new FloatParameter { value = 5.0f };
        #endregion
    }

    public class PixelizeRenderer : PostProcessEffectRenderer<PixelizeEffect>
    {
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Spectra/Postprocess/Pixelize"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);
            sheet.properties.SetFloat(name: "_PixelAspectRatio", value: settings.PixelAspectRatio);
            sheet.properties.SetFloat(name: "_Pixelization", value: settings.Pixelization);
            sheet.properties.SetFloat(name: "_ScanlineFade", value: settings.ScanlineFade);
            sheet.properties.SetFloat(name: "_VignetteFade", value: settings.VignetteFade);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
        }
    }
}