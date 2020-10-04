Shader "Spectra/Replacement/Holodeck" {
    HLSLINCLUDE
        #pragma vertex vert

        #include "../StdLib.hlsl"

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

        Varyings vert (Attributes input) {
            Varyings output = (Varyings)0;

            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.color = input.color;
            output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);

            return output;
        }

        half4 frag (Varyings input) : SV_Target {
            float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord).a * input.color.a;

            half2 square = step(.025, abs(frac(input.texcoord * 10.)));
            return half4((1.0 - step(1.5, square.x + square.y)) * half3(1., .95, 0.), 1.) * alpha;
        }
    ENDHLSL

    Category {
        Tags { "IgnoreProjectors" = "True" }

        LOD 100

        SubShader {
            Tags { "RenderType" = "Background" }

            Pass {
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Opaque" }

            Pass {
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "QUEUE"="Transparent" "RenderType" = "Transparent" }

            Blend One OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            Pass {
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }
    }
}
