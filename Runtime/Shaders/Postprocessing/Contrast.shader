Shader "Spectra/Postprocess/Contrast"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
        _Contrast ("Brightness", Range(-10, 10)) = 0.0
    }

    HLSLINCLUDE
        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Blend;
        half _Contrast;

        half4 Frag (VaryingsDefault input) : SV_Target {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, saturate((1. + _Contrast) * (color.rgb - 0.5) + 0.5), _Blend.xxx);
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
