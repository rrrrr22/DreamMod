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
float sdCylinder(float3 p, float3 a, float3 b, float r)
{
    float3 ba = b - a;
    float3 pa = p - a;
    float baba = dot(ba, ba);
    float paba = dot(pa, ba);
    float x = length(pa * baba - ba * paba) - r * baba;
    float y = abs(paba - baba * 0.5) - baba * 0.5;
    float x2 = x * x;
    float y2 = y * y * baba;
    
    float d = (max(x, y) < 0.0) ? -min(x2, y2) : (((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0));
    
    return sign(d) * sqrt(abs(d)) / baba;
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
float map(float3 p, out int ID, int exclude, out float3 lastP)
{

    p = Rotate(p, float3(1, 0, 0), 3.1415);
    p = Rotate(p, float3(0, 0, 1), 3.1415 / 2 * .64);
    p = Rotate(p, float3(1, 0, 0), 3.1415 / 2 * .25);
    
    //float plane = sdPlane(p, normalize(float3(0, 0, .25)),2);
    float plane = sdSphere(p,5);
    
    //p = Rotate(p, float3(1, 0, 0), 3.1415 / 2 * -1);
    //p = Rotate(p, float3(0, 1, 0), 3.1415);
    //float plane2 = sdCylinder(p, float3(0, 0, 0), float3(-10,0,10),0.25);

    float d = plane;
    if (d == plane)
    {
        ID = 0;
    }
    else if (d == 1)
    {
        ID = 1;
    }
    else
        ID = 69;
       
    lastP = p;
    return (d);
}

float map2(float3 p)
{
    return sdSphere(p, 16);

}




float4 Blackhole(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float2 uv = (coords + float2(-0.2, -.5) + screenPosition / 500/ 16) * 2. - 1;
    float3 rayOrigin = float3(0, 0, -20);
    float3 rayDir = normalize(float3(uv.x, uv.y, 1));
    float t = 0;
    float3 col = float3(0, 0, 0);
    float3 p = rayOrigin + rayDir * t;
    int ID = 0;
    float lastD = -1;
    float3 lastP = 0;
    //raymarching
    for (int i = 0; i < 50; i++)
    {
        p = rayOrigin + rayDir * t;
        float d = map(p, ID, 0, lastP);
        t += d;

    }
    p = lastP;
    float2 polar = float2(atan2(p.y, p.x) / (3.1415 / 2) + length(p), length(p.xy));
    float2 polar2 = float2(atan2(p.y, p.x) / (3.1415 / 2) * 2, length(p.xy)) * .5;
    float2 polar3 = float2(atan2(p.y, p.x) / (3.1415 / 2) * 4, length(p.xy)) * .25;
    switch (ID)
    {
        case 0:
            
            float directions = 32;
            float angle = atan2(p.y, p.x) / (3.1415 / 2);
            float d = length(p.xy);
            float2 VortexUV = float2(sin(angle + d - time * 1), d + time * 0.03);

            
            float3 eye = tex2D(image1, VortexUV);
            float3 eyeMask = tex2D(image1, VortexUV - time * 5);
           // eye *= eyeMask.r;
            eye += 1 / polar.y;

            eye *= smoothstep(1, 0, 1 / length(p) * 4) * (1 / length(p)*50);
            //le color
            eye *= (cos((polar.y) / (1.0 + t) + time + float3(6, 1, 2))
            + 1.3) / t * 5;

            //col = sdVesica(p.xy,1,1).xxx;
            //col = tex2D(image1, p.xy + time);
            float3 eyeOfTheAbyss = tex2D(image1, (VortexUV.xy + time * 2));
            float3 eyeOfTheAbyssMask = tex2D(image1, (VortexUV.xy - time * 7) * .1).r;
            float circleMult = smoothstep(smoothstep(3, 0, 1 / length(p)) * 100,
            1, 1 / length(p));
            col = eye;

            col += tex2D(image1, polar + float2(screenPosition / 500 /16) + time * 0.25) * (cos((length(p) / .1) / (1.0 + t) + time + float3(6, 1, 2))
            + 1.3) / t * (smoothstep(4, 0, length(p + float2(screenPosition / 500 / 16)))) * tex2D(image1, uv + float2(screenPosition / 500 / 16) * 2.25
            - time * 0.25).
            r;
             
            col -= star(p.xy / 10, 2 * (1 - sin(time * 25) * .5)) * smoothstep(1, 0, length(p.xy));
            col *= smoothstep(1, 0, length(p.xy) / 20);
            col *= 2;
                break;
            
        case 1:
            col += abs(1/lastP.x) * .1;
            col *= tex2D(image1, Rotate(polar, 3.1415 / 2) + float2(time, 0)).r;
            
            //col *= tex2D(image1, Rotate(polar, 3.1415 / 2) + float2(time, 0)).r;

            break;
            
        default:

            break;
    }
    
    
    //aura

    //col += tex2D(image2, uv).r;
    
    // clear the bg and only draw the shape at full brightness
    return float4(col, 10/t);
}
    
technique Technique1
{
    pass Blackhole
    {
        
        PixelShader = compile ps_3_0 Blackhole();
    }
}