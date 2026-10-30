#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile_fragment _ _SHADOWS_SOFT

void CalculateMainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float Attenuation)
{
#if defined(SHADERGRAPH_PREVIEW)
    Direction = float3(0.5, 0.5, 0.0);
    Color = 1.0f;
    Attenuation = 1.0f;
#else
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
        float4 clipPos = TransformWorldToHClip(WorldPos);
        float4 shadowCoord = ComputeScreenPos(clipPos);
    #else
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif

    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    float rawAtten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
    Attenuation = smoothstep(0.35, 0.65, rawAtten);
#endif
}

#endif // CUSTOM_LIGHTING_INCLUDED