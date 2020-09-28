Shader "Hidden/Spectra/Ghost" {
    HLSLINCLUDE
        // Required to compile gles 2.0 with standard srp library
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        #pragma target 2.0

        #pragma vertex vert

        #include "StdLib.hlsl"

        struct Attributes {
            float4 positionOS : POSITION;
            float4 color : COLOR;
            float3 normalOS : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

        struct Varyings {
            float4 positionCS : SV_POSITION;
            float4 color : COLOR;
            float2 texcoord : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            half fresnel : TEXCOORD2;
        };

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_ST;

        Varyings vert (Attributes input) {
            Varyings output = (Varyings)0;

            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
            output.color = input.color;
            output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.positionCS = mul(GetWorldToHClipMatrix(), float4(output.positionWS, 1.0));

            float fresnelBias = 0.0;
            float fresnelPower = 8.0;
            float fresnelScale = 4.5;
            half3 eyeDir = normalize(output.positionWS - _WorldSpaceCameraPos.xyz);
            output.fresnel = fresnelBias + fresnelScale * pow(1. + dot(eyeDir, normalInputs.normalWS), fresnelPower);

            return output;
        }

        half4 frag (Varyings input) : SV_Target {

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord) * input.color;
            color.rgb = lerp(saturate(float3(color.r, .25 + color.g, .5 + color.b * 1.5)), float3(.6, 1., 1.), input.fresnel);
            color.a = lerp(color.a * 0.5, 1.0, input.fresnel);
            color.rgb *= color.a;
            return color;
        }
    ENDHLSL

    Category {
        Tags { "IgnoreProjectors" = "True" }

        LOD 100

        SubShader {
            Tags { "RenderType" = "Background" }

            // extra pass that renders to depth buffer only
            Pass {
                ZWrite On
                ColorMask 0
            }

            Pass {
                Blend One OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Opaque" }

            // extra pass that renders to depth buffer only
            Pass {
                ZWrite On
                ColorMask 0
            }

            Pass {
                Blend One OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }

        SubShader {
            Tags { "RenderType" = "Transparent" }

            Pass {
                Blend One OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                Name "Unlit"
                HLSLPROGRAM
                    #pragma fragment frag
                ENDHLSL
            }
        }
    }
}
