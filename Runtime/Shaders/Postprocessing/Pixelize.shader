Shader "Spectra/Postprocess/Pixelize"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
        _PixelAspectRatio("Pixel Aspect Ratio", Range(0., 10.)) = .125
        _Pixelization ("Pixelization", Range(1, 1000)) = 150.0
        _ScanlineFade("Scanline Fade", Range(0.0, 1.0)) = 0.05
        _VignetteFade("Vignette Fade", Range(0.0, 10.0)) = 5.0
    }

    HLSLINCLUDE
        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Blend;
        half _PixelAspectRatio;
        half _Pixelization;
        half _ScanlineFade;
        half _VignetteFade;

        half4 Frag (VaryingsDefault input) : SV_Target {
            half2 uv = input.texcoord;
            uv = floor(uv * _Pixelization) / _Pixelization;

            // Multisample
            half4 pixelizedColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            pixelizedColor *= .6;
            pixelizedColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + half2(.5, .5) / _Pixelization) * .1;
            pixelizedColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + half2(.5, -.5) / _Pixelization) * .1;
            pixelizedColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + half2(-.5, -.5) / _Pixelization) * .1;
            pixelizedColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + half2(-.5, .5) / _Pixelization) * .1;

            // Vignette + Scanline
            half scanline = frac(_Time[3] + uv.y);
            half2 offset = abs(frac(input.texcoord * _Pixelization) - 0.5);
            offset.x *= _ScreenParams.x / _ScreenParams.y * _PixelAspectRatio;
            half vignette = pow(saturate(1.0 - dot(offset, offset)), _VignetteFade);
            pixelizedColor.rgb = saturate(pixelizedColor.rgb * vignette * lerp(1.0 - _ScanlineFade, 1.0, scanline));

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, pixelizedColor.rgb, _Blend.xxx);
            return color;
        }
    ENDHLSL

    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
