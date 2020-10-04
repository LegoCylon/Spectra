Shader "Spectra/Vertex Colors" {
    HLSLINCLUDE
        #pragma vertex vert

        #include "StdLib.hlsl"

        struct Attributes {
            float4 positionOS : POSITION;
            float4 color : COLOR;
        };

        struct Varyings {
            float4 vertex : SV_POSITION;
            float4 color : COLOR;
        };

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;

        Varyings vert (Attributes input) {
            Varyings output = (Varyings)0;

            output.vertex = TransformObjectToHClip(input.positionOS.xyz);
            output.color = input.color;

            return output;
        }

        half4 frag (Varyings input) : SV_Target {
            float4 color = input.color;
            color.rgb *= color.a;
            return color;
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
