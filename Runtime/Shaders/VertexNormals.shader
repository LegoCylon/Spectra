Shader "Spectra/Vertex Normals" {
    HLSLINCLUDE
        // Required to compile gles 2.0 with standard srp library
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        #pragma target 2.0

        #pragma vertex vert

        #include "StdLib.hlsl"

        struct Attributes {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings {
            float4 positionCS : SV_POSITION;
            float3 normalWS : TEXCOORD0;
        };

        Varyings vert (Attributes input) {
            Varyings output = (Varyings)0;

            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.normalWS = normalInputs.normalWS;

            return output;
        }

        half4 frag (Varyings input) : SV_Target {
            return half4((1.0 + normalize(input.normalWS)) * 0.5, 1.0);
        }
    ENDHLSL

    Category {
        Tags { "IgnoreProjectors" = "True" }

        LOD 100

        SubShader {
            Tags { "RenderType" = "Background" }

            Pass {
                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Opaque" }

            Pass {
                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Transparent" }

            Pass {
                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }
    }
}
