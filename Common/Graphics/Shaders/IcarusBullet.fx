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

float4 Bullet(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{

    float2 uv = (coords * 2 - 1);
    float d = length((uv.y * 5));
    float m = 1 / d;
    float4 fire = m * sin(time * 20) * .025 + .2 * m;
    fire *= smoothstep(0, .5, fire);
    fire.r *= (m - float2(0., 0.5));
    fire.a = fire.r;
    
    float4 color1 = fire * 1000 * m;
    color1 = floor(color1 * 5) / 5;

    //initial position of the fire
    color1 *= smoothstep(0, 1, uv.x + .5);
    
    //apply le color
    color1.rgb = saturate(color1.rgb);

    //end point of the fire
    color1 *= smoothstep(1, 0, coords.x * coords.x * .1);
    color1 = lerp(saturate(color1) * 3, float4(0, 0, 0, 0), coords.x + (frac(time * 5) * 0.4));
    
    color1 *= m;
    color1.rgb *= tex2D(image1, uv * .025 + float2(time,time)).rgb;


    return color1;

}
    
technique Technique1
{
    pass Bullet
    {
        
        PixelShader = compile ps_3_0 Bullet();
    }
}