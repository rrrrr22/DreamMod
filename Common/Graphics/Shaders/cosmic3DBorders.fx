#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);
sampler2D image4 : register(s4);
sampler2D image5 : register(s5);
sampler2D image6 : register(s6);

float4x4 viewWorldProjection3D;
float4x4 viewWorldProjection;
float time;
float4 shaderData;
float3 colors[3];
float2 screenPosition;
float2 screenSize;
float2 screenCenter;


float3 Rotate(float3 p, float3 axis, float angle)
{
    return lerp(dot(axis, p) * axis, p, cos(angle)) + cross(axis, p) * sin(angle);
}



float4 CosmicBoarder(float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords + screenPosition / 500 / 16 / 4;
    float4 finalCol = tex2D(image1, uv);
    float2 gridUV = frac((uv + float2(0.,time * 0.1)) * 8)
    -.5;
    float value = gridUV.x > 0.45 && gridUV.x < 0.5 || gridUV.y > 0.45 && gridUV.y < 0.5 ? 1 : 0;
    float grad = coords.y;
    finalCol += float4(colors[1] * value * grad + smoothstep(0,2,grad), value * grad);
    
    
    return finalCol;
}
    
technique Technique1
{
    pass CosmicBoarder
    {
        PixelShader = compile ps_3_0 CosmicBoarder();
    }
}