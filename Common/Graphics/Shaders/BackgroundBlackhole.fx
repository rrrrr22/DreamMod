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
    
    float rays = max(0, 1 - abs(uv.x * uv.y * 255));
    m += rays * flare;
    uv *= Rotate(uv, 3.1415 / 4.);
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
    return float2(x, y) - .5;
}

float4 layer(float2 uv, float l)
{
    float4 layer = float4(0, 0, 0, 0);
    float2 gridUV = frac(uv * 8) - .5;
    float2 gridID = floor(uv * 8) - .5;
    for (float y = -1; y <= 1.; y++)
    {
        for (float x = -1; x <= 1.; x++)
        {
            float2 offset = float2(x, y);
            float2 rv = random(gridID + offset + l);
            float size = saturate((sin(time * 5 * rv.x)) * rv.y * .5) + .05;
            float d = length(gridUV - offset - rv);
            layer += star(gridUV - offset - rv, rv.y * 0.5) * size * float4(rv.x * 7., rv.x * 6, rv.y * 8, 0) * smoothstep(1, 0, d);
            
        }
    
    }
    return layer;
}
float sdSphere(float3 p, float s)
{
    return (length(p) - s);
}

float sdPlane(float3 p, float3 normlized, float h)
{
    return dot(p, normlized) + h;
}
float3 rotateZ(float3 p, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float3 p2 = p;
    p2.y = (p.x * c) + (p.y * -s);
    p2.x = (p.x * s) + (p.y * c);

    return p2;

}
float3 rotateX(float3 p, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float3 p2 = p;
    p2.y = (p.z * c) + (p.y * -s);
    p2.z = (p.z * s) + (p.y * c);

    return p2;

}

float mutliLerp(float value1, float value2, float value3, float t)
{
    float value = 0;
    if (t < 0.5)
    {
        value = lerp(value1, value2, (t) * 2);

    }
    else
    {
        value = lerp(value2, value3, (t - 0.5) * 2);

    }
    return value;

}
float map(float3 p)
{
    //p = rotateX(p, -3.1415 / 2 * 1.25);
    //p = rotateZ(p, 3.1415);
    
    float plane = sdPlane(p, normalize(float3(sin(time) + 3, sin(time) + 3, sin(time)+3)), normalize(10));
    float blacksphere = sdSphere(p + float3(0, 0, 1), .1);
    return plane;
}

float map2(float3 p)
{
    return sdSphere(p, 16);

}

float3 normal(float3 p, float s)
{

    float2 off = float2(s, 0);
    float3 n = float3(map(p + off.xyy).x, map(p + off.yxy).x, map(p + off.xyy).x) -
    float3(map(p - off.xyy).x, map(p - off.yxy).x, map(p - off.xyy).x);
    return normalize(n);

}


float4 Blackhole(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 uv = (coords) * 2. - 1;
    float3 rayOrigin = float3(0, 0, -10);
    float3 rayDir = normalize(float3(uv.x, uv.y, 1));
    float t = 0;
    float3 col = float3(0, 0, 0);

    //raymarching
    for (int i = 0; i < 125; i++)
    {
        float3 p = rayOrigin + rayDir * t;
        float d = map(p);
        t += d;
        
        //polar
        float dest = distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), p);
        float angle = atan2(p.y, p.x) / (3.1415 * 0.5);
        float2 polarUV = float2(angle, dest);
        col = t;
    }
    // clear the bg and only draw the shape at full brightness
    return float4(col, 1) * smoothstep(1, 0, t / 10);
}
    
technique Technique1
{
    pass Blackhole
    {
        
        PixelShader = compile ps_3_0 Blackhole();
    }
}