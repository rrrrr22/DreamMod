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

float sdSphere(float3 p, float s)
{
    return (length(p) - s);
}

float sdSpike(float3 p, float s)
{
    p = abs(p);
    float m = p.x + p.y + p.z - s;
    float3 q;
    if (3.0 * p.x < m)
        q = p.xyz;
    else if (3.0 * p.y < m)
        q = p.yzx;
    else if (3.0 * p.z < m)
        q = p.zxy;
    else
        return m * 0.57735027;
    
    float k = clamp(0.5 * (q.z - q.y + s), 0.0, s);
    return length(float3(q.x, q.y - s + k, q.z - k));

}

float sdHexPrism(float3 p, float2 h)
{
    float3 k = float3(-0.8660254, 0.5, 0.57735);
    p = abs(p);
    p.xy -= 2.0 * min(dot(k.xy, p.xy), 0.0) * k.xy;
    float2 d = float2(
       length(p.xy - float2(clamp(p.x, -k.z * h.x, k.z * h.x), h.x)) * sign(p.y - h.x),
       p.z - h.y);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}
float sdBox(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
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

float opDisplace(in float primitive, in float3 p)
{
    float d1 = primitive;
    float d2 = sin(3 * p.x + time + shaderData.x) * sin(3 * p.y + time + shaderData.x) * sin(3 * p.z + time + shaderData.x);
    return d1 + d2;
}
float map(float3 p)
{

    float shape = opDisplace(mutliLerp(sdSphere(p, float2(1, 1)), sdHexPrism(p, float2(1, 1)), sdSphere(p, float2(1, 1)), .5*sin(time)*2), p);
   
    return shape;
}

float3 normal(float3 p, float s)
{

    float2 off = float2(s, 0);
    float3 n = float3(map(p + off.xyy).x, map(p + off.yxy).x, map(p + off.xyy).x) -
    float3(map(p - off.xyy).x, map(p - off.yxy).x, map(p - off.xyy).x);
    return normalize(n);

}
float star(float2 uv, float flare)
{
    float d = length(uv);
    float m = .05 / d;
    
    m = 1 / abs(uv).xy * .05 * smoothstep(0, 1, m);

    return m;
}
float4 Core(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords * 2. - 1;
    float3 rayOrigin = float3(0, 0, -5);
    float3 rayDir = normalize(float3(uv.x, uv.y, 1));
    float t = 0;
    float3 col = float3(0, 0, 0);
    float3 p = rayOrigin + rayDir * t;

    //raymarching
    for (int i = 0; i < 25; i++)
    {
        p = rayOrigin + rayDir * t;
        float d = map(p);
        t += d;

        float edge = 0.003 * t;
        float edgeAmount = length(normal(p, 0.05) - normal(p, edge));

    }
    col += colors[0];
    
    
    
    // clear the bg and only draw the shape at full brightness
        return float4(col * shaderData.z, shaderData.z) / lerp(1, 0, t / 10);
    
}



technique Technique1
{
    pass Core
    {
        PixelShader = compile ps_3_0 Core();
    }
}
