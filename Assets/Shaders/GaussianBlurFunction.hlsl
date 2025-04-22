//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

//wtf?

float gaussianCalc(int x, int y, float spread)
{
    float sigmaSqu = spread * spread;
    return (1 / sqrt(6.28319 * sigmaSqu)) * pow(2.71828, -((x * x) + (y * y)) / (2 * sigmaSqu));
}

void GaussianBlur_float(UnityTexture2D targetTexture, float2 texelSize, float2 UV, float kernelSize, UnitySamplerState samplerState, float spread, out float4 OutRGBA)
{
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float kernelSum = 0.0;

    int upper = ((kernelSize - 1) / 2);
    int lower = -upper;

    for (int x = lower; x <= upper; ++x)
    {
        for (int y = lower; y <= upper; ++y)
        {
            float gauss = gaussianCalc(x, y, spread);
            kernelSum += gauss;
    
            float2 offset = float2(texelSize.x * x, texelSize.y * y);
            col += gauss * targetTexture.Sample(samplerState, UV + offset);
        }
    }

    col /= kernelSum;
    OutRGBA = float4(col.r, col.g, col.b, col.a);
}

#endif