Shader "Spectra/Green"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
        #include "StdLib.hlsl"
        #include "ColorFilters.hlsl"

        struct Attributes {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        struct Varyings {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
        };

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;

        Varyings Vert (Attributes input) {
            Varyings output = (Varyings)0;

            output.vertex = TransformObjectToHClip(input.positionOS.xyz);
            output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);

            return output;
        }

        half4 Frag (Varyings input) : SV_Target {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            color.rgb = ColorFilterGreen(color.rgb) * color.a;
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
