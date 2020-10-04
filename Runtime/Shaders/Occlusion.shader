Shader "Spectra/Occlusion" {
    HLSLINCLUDE
        // Required to compile gles 2.0 with standard srp library
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        #pragma target 2.0

        #pragma vertex vert

        #include "StdLib.hlsl"

        struct Attributes {
            float4 positionOS : POSITION;
        };

        struct Varyings {
            float4 vertex : SV_POSITION;
        };

        Varyings vert (Attributes input) {
            Varyings output = (Varyings)0;

            output.vertex = TransformObjectToHClip(input.positionOS.xyz);

            return output;
        }

        half4 fragOpaque (Varyings input) : SV_Target {
            return half4(0.1, 0.04, 0.0, 0.2);
        }

        half4 fragTransparent (Varyings input) : SV_Target {
            return half4(0.1, 0.04, 0.0, 0.2);
        }
    ENDHLSL

    Category {
        Tags { "IgnoreProjectors" = "True" }

        LOD 100

        SubShader {
            Tags { "RenderType" = "Opaque" }

            // extra pass that renders to depth buffer only
            Pass {
                ZWrite On
                ColorMask 0
            }

            Pass {
                Blend One One
                ZTest Greater
                ZWrite Off

                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment fragOpaque
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Transparent" }

            Blend One One
            Cull Off
            ZTest Greater
            ZWrite Off

            Pass {
                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment fragTransparent
                ENDHLSL
            }
        }
    }
}
