#ifndef LEGOCYLON_COLORFILTERS_HLSL
#define LEGOCYLON_COLORFILTERS_HLSL

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"

half3 ColorFilterHue (half3 input)
{
    return RgbToHsv(input).rrr;
}

half3 ColorFilterSaturation (half3 input)
{
    return RgbToHsv(input).ggg;
}

half3 ColorFilterRed (half3 input)
{
    return input.rrr;
}

half3 ColorFilterGreen (half3 input)
{
    return input.ggg;
}

half3 ColorFilterBlue (half3 input)
{
    return input.bbb;
}

half3 ColorFilterLuminance (half3 input)
{
    half luminance = dot(input.rgb, half3(0.2126729, 0.7151522, 0.0721750));
    input = luminance.xxx;
    return input;
}

half3 ColorFilterPosterize (half3 input)
{
    half3 hsv = RgbToHsv(input);
    return HsvToRgb(half3(floor(hsv.r * 10) / 10, floor(hsv.g * 10) / 10, floor(hsv.b * 10) / 10));
}

half3 ColorFilterSepia (half3 i)
{
    return mul(half3x3(0.393, 0.769, 0.189,  0.349, 0.686, 0.168,  0.272, 0.534, 0.131), i);
}

// The following matrix values are from https://github.com/PlanetCentauri/ColorblindFilter/blob/master/Code
half3 ColorFilterProtanopia (half3 i)
{
    return mul(half3x3(0.567, 0.433, 0.0,  0.558, 0.442, 0.0,  0.0, 0.242, 0.758), i);
}

half3 ColorFilterProtanomaly (half3 i)
{
    return mul(half3x3(0.817, 0.183, 0.0,  0.333, 0.667, 0.0,  0.0, 0.125 ,0.875), i);
}

half3 ColorFilterDeuteranopia (half3 i)
{
    return mul(half3x3(0.625, 0.375, 0.0,  0.7, 0.3, 0.0,  0.0, 0.3, 0.7), i);
}

half3 ColorFilterDeuteranomaly (half3 i)
{
    return mul(half3x3(0.8, 0.2, 0.0,  0.258, 0.742, 0.0,  0.0, 0.142 ,0.858), i);
}

half3 ColorFilterTritanopia (half3 i)
{
    return mul(half3x3(0.95, 0.05, 0.0,  0.0, 0.433, 0.567,  0.0, 0.475 ,0.525), i);
}

half3 ColorFilterTritanomaly (half3 i)
{
    return mul(half3x3(0.967, 0.033, 0.0,  0.0, 0.733, 0.267,  0.0, 0.183 ,0.817), i);
}

half3 ColorFilterAchromatopsia (half3 i)
{
    return mul(half3x3(0.299, 0.587, 0.114,  0.299, 0.587, 0.114,  0.299, 0.587 ,0.114), i);
}

half3 ColorFilterAchromatomaly (half3 i)
{
    return mul(half3x3(0.618, 0.320, 0.062,  0.163, 0.775, 0.062,  0.163, 0.320 ,0.516), i);
}

#endif