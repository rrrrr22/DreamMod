#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);

float4x4 viewWorldProjection;
float time;
float4 shaderData;
float3 colors[3];
float2 screenPosition;
float2 screenSize;
float2 screenCenter;
struct VertexShaderInput
{
    float4 Position : POSITION0;
};
 
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 TextureCoordinate : TEXCOORD0;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
 
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
 
    float4 VertexPosition = mul(input.Position, World);
    output.TextureCoordinate = VertexPosition - CameraPosition;
 
    return output;
}
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
float2 expandInsideOutside(float2 uv)
{
    float1 t = time + shaderData.w;
    float2 uv2 = Rotate(uv, t);
    float1 d = length(uv2);

    return (d * uv2 + ((t) - uv2));
    
}

float star(float2 uv, float flare)
{
    float d = length(uv);
    float m = .05 / d;
    
    float rays = max(0, 1-abs(uv.x*uv.y* 255));
    m += rays * flare;
    uv *= Rotate(uv,3.1415/4.);
    rays = max(0, 1 - abs(uv.x * uv.y * 255));
    m += rays * .3 * flare;
    return m;
}
float2 random(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    float x = frac(p.x * 125.63);
    float y = frac(p.y * 735.32);
    return float2(x,y) - .5;
}

float4 layer(float2 uv, float l, float s)
{
    float4 layer = float4(0,0,0,0);
    float2 gridUV = frac(uv * 8) - .5;
    float2 gridID = floor(uv * 8) - .5;
    for (float y = -1; y <= 1.; y++)
    {
        for (float x = -1; x <= 1.; x++)
        {
            float2 offset = float2(x, y);
            float2 rv = random(gridID + offset + l);
            float size = saturate((sin(time * 5 * rv.x)) * rv.y * .5) + s;
            float d = length(gridUV - offset - rv);
            layer += star(gridUV - offset - rv, rv.y * 0.5) * size * float4(cos(length(rv * 20) * lerp(float3(0, 2, 1), float3(0, 0, 1), uv.x)) + .2, 0) * smoothstep(1, 0, d);

        }
    
    }
    return layer;
}

float4 randomAura(float2 baseUV, float i,float max_i, float3 color)
{
    float2 gridUV = frac(baseUV) - .5;
    float2 gridID = floor(baseUV) - .5;
    float2 auraUV = baseUV + random(gridID);
    float4 aura = tex2D(image1, auraUV + .2);


    float2 rv = random(gridID);
    aura = tex2D(image1, (gridUV - rv) * 2);
    aura *=  0.5 * (1 / length(auraUV));
    //aura.a *= float3(cos(length(auraUV * 20) * float3(5, 2, 10)));
    //aura.rgb *= 0.25;
   
   return aura;
}

float4 Cosmic(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position, VertexShaderInput input) : COLOR0
{
    float2 centeredUV = float2(coords.x * (screenSize.x / screenSize.y), coords.y * (screenSize.x / screenSize.y));
    float2 BHcenteredUV = centeredUV - float2(lerp(1, 0, screenPosition.x / 500 / 16), lerp(1, 0, screenPosition.y / 500 / 16));
    BHcenteredUV *= 1;
    float2 pixelatedUV = round(BHcenteredUV * (256.)) / 256.;

    float d = length(pixelatedUV);
    float angle = atan2(pixelatedUV.y, pixelatedUV.x);


    pixelatedUV = Rotate(pixelatedUV,-time * 0.4);
    
    float angle2 = atan2(pixelatedUV.y, pixelatedUV.x);
    float2 VortexUV = float2(sin(angle + d * lerp(25, 5, d) - time * 0.03), d * lerp(5, 5, d) + time * 0.03);
    float2 VortexUV2 = float2(sin(angle2 + d * lerp(25, 5, d) - time * 0.03), d * lerp(5, 5, d) + time * 0.03) * 2;

    float4 finalCol = tex2D(image1, VortexUV).r;
    finalCol += tex2D(image1, VortexUV2);

    finalCol = floor(finalCol * (6)) / 6;

    finalCol *= smoothstep(1.25,0,d);
    
    //blackhole
    finalCol = lerp(float4(0, 0, 0, 1), finalCol, saturate(smoothstep(0.2, 1, d * 2)));

    finalCol.rgb *= lerp(colors[0], lerp(colors[1],colors[2],VortexUV2.x), finalCol.r);
    finalCol *= 1;

    
    //star "system" if you could call it that lol
    float4 space = float4(0,0,0,1);


    for (float i = 0; i < 25; i += 1)
    {
        float2 starUV = round(coords * 1024) / 1024 - float2(lerp(1, 0, screenPosition.x / 500 / 16), lerp(1, 0, screenPosition.y / 500 / 16));
        space += layer(starUV, i,  i / 1000);

    }
    
    
    //finally, the aura thingy
    //float4 aura = 0;
    //for (float j = 0; j < 10; j += 10)
    //{
    //    aura += randomAura(starUV3, j, 10, colors[0]);

    //}
    float2 nebulaUV = coords - float2(lerp(1, 0, screenPosition.x / 500 / 16), 0);

    float4 nebula = texCUBE(image2, input.Position);
    
    
    //unused blackhole here , i dont want swiss cheese in my cosmic dimension...
    return lerp(lerp(lerp(float4(0, 0, 0, 2), nebula * float4(coords.yxx, 1) * 2, nebula), finalCol, 0), space, space);
}
    
technique Technique1
{
    pass Cosmic
    {
        
        PixelShader = compile ps_3_0 Cosmic();
    }
}