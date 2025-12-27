#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);
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
texture skyboxTexture;
samplerCUBE skyboxSampler = sampler_state
{
    Texture = <skyboxTexture>;
    MAGFILTER = LINEAR;
    MINFILTER = ANISOTROPIC;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
};

float4 PS(float2 coords : TEXCOORD0) : COLOR0
{
    return float4(0,0,0,1) +
    texCUBE(skyboxSampler, float3(coords,1));
}

technique t0
{
    pass skybox
    {
        PixelShader = compile ps_3_0 PS();
    }
}