#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);
samplerCUBE skybox : register(s4);
float4x4 viewWorldProjection;
float4x4 projection;
float4x4 view;
float4x4 world;
float time;
float4 shaderData;
float3 colors[3];
float2 screenPosition;
float2 screenSize;
float2 screenCenter;
float2 vertexRectSize;



float4 PS(float3 normal : TEXCOORD0) : COLOR0
{
    
    return texCUBE(skybox, float3(normal.x + screenPosition.x / 500 / 16, normal.y + screenPosition.y / 500 / 16, 1));
}

technique t0
{
    pass skybox
    {
        PixelShader = compile ps_3_0 PS();
    }
}