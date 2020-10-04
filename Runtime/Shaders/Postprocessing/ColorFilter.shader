Shader "Spectra/Postprocess/Color Filter"
{
    Properties
    {
        _Blend ("Filter Intensity", Range(0, 1)) = 1.0
        _RedMix ("Red Mix", Vector) = (1, 0, 0, 1)
        _GreenMix ("Green Mix", Vector) = (0, 1, 0, 1)
        _BlueMix ("Blue Mix", Vector) = (0, 0, 1, 1)
    }

    HLSLINCLUDE
        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;
        half _Blend;
        half4 _RedMix;
        half4 _GreenMix;
        half4 _BlueMix;

        half4 Frag (VaryingsDefault input) : SV_Target {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = lerp(color.rgb, mul(half3x3(_RedMix.rgb, _GreenMix.rgb, _BlueMix.rgb), color.rgb), _Blend.xxx);
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
