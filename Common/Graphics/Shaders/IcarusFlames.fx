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

float4 IcarusFlames(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float MAXTIMELEFT = 180;
    float timeleft = shaderData.y;
    float progress = lerp(1, 0, ((timeleft - 160) / (MAXTIMELEFT - 150)));

    float2 uv = coords * 2. - 1.;
    float d = length(uv);
    float blub = 1 / d.xxxx * 1;
    blub *= smoothstep(1,0,d);
    float4 fireTexture = tex2D(image1, float2(uv.x, uv.y + time + shaderData.x) * 2) * blub;
    fireTexture.a *= fireTexture.r;
    
    //explosion

    float4 noise1 = tex2D(image1, uv + time);
    float4 noise2 = tex2D(image1, expandInsideOutside(uv, -1));
    float circle1 = clamp(smoothstep(0, 1, d / progress) * 15, 0, 1);
    circle1 *= lerp(0, noise1.r, 1 / d);
    float circle2 = smoothstep(0.1, 1, progress / d);
    float circle3 = clamp(-1 * (0.5 - distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), uv) * (lerp(0, 1, 1 / progress))), 0, 1);
    circle3 *= 2;
    circle3 *= smoothstep(0.9, 1, clamp(progress, 0, 0.75) / d);


    float4 color1 = float4(colors[2].rgb, 1);
    color1 *= 15;
    color1 += noise1;
    color1.a = color1.r;
    color1.rgb += colors[2] * (d - 8);
    color1 /= noise1.r * 1.5;
    color1.a = colors[0].r;

    float4 fireball = fireTexture.a * float4(lerp(colors[1], lerp(colors[0], colors[2], (float)shaderData.y / MAXTIMELEFT), blub * fireTexture.a), 1);
    //step(d,0.75) removes the corners since full circles only take 3/4 of the rendertarget 
    return lerp(color1 * circle3 * (((timeleft - 160) / MAXTIMELEFT) + 1),fireball, fireball);
    
    
     
    
}
    
technique Technique1
{
    pass IcarusFlames
    {
        
        PixelShader = compile ps_3_0 IcarusFlames();
    }
}