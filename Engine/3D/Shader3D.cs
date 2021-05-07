using System;
using System.Collections.Generic;
using System.Text;

namespace Altseed2
{
    /// <summary>
    /// ディレクショナルライティングノード
    /// </summary>
    public class DirectionalLightingNode : Node
    {
        const string DirectionalLightingVS = @"
cbuffer Consts : register(b0)
{
    float4x4 matView;
    float4x4 matProjection;
    float4x4 transformInv;
    float4 lightDirection;
};

struct VS_INPUT{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR0;
    float2 UV1 : UV0;
    float2 UV2 : UV1;
};
struct VS_OUTPUT{
    float4  Position : SV_POSITION;
    float4  Normal : NORMAL0;
    float4  Color    : COLOR0;
    float2  UV1 : UV0;
    float2  UV2 : UV1;
};
    
VS_OUTPUT main(VS_INPUT input){
    VS_OUTPUT output;
    float4 pos = float4(input.Position, 1.0f);

    pos = mul(matView, pos);
    pos = mul(matProjection, pos);

    output.Position = pos;
    output.Normal = float4(input.Normal, 1.0f);
    output.UV1 = input.UV1;
    output.UV2 = input.UV2;

    float3 invLight = normalize(transformInv * -lightDirection).xyz;
    float diffuse = clamp(dot(output.Normal.xyz, invLight), 0.1f, 1.0f);
    output.Color = input.Color * float4(float3(diffuse), 1.0);
        
    return output;
}
";

        /// <summary>
        /// マテリアル
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// 光の方向
        /// </summary>
        public Vector3F LightDirection
        {
            get
            {
                var vec4 = Material.GetVector4F("lightDirection");
                return new Vector3F(vec4.X, vec4.Y, vec4.Z);
            }
            set => Material.SetVector4F("lightDirection", new Vector4F(value.X, value.Y, value.Z, 0).Normal);
        }

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        public DirectionalLightingNode()
        {
            Material = Material.Create();
            Material.Shader = Shader.Create("DirectionalLighting", DirectionalLightingVS, ShaderStage.Vertex);
            Material.SetVector4F("lightDirection", new Vector4F(-1, -1, 0, 0).Normal);
            Material.SetMatrix44F("transformInv", default);
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            if (Parent is Polygon3DNode polygon)
            {
                polygon.Material = Material;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Parent is Polygon3DNode polygon)
            {
                Material.SetMatrix44F("transformInv", polygon.AbsoluteTransform);
            }
        }
    }
}
