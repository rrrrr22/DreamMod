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
float sdVesica(float2 p, float w, float h)
{
    float3 d = 0.5 * (w * w - h * h) / h;
    p = abs(p);
    float3 c = (w * p.y < d * (p.x - w)) ? float3(0.0, w, 0.0) : float3(-d.x, 0.0, (d.x) + h);
    return length(p - c.yx) - c.z;
}
float sdCone(float3 p, float2 c, float h)
{
  // c is the sin/cos of the angle, h is height
  // Alternatively pass q instead of (c,h),
  // which is the point at the base in 2D
    float2 q = h * float2(c.x / c.y, -1.0);
    
    float2 w = float2(length(p.xz), p.y);
    float2 a = w - q * clamp(dot(w, q) / dot(q, q), 0.0, 1.0);
    float2 b = w - q * float2(clamp(w.x / q.x, 0.0, 1.0), 1.0);
    float k = sign(q.y);
    float d = min(dot(a, a), dot(b, b));
    float s = max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
    return sqrt(d) * sign(s);
}
float3 Rotate(float3 p, float3 axis, float angle)
{
    return lerp(dot(axis, p) * axis, p, cos(angle)) + cross(axis, p) * sin(angle);
}

float2 random(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    float x = frac(p.x * 125.63);
    float y = frac(p.y * 735.32);
    return float2(x, y) - .5;
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

float3 mutliLerp(float3 value1, float3 value2, float3 value3, float t)
{
    float3 value = 0;
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
float map(float3 p, out int ID, int exclude)
{
    //p = rotateX(p, -3.1415 / 2 * 1.25);
    //p = rotateZ(p, 3.1415);

    
    //p = Rotate(p, float3(0,0,1),3.1415);
    //p = Rotate(p, float3(0,1,0),3.1415 *1);
    p = Rotate(p, float3(1, 0, 0), 3.1415 * 1);
    float plane = sdPlane(p, normalize(float3(1, 1, 1)),
    1);
    p = Rotate(p, float3(0, 1, 0), 3.1415 / 2);
    p = Rotate(p, float3(0, 0, 1), 3.1415 / 2);

    float plane2 = sdPlane(p, normalize(float3(1, 1, 1)), 1);

    float d = plane;
    d = min(d, plane2);
    if (d == plane)
    {
        ID = 0;
    }
    else if (d == plane2)
    {
        ID = 1;
    }
    else
        ID = 69;
       

    return d;
}

float map2(float3 p)
{
    return sdSphere(p, 16);

}




float4 Blackhole(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 uv = (coords + float2(0.1, .1)) * 2. - 1;
    float3 rayOrigin = float3(0, 0, -2);
    float3 rayDir = normalize(float3(uv.x * 3, uv.y * 3, 1));
    float t = 0;
    float3 col = float3(0, 0, 0);
    float3 p = rayOrigin + rayDir * t;
    int ID = 0;
    float lastD = -1;
    //raymarching
    for (int i = 0; i < 25; i++)
    {
        p = rayOrigin + rayDir * t;
        float d = map(p, ID, 0);
        t += d;


    }
    float2 polar = float2(atan2(p.y, p.x) / (3.1415 / 2) * 1, length(p.xy));
    float2 polar2 = float2(atan2(p.y, p.x) / (3.1415 / 2) * 2, length(p.xy)) * .5;
    float2 polar3 = float2(atan2(p.y, p.x) / (3.1415 / 2) * 4, length(p.xy)) * .25;
    switch (ID)
    {
        case 0:
    
            col = tex2D(image1, polar * .25 + time * 2);
            col += 1 / polar.y;
            col *= tex2D(image1, polar2 * 2 + time * 1.5).r;
            col *= tex2D(image1, polar3 * 2 + time * 1.25).r;
            col *= smoothstep(2.25, 0, polar.y);
            
            //col = sdVesica(p.xy,1,1).xxx;
            //col = tex2D(image1, p.xy + time);

            break;
            
        case 1:
            col += abs(1 / p.x);
            col *= smoothstep(2.25, 0, abs(p.x));
            col *= tex2D(image1, Rotate(polar, 3.1415 / 2) + float2(time, 0)).r;

            break;
            
        default:

            break;
    }
    
    
    //aura
    
    //col += tex2D(image2, uv).r;
    
    // clear the bg and only draw the shape at full brightness
    return float4(col, 1);
}
    
technique Technique1
{
    pass Blackhole
    {
        
        PixelShader = compile ps_3_0 Blackhole();
    }
}