#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);

float4x4 viewWorldProjection;
float time;
float4 shaderData;
float3 colors[3];
float3 screenPosition;
float3 screenSize;

float PingPong(float value)
{
    value %= 1;
    if (value < 0)
        value += 1;

    if (value >= 0.5)
        return 2 - value * 2;

    return value * 2;
}
float2 Rotate(float2 uv, float amount)
{
    float2 uv2 = uv;
    float s = sin(amount);
    float c = cos(amount);
    uv2.x = (uv.x * c) + (uv.y * -s);
    uv2.y = (uv.x * s) + (uv.y * c);

    return uv2;
    
}
float2 expandInsideOutside(float2 uv, float dir)
{
    float1 t = (time + shaderData.w) * dir;
    float2 uv2 = Rotate(uv, t);
    float1 d = length(uv2);

    return (d * uv2 + (Rotate(uv, t + shaderData.w)) * 0.1);
    
}

float easeBack(float x)
{
    const float c1 = 2;
    const float c2 = c1 * 1.525;

    return x < 0.5
  ? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
  : (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
}

float4 GenericExplosion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 uv = coords * 2. - 1.;
    uv = Rotate(uv, shaderData.x);
    float MAXTIMELEFT = 180;
    float timeleft = shaderData.z;
    float progress = lerp(0, 1, ((timeleft - 170.) / (MAXTIMELEFT - 160.)));
    float progressAlpha = shaderData.w;
    float dest = distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), uv);
    float d = length(uv);
    float2 polar = float2(atan2(uv.y, uv.x) / (3.1415 * 0.5), dest);
    float4 col = tex2D(image1,polar - time);
    return col * (tex2D(image2, polar - time) * smoothstep(0, 1, d) * step(clamp(d - progress, 0, 1), 1) * step(d, 1) + smoothstep(0.8, .9, d) * step(d, 1)) * lerp(5, 0, progressAlpha);
}
    
technique Technique1
{
    pass GenericExplosion
    {
        
        PixelShader = compile ps_3_0 GenericExplosion();
    }
}