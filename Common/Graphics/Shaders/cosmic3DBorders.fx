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
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 TexCoord : TEXCOORD0;
};
 
struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 TexCoord : TEXCOORD0;
};
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = mul(viewWorldProjection3D, input.Position);
    output.TexCoord = mul(viewWorldProjection3D, input.TexCoord);
    return output;
}

float3 Rotate(float3 p, float3 axis, float angle)
{
    return lerp(dot(axis, p) * axis, p, cos(angle)) + cross(axis, p) * sin(angle);
}

float2 SkyBoxUV(in float x,in float y,in float z, out int index)
{

    float absX = abs(x);
    float absY = abs(y);
    float absZ = abs(z);

    int isXPositive = x > 0 ? 1 : 0;
    int isYPositive = y > 0 ? 1 : 0;
    int isZPositive = z > 0 ? 1 : 0;

    float maxAxis, uc, vc;
    if (isXPositive && absX >= absY && absX >= absZ)
    {
        maxAxis = absX;
        uc = -z;
        vc = y;
        index = 0;
    }
    if (!isXPositive && absX >= absY && absX >= absZ)
    {
        maxAxis = absX;
        uc = z;
        vc = y;
        index = 1;
    }
    if (isYPositive && absY >= absX && absY >= absZ)
    {
        maxAxis = absY;
        uc = x;
        vc = -z;
        index = 2;
    }
    if (!isYPositive && absY >= absX && absY >= absZ)
    {
        maxAxis = absY;
        uc = x;
        vc = z;
        index = 3;
    }
    if (isZPositive && absZ >= absX && absZ >= absY)
    {
        maxAxis = absZ;
        uc = x;
        vc = y;
        index = 4;
    }
    if (!isZPositive && absZ >= absX && absZ >= absY)
    {
        maxAxis = absZ;
        uc = -x;
        vc = y;
        index = 5;
    }
    float u = 0.5f * (uc / maxAxis + 1.0f);
    float v = 0.5f * (vc / maxAxis + 1.0f);
    return float2(u, v);
}



float4 CosmicBoarder(VertexShaderOutput input) : COLOR0
{
    float textureWH = 1024;
    float3 uv = input.TexCoord;
    int index = 0;
    float3 rd = float3(uv.x, uv.y, uv.z / textureWH);
    rd = normalize(rd);


    float2 skyBoxUV = SkyBoxUV(rd.x, rd.y, rd.z, index);
    float4 finalCol = 0;
    switch (index)
    {
        
        
        case 0:
            finalCol = tex2D(image2, skyBoxUV);
            break;
        case 1:
            finalCol = tex2D(image4, skyBoxUV);
            break;
        case 2:
            finalCol = tex2D(image6, skyBoxUV);
            break;
        case 3:
            finalCol = tex2D(image5, skyBoxUV);
            break;
        case 4:
            finalCol = tex2D(image3, skyBoxUV);
            break;
        case 5:
            finalCol = tex2D(image1, skyBoxUV);
            break;

    }
    
    return finalCol;
    
}
    
technique Technique1
{
    pass CosmicBoarder
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 CosmicBoarder();
    }
}