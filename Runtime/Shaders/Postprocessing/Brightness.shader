Shader "Spectra/Postprocess/Brightness"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
        _Brightness ("Brightness", Range(0, 100)) = 0.0
    }

    HLSLINCLUDE
        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Blend;
        half _Brightness;

        half4 Frag (VaryingsDefault input) : SV_Target {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, color.rgb * (1. + _Brightness).xxx, _Blend.xxx);
            color.rgb *= color.a;
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
