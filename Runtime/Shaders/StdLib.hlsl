#if !defined(LEGOCYLON_SPACE_TRANSFORMS_HLSL)
#define LEGOCYLON_SPACE_TRANSFORMS_HLSL

// Copied from
// https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl
// to ensure compatibility regardless of rendering pipeline.

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

float4 unity_WorldTransformParams;
float4x4 unity_WorldToObject;
float4x4 unity_MatrixP;
float4x4 unity_MatrixInvV;
#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     unity_MatrixP
#define UNITY_MATRIX_I_P   unity_MatrixInvP
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_I_VP  unity_MatrixInvVP
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)

#if UNITY_REVERSED_Z
#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
//GL with reversed z => z clip range is [near, -far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
#define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(-(coord), 0)
#else
//D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
//max is required to protect ourselves from near plane not being correct/meaningfull in case of oblique matrices.
#define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((1.0-(coord)/_ProjectionParams.y)*_ProjectionParams.z),0)
#endif
#elif UNITY_UV_STARTS_AT_TOP
//D3d without reversed z => z clip range is [0, far] -> nothing to do
#define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
#else
//Opengl => z clip range is [-near, far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
#define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
#endif

float4x4 GetObjectToWorldMatrix()
{
    return unity_ObjectToWorld;
}

float4x4 GetWorldToObjectMatrix()
{
    return unity_WorldToObject;
}

// Transform to homogenous clip space
float4x4 GetWorldToHClipMatrix()
{
    return unity_MatrixVP;
}

float3 TransformObjectToWorld(float3 positionOS)
{
    return mul(GetObjectToWorldMatrix(), float4(positionOS, 1.0)).xyz;
}

float3 TransformWorldToObject(float3 positionWS)
{
    return mul(GetWorldToObjectMatrix(), float4(positionWS, 1.0)).xyz;
}

// Transforms position from object space to homogenous space
float4 TransformObjectToHClip(float3 positionOS)
{
    // More efficient than computing M*VP matrix product
    return mul(GetWorldToHClipMatrix(), mul(GetObjectToWorldMatrix(), float4(positionOS, 1.0)));
}

// Normalize that account for vectors with zero length
half3 SafeNormalize(float3 inVec)
{
    float dp3 = max(FLT_MIN, dot(inVec, inVec));
    return inVec * rsqrt(dp3);
}

// Normalize to support uniform scaling
float3 TransformObjectToWorldDir(float3 dirOS, bool doNormalize = true)
{
    float3 dirWS = mul((float3x3)GetObjectToWorldMatrix(), dirOS);
    if (doNormalize)
        return SafeNormalize(dirWS);

    return dirWS;
}

// Transforms normal from object to world space
float3 TransformObjectToWorldNormal(float3 normalOS, bool doNormalize = true)
{
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
    return TransformObjectToWorldDir(normalOS, doNormalize);
    #else
    // Normal need to be multiply by inverse transpose
    float3 normalWS = mul(normalOS, (float3x3)GetWorldToObjectMatrix());
    if (doNormalize)
        return SafeNormalize(normalWS);

    return normalWS;
    #endif
}

half GetOddNegativeScale()
{
    // FIXME: We should be able to just return unity_WorldTransformParams.w, but it is not
    // properly set at the moment, when doing ray-tracing; once this has been fixed in cpp,
    // we can revert back to the former implementation.
    return unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0;
}

struct VertexNormalInputs
{
    half3 tangentWS;
    half3 bitangentWS;
    float3 normalWS;
};

VertexNormalInputs GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = half3(1.0, 0.0, 0.0);
    tbn.bitangentWS = half3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    float sign = tangentOS.w * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    return tbn;
}

#endif