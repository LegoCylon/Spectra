Shader "Spectra/CRT"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 1.)) = .5
        _Contrast ("Contrast", Range(-1., 1.)) = .1
        _PixelAspectRatio("Pixel Aspect Ratio", Range(0., 1.)) = .125
        _PixelQuantization("Pixel Quantization", Range(1., 1000.)) = 150.0
        _Posterization("Posterization", Range(1., 1000.)) = 256.0
        _ScanlineFade("Scanline Fade", Range(0.0, 1.0)) = 0.05
        _VignetteFade("Vignette Fade", Range(0.0, 10.0)) = 5.0
        [Toggle(MONOCHROME_ENABLED)] _Monochrome("Monochrome", Int) = 1
    }

    HLSLINCLUDE
        #include "StdLib.hlsl"
        #include "ColorFilters.hlsl"

        #pragma shader_feature_local MONOCHROME_ENABLED

        struct Attributes {
            float4 positionOS : POSITION;
            float4 color : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        struct Varyings {
            float4 positionCS : SV_POSITION;
            float4 color : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Brightness;
        half _Contrast;
        float _PixelQuantization;
        half _PixelAspectRatio;
        float _Posterization;
        half _ScanlineFade;
        half _VignetteFade;

        Varyings Vert (Attributes input) {
            Varyings output = (Varyings)0;

            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.color = input.color;
            output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);

            return output;
        }

        half4 Sample(TEXTURE2D_ARGS(t, s), half2 uv, float4 tint)
        {
            half4 color = SAMPLE_TEXTURE2D(t, s, uv) * tint;
            color.rgb *= color.a;
            return color;
        }

        half4 Frag (Varyings input) : SV_Target {
            // Quantize uv
            half2 uv = input.texcoord;
            uv = floor(uv * _PixelQuantization) / _PixelQuantization;

            // Multisample
            half4 color = Sample(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv, input.color);
            color *= .6;
            color += Sample(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv + half2(.5, .5) / _PixelQuantization, input.color) * .1;
            color += Sample(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv + half2(.5, -.5) / _PixelQuantization, input.color) * .1;
            color += Sample(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv + half2(-.5, -.5) / _PixelQuantization, input.color) * .1;
            color += Sample(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv + half2(-.5, .5) / _PixelQuantization, input.color) * .1;

            // Brightness
            color.rgb *= 1. + _Brightness;

            // Contrast
            color.rgb = saturate((1. + _Contrast) * (color.rgb - 0.5) + 0.5);

            // Posterize color
            half posterization = pow(_Posterization, 1. / 3.);
            color.rgb = floor(color.rgb * posterization) / posterization;

            // Monochrome
            #if MONOCHROME_ENABLED
            color.rgb = half3(.1, 1., .1) * ColorFilterLuminance(color.rgb);
            #endif

            // Vignette + Scanline
            half scanline = frac(_Time[3] + uv.y);
            half2 offset = abs(frac(input.texcoord * _PixelQuantization) - 0.5);
            offset.x *= _ScreenParams.x / _ScreenParams.y * _PixelAspectRatio;
            half vignette = pow(saturate(1.0 - dot(offset, offset)), _VignetteFade);
            color.rgb = saturate(color.rgb * vignette * lerp(1.0 - _ScanlineFade, 1.0, scanline));
            return color;
        }
    ENDHLSL

    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
