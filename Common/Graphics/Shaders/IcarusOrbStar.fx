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
    const float c1 = 3;
    const float c2 = c1 * 1.525;

    return x < 0.5
  ? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
  : (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
}

float star(float2 uv, float flare)
{
    float d = length(uv);
    float m =  .2 / d;
    
    float rays = max(0, 1 - abs(uv.x * uv.y * 100));
    m += rays * flare;
    m *= smoothstep(1, 0, d);
    return m;
}
// this thing is a mess, but it looks cool tho

float4 IcarusOrbStar(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 uv = (coords * 2 - 1);
    float d = length((uv * 2));
    float m = 1 / d;
    float dest = distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), uv);
    float2 polar = float2(atan2(uv.y, uv.x) / (3.1415 * 0.5), dest);
    return (star(uv, (sin(time * 5) + 2))).r * tex2D(image1, polar + time) * smoothstep(1,0,d);

    
}
    
technique Technique1
{
    pass IcarusOrbStar
    {
        
        PixelShader = compile ps_3_0 IcarusOrbStar();
    }
}