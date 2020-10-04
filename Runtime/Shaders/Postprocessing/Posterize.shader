Shader "Spectra/Postprocess/Posterize"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
        _ColorCount ("Color Count", Int) = 256
    }

    HLSLINCLUDE
        #include "../StdLib.hlsl"
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Blend;
        half _ColorCount;

        half4 Frag (VaryingsDefault input) : SV_Target {
            half posterization = pow(_ColorCount, 1. / 3.);

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, floor(color.rgb * posterization) / posterization, _Blend.xxx);
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
