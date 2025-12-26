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
    Texture = (skyboxSampler);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};
 
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 TextureCoordinate : TEXCOORD0;
};
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;
 
    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);
    output.Position = mul(viewPosition, projection);
 
    float4 VertexPosition = mul(input.Position, world);
    output.TextureCoordinate = VertexPosition - float4(screenPosition, 0, 0);
 
    return output;
}

float4 PS(VertexShaderOutput input) : COLOR0
{
    return texCUBE(skyboxSampler, normalize(input.TextureCoordinate));
}

technique t0
{
    pass skybox
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}