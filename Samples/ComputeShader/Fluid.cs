﻿using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Fluid
    {
        const float EffectiveRadius = 6;
        const float ParticleRadius = 2;

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        public struct Particle
        {
            public Vector2F Current;
            public Vector2F Next;
            public Vector2F Velocity;
            [MarshalAs(UnmanagedType.R4)]
            public float Pscl;
            [MarshalAs(UnmanagedType.U1)]
            public bool IsFix;
        };

        public static float CalcRestDensity(float h)
        {
            var a = 4f / (MathF.PI * MathF.Pow(h, 8f));
            var r0 = 0.0f;
            var l = 2 * ParticleRadius;
            int n = (int)MathF.Ceiling(h / l) + 1;
            for (int x = -n; x <= n; ++x)
            {
                for (int y = -n; y <= n; ++y)
                {
                    Vector2F rij = new Vector2F(x * l, y * l);
                    var r = rij.Length;
                    if (r >= 0.0 && r <= h)
                    {
                        var q = h * h - r * r;
                        r0 += a * q * q * q;
                    }
                }
            }
            return r0;
        }

        class ParticleNode : CommandDrawnNode
        {
            public Material Material { get; set; }
            public Buffer<int> IndexBuffer { get; set; }
            public Buffer<Vertex> VertexBuffer { get; set; }

            protected override void Draw(CommandList commandList)
            {
                commandList.Material = Material;
                commandList.SetIndexBuffer(IndexBuffer);
                commandList.SetVertexBuffer(VertexBuffer);
                commandList.Draw(IndexBuffer.Count / 3);
            }
        }

        public static void Main(string[] args)
        {
            Engine.Initialize("Fluids Sim", 640, 480);
            Engine.FramerateMode = FramerateMode.Variable;
            Engine.TargetFPS = 240;
            var density = CalcRestDensity(EffectiveRadius);

            var particles = Buffer<Particle>.Create(BufferUsageType.Compute, 2048);
            {
                var data = particles.Lock();

                int ind = 0;
                for (int y = 0; y < 300 / (ParticleRadius * 2); y++)
                {
                    for (int x = 0; x < 300 / (ParticleRadius * 2); x++)
                    {
                        if (x * (ParticleRadius * 2) + ParticleRadius < 8 ||
                            x * (ParticleRadius * 2) + ParticleRadius > 300 - 8 ||
                            y * (ParticleRadius * 2) + ParticleRadius < 8 ||
                            y * (ParticleRadius * 2) + ParticleRadius > 300 - 8)
                        {
                            data[ind++] = new Particle()
                            {
                                Current = new Vector2F(x * (ParticleRadius * 2) + ParticleRadius, y * (ParticleRadius * 2) + ParticleRadius),
                                Next = new Vector2F(x * (ParticleRadius * 2) + ParticleRadius, y * (ParticleRadius * 2) + ParticleRadius),
                                Velocity = new Vector2F(0, 0),
                                Pscl = 0,
                                IsFix = true,
                            };
                        }
                    }
                }
                for (int i = ind; i < particles.Count; i++)
                {
                    data[i] = new Particle()
                    {
                        Current = new Vector2F(i % 50 * 4 + 10, i / 50 * 4 + 20),
                        Velocity = new Vector2F(0, 0),
                        Pscl = 0,
                    };
                }
                particles.Unlock();
            }

            var gridTable = Buffer<Vector2I>.Create(BufferUsageType.Compute, 2048);
            var gridIndicesTable = Buffer<Vector2I>.Create(BufferUsageType.Compute, 60 * 60);
            var particleComputeVertex = Buffer<Vertex>.Create(BufferUsageType.Compute, particles.Count * 4);
            var particleVertex = Buffer<Vertex>.Create(BufferUsageType.Vertex, particles.Count * 4);
            var particleComputeIndex = Buffer<int>.Create(BufferUsageType.Compute, particles.Count * 6);
            var particleIndex = Buffer<int>.Create(BufferUsageType.Index, particles.Count * 6);

            var csCalcExternalForces = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

cbuffer CB : register(b0)
{
    float4 Force;
    float4 Gravity;
    float4 GridNum;
    float4 GridSize;
};

RWStructuredBuffer<Particle> particles : register(u0);

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    if (particles[dtid.x].IsFix)
        return;
    particles[dtid.x].Velocity += (Gravity.xy + Force.xy) * (1.0 / 60);
    particles[dtid.x].Next = particles[dtid.x].Current + particles[dtid.x].Velocity * (1.0 / 60);
    particles[dtid.x].Next.x = clamp(particles[dtid.x].Next.x, 8, GridNum.x * GridSize.x - 8);
    particles[dtid.x].Next.y = clamp(particles[dtid.x].Next.y, 8, GridNum.y * GridSize.y - 8);
}
";

            var calcExternalPipeline = ComputePipelineState.Create();
            calcExternalPipeline.Shader = Shader.Create("csCalcExternalForces", csCalcExternalForces, ShaderStage.Compute);
            calcExternalPipeline.SetVector4F("Force", new Vector4F(0, 0, 0, 0));
            calcExternalPipeline.SetVector4F("Gravity", new Vector4F(0, 100, 0, 0));
            calcExternalPipeline.SetVector4F("GridNum", new Vector4F(60, 60, 0, 0));
            calcExternalPipeline.SetVector4F("GridSize", new Vector4F(5, 5, 0, 0));

            var csBuildGrid = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

cbuffer CB : register(b0)
{
    float4 GridNum;
    float4 GridSize;
    float4 ParticleNum;
};

RWStructuredBuffer<Particle> particles : register(u0);
RWStructuredBuffer<int2> gridTable : register(u1);

int GetGridHash(float2 pos)
{
    return (int)(pos.x / GridSize.x) + (int)(pos.y / GridSize.y) * (int)GridNum.x;
}

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    if (dtid.x < ParticleNum.x)
    {
        gridTable[dtid.x].x = GetGridHash(particles[dtid.x].Next);
        gridTable[dtid.x].y = dtid.x;
    }
    else
    {
        gridTable[dtid.x].x = 0xffffffff;
        gridTable[dtid.x].y = dtid.x;
    }
}
";

            var buildGridPipeline = ComputePipelineState.Create();
            buildGridPipeline.Shader = Shader.Create("csBuildGrid", csBuildGrid, ShaderStage.Compute);
            buildGridPipeline.SetVector4F("GridNum", new Vector4F(60, 60, 0, 0));
            buildGridPipeline.SetVector4F("GridSize", new Vector4F(5, 5, 0, 0));
            buildGridPipeline.SetVector4F("ParticleNum", new Vector4F(particles.Count, 0, 0, 0));

            var csBitonicSort = @"
cbuffer CB : register(b0)
{
    float4 Inc;
    float4 Dir;
};

RWStructuredBuffer<int2> gridTable : register(u0);

[numthreads(64, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    int inc = (int)Inc.x;
    int dir = (int)Dir.x;
    int t = dtid.x; // thread index
	int low = t & (inc - 1); // low order bits (below INC)
	int i = (t << 1) - low; // insert 0 at position INC
    bool reverse = ((dir & i) == 0);

	// Load
	int2 x0 = gridTable[i];
	int2 x1 = gridTable[inc + i];

	// Sort
	int2 auxa = x0;
	int2 auxb = x1;
	if ((x0.x < x1.x) ^ reverse) { x0 = auxb; x1 = auxa; }

	// Store
	gridTable[i] = x0;
	gridTable[inc + i] = x1;
}
";

            var bitonicSortPipeline = ComputePipelineState.Create();
            bitonicSortPipeline.Shader = Shader.Create("csBitonicSort", csBitonicSort, ShaderStage.Compute);

            var csClearGridIndices = @"
RWStructuredBuffer<int2> gridIndicesTable : register(u0);

[numthreads(64, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    gridIndicesTable[dtid.x] = int2(0xffffffff, 0xffffffff);
}
";

            var clearGridIndicesPipeline = ComputePipelineState.Create();
            clearGridIndicesPipeline.Shader = Shader.Create("clearGridIndicesPipeline", csClearGridIndices, ShaderStage.Compute);

            var csBuildGridIndices = @"
cbuffer CB : register(b0)
{
    float4 ParticleNum;
};

RWStructuredBuffer<int2> gridTable : register(u0);
RWStructuredBuffer<int2> gridIndicesTable : register(u1);

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    int id = dtid.x;
    int idPrev = (id == 0) ? (int)ParticleNum : id;
    idPrev--;
    int idNext = id + 1;
    if (idNext == (int)ParticleNum) { idNext = 0; }

    int cell = gridTable[id].x;
    int cellPrev = gridTable[idPrev].x;
    int cellNext = gridTable[idNext].x;

    if (cell != cellPrev)
    {
        gridIndicesTable[cell].x = id;
    }

    if (cell != cellNext)
    {
        gridIndicesTable[cell].y = id + 1;
    }
}
";

            var buildGridIndicesPipeline = ComputePipelineState.Create();
            buildGridIndicesPipeline.Shader = Shader.Create("csBuildGridIndices", csBuildGridIndices, ShaderStage.Compute);
            buildGridIndicesPipeline.SetVector4F("ParticleNum", new Vector4F(particles.Count, 0, 0, 0));

            var csCalcScalingFactor = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

cbuffer CB : register(b0)
{
    float4 GridNum;
    float4 GridSize;
    float4 EffectiveRadius;
    float4 Density;
    float4 Eps;
    float4 Dt;
    float4 Wpoly6;
    float4 GWspiky;
};

RWStructuredBuffer<Particle> particles : register(u0);
RWStructuredBuffer<int2> gridTable : register(u1);
RWStructuredBuffer<int2> gridIndicesTable : register(u2);

int GetGridHash(float2 pos)
{
    return (int)(pos.x / GridSize.x) + (int)(pos.y / GridSize.y) * (int)GridNum.x;
}

int2 GetGridPos(float2 pos)
{
    return int2((int)(pos.x / GridSize.x), (int)(pos.y / GridSize.x));
}

float2 CalcDensityCellPB(int2 gridPos, int i, float2 pos0)
{
    int gridHash = gridPos.x + gridPos.y * (int)GridNum.x;
    int startIndex = gridIndicesTable[gridHash].x;

    float h = EffectiveRadius.x;
	float dens = 0.0f;
	if(startIndex != 0xffffffff){	// セルが空でないかのチェック
		// セル内のパーティクルで反復
		int endIndex = gridIndicesTable[gridHash].y;
		for(int j = startIndex; j < endIndex; ++j){
			//if(j == i) continue;

			float2 pos1 = particles[gridTable[j].y].Next;

			float2 rij = pos0 - pos1;
			float r = length(rij);

			if(r <= h){
				float q = h * h - r * r;
				dens += Wpoly6.x * q * q * q;
			}
		}
	}

	return dens;
}

float CalcScalingFactorCell(int2 gridPos, int i, float2 pos0)
{
	int gridHash = gridPos.x + gridPos.y * (int)GridNum.x;
    int startIndex = gridIndicesTable[gridHash].x;

	float h = EffectiveRadius.x;
	float r0 = Density.x;
	float sd = 0.0f;
	float sd2 = 0.0f;
	if(startIndex != 0xffffffff){	// セルが空でないかのチェック
		// セル内のパーティクルで反復
		uint endIndex = gridIndicesTable[gridHash].y;
		for(uint j = startIndex; j < endIndex; ++j){

			float2 pos1 = particles[gridTable[j].y].Next;

			float2 rij = pos0-pos1;
			float r = length(rij);

			if(r <= h && r > 0.0){
				float q = h - r;

				// Spikyカーネルで位置変動を計算
				float2 dp = -(GWspiky.x * q * q * rij / r) / r0;
                sd2 -= dp;

			    if(gridTable[j].y != i)
				    sd += dot(dp, dp);
			}
		}
	}
	return sd + dot(sd2, sd2);
}

void CalcScalingFactor(int id)
{
	float2 pos = particles[id].Next;	// パーティクル位置
	float h = EffectiveRadius.x;
    float r0 = Density.x;

	// パーティクル周囲のグリッド
	int2 grid_pos0, grid_pos1;
	grid_pos0 = GetGridPos(pos-h);
	grid_pos1 = GetGridPos(pos+h);

	// 周囲のグリッドも含めて近傍探索，密度計算
	float dens = 0.0f;
	for(int y = grid_pos0.y; y <= grid_pos1.y; ++y){
		for(int x = grid_pos0.x; x <= grid_pos1.x; ++x){
			int2 n_grid_pos = int2(x, y);
			dens += CalcDensityCellPB(n_grid_pos, id, pos);
		}
	}
    
	// 密度拘束条件(式(1))
	float C = dens/r0-1.0;

	// 周囲のグリッドも含めて近傍探索，スケーリングファクタの分母項計算
	float sd = 0.0f;
	for(int y = grid_pos0.y; y <= grid_pos1.y; ++y){
		for(int x = grid_pos0.x; x <= grid_pos1.x; ++x){
			int2 n_grid_pos = int2(x, y);
			sd += CalcScalingFactorCell(n_grid_pos, id, pos);
		}
	}

	// スケーリングファクタの計算(式(11))
	particles[id].Pscl = -C/(sd+Eps);
}

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    int id = dtid.x;
    CalcScalingFactor(id);
}
";

            var calcScalingFactorPipeline = ComputePipelineState.Create();
            calcScalingFactorPipeline.Shader = Shader.Create("csCalcScalingFactor", csCalcScalingFactor, ShaderStage.Compute);
            calcScalingFactorPipeline.SetVector4F("GridNum", new Vector4F(60, 60, 0, 0));
            calcScalingFactorPipeline.SetVector4F("GridSize", new Vector4F(5, 5, 0, 0));
            calcScalingFactorPipeline.SetVector4F("EffectiveRadius", new Vector4F(EffectiveRadius, 0, 0, 0));
            calcScalingFactorPipeline.SetVector4F("Density", new Vector4F(density, 0, 0, 0));
            calcScalingFactorPipeline.SetVector4F("Eps", new Vector4F(0.1f, 0, 0, 0));
            calcScalingFactorPipeline.SetVector4F("Dt", new Vector4F(1.0f / 60, 0, 0, 0));
            calcScalingFactorPipeline.SetVector4F("Wpoly6", new Vector4F(4f / (MathF.PI * MathF.Pow(EffectiveRadius, 8f)), 0, 0, 0));
            calcScalingFactorPipeline.SetVector4F("GWspiky", new Vector4F(-30f / (MathF.PI * MathF.Pow(EffectiveRadius, 5f)), 0, 0, 0));

            var csCalcCorrectPosition = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

cbuffer CB : register(b0)
{
    float4 GridNum;
    float4 GridSize;
    float4 EffectiveRadius;
    float4 Density;
    float4 Eps;
    float4 Dt;
    float4 Wpoly6;
    float4 GWspiky;
};

RWStructuredBuffer<Particle> particles : register(u0);
RWStructuredBuffer<int2> gridTable : register(u1);
RWStructuredBuffer<int2> gridIndicesTable : register(u2);

int GetGridHash(float2 pos)
{
    return (int)(pos.x / GridSize.x) + (int)(pos.y / GridSize.y) * (int)GridNum.x;
}

int2 GetGridPos(float2 pos)
{
    return int2((int)(pos.x / GridSize.x), (int)(pos.y / GridSize.x));
}

float2 CalcPositionCorrectionCell(int2 gridPos, int i, float2 pos0)
{
	int gridHash = gridPos.x + gridPos.y * (int)GridNum.x;
    int startIndex = gridIndicesTable[gridHash].x;

	float h = EffectiveRadius.x;
	float r0 = Density.x;
	float2 dp = float2(0.0);

	float dt = Dt.x;

	float si = particles[i].Pscl;

	if(startIndex != 0xffffffff){	// セルが空でないかのチェック
		// セル内のパーティクルで反復
		uint endIndex = gridIndicesTable[gridHash].y;
		for(uint j = startIndex; j < endIndex; ++j){
			if(gridTable[j].y == i) continue;

			float2 pos1 = particles[gridTable[j].y].Next;

			float2 rij = pos0 - pos1;
			float r = length(rij);

			if(r <= h && r > 0.0){
                float scorr = 0;
                {
				    float q = h * h - r * r;
				    float q2 = h * h - 0.04 * h * h;

                    float ww = Wpoly6.x * q * q * q / (Wpoly6.x * q2 * q2 * q2);
                    scorr = -0.1 * pow(ww, 4) * dt * dt;
                }

                {
				    float q = h - r;
				    float sj = particles[gridTable[j].y].Pscl;

				    // Spikyカーネルで位置修正量を計算
				    dp += (si + sj + scorr) * (GWspiky.x * q * q * rij / r) / r0;
                }
			}
		}
	}

	return dp;
}

float2 CalcPositionCorrection(int id)
{
	float2 pos = particles[id].Next;	// パーティクル位置
	float h = EffectiveRadius.x;

	// パーティクル周囲のグリッド
	int2 grid_pos0, grid_pos1;
	grid_pos0 = GetGridPos(pos - h);
	grid_pos1 = GetGridPos(pos + h);

	// 周囲のグリッドも含めて近傍探索，位置修正量を計算
	float2 dpij = float2(0.0f);
	for(int y = grid_pos0.y; y <= grid_pos1.y; ++y){
		for(int x = grid_pos0.x; x <= grid_pos1.x; ++x){
			int2 n_grid_pos = int2(x, y);
			dpij += CalcPositionCorrectionCell(n_grid_pos, id, pos);
	    }
	}

    return dpij;
}

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    if (particles[dtid.x].IsFix)
        return;
    int id = dtid.x;
    particles[id].Next += CalcPositionCorrection(id);
}
";

            var calcCorrectPositionPipeline = ComputePipelineState.Create();
            calcCorrectPositionPipeline.Shader = Shader.Create("csCalcCorrectPosition", csCalcCorrectPosition, ShaderStage.Compute);
            calcCorrectPositionPipeline.SetVector4F("GridNum", new Vector4F(60, 60, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("GridSize", new Vector4F(5, 5, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("EffectiveRadius", new Vector4F(EffectiveRadius, 0, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("Density", new Vector4F(density, 0, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("Eps", new Vector4F(0.1f, 0, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("Dt", new Vector4F(1.0f / 60, 0, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("Wpoly6", new Vector4F(4f / (MathF.PI * MathF.Pow(EffectiveRadius, 8f)), 0, 0, 0));
            calcCorrectPositionPipeline.SetVector4F("GWspiky", new Vector4F(-30f / (MathF.PI * MathF.Pow(EffectiveRadius, 5f)), 0, 0, 0));

            var csIntegrate = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

cbuffer CB : register(b0)
{
    float4 Dt;
};

RWStructuredBuffer<Particle> particles : register(u0);

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    particles[dtid.x].Velocity = (particles[dtid.x].Next - particles[dtid.x].Current) * (1.0 / Dt.x);
    particles[dtid.x].Current = particles[dtid.x].Next;
}
";

            var integratePipeline = ComputePipelineState.Create();
            integratePipeline.Shader = Shader.Create("csIntegrate", csIntegrate, ShaderStage.Compute);
            integratePipeline.SetVector4F("Dt", new Vector4F(1.0f / 60, 0, 0, 0));

            var csBuildVBIB = @"
struct Particle
{
    float2 Current;
    float2 Next;
    float2 Velocity;
    float Pscl;
    bool IsFix;
};

struct Vertex{
    float3 Position;
    float3 Normal;
    int Color;
    float2 UV1;
    float2 UV2;
};

cbuffer CB : register(b0)
{
    float4 ParticleRadius;
    float4 Color;
};

RWStructuredBuffer<Particle> particles : register(u0);
RWStructuredBuffer<Vertex> vertex : register(u1);
RWStructuredBuffer<int> index : register(u2);

int DecodeFloatRGBA(float4 rgba) {
    int res = 0;
    res += (int)(rgba.a * 255);
    res = res << 8;
    res += (int)(rgba.b * 255);
    res = res << 8;
    res += (int)(rgba.g * 255);
    res = res << 8;
    res += (int)(rgba.r * 255);
    return res;
}

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
    for (int i = 0; i < 4; i++)
    {
        float4 c = Color;
        if (particles[dtid.x].IsFix)
            c.a = 0;
        vertex[dtid.x * 4 + i].Color = DecodeFloatRGBA(c);
    }

    vertex[dtid.x * 4].Position = float3(particles[dtid.x].Current, 0.5) + float3(-1, -1, 0) * ParticleRadius.x * 3;
    vertex[dtid.x * 4 + 1].Position = float3(particles[dtid.x].Current, 0.5) + float3(1, -1, 0) * ParticleRadius.x * 3;
    vertex[dtid.x * 4 + 2].Position = float3(particles[dtid.x].Current, 0.5) + float3(1, 1, 0) * ParticleRadius.x * 3;
    vertex[dtid.x * 4 + 3].Position = float3(particles[dtid.x].Current, 0.5) + float3(-1, 1, 0) * ParticleRadius.x * 3;
    vertex[dtid.x * 4].UV1 = float2(0, 0);
    vertex[dtid.x * 4 + 1].UV1 = float2(1, 0);
    vertex[dtid.x * 4 + 2].UV1 = float2(1, 1);
    vertex[dtid.x * 4 + 3].UV1 = float2(0, 1);
    
    index[dtid.x * 6] = dtid.x * 4;
    index[dtid.x * 6 + 1] = dtid.x * 4 + 1;
    index[dtid.x * 6 + 2] = dtid.x * 4 + 2;
    index[dtid.x * 6 + 3] = dtid.x * 4;
    index[dtid.x * 6 + 4] = dtid.x * 4 + 2;
    index[dtid.x * 6 + 5] = dtid.x * 4 + 3;
}
";

            var buildVBIB = ComputePipelineState.Create();
            buildVBIB.Shader = Shader.Create("csIntegrate", csBuildVBIB, ShaderStage.Compute);
            buildVBIB.SetVector4F("ParticleRadius", new Vector4F(ParticleRadius, 0, 0, 0));
            buildVBIB.SetVector4F("Color", new Vector4F(0.2f, 0.2f, 1f, 1f));

            var renderTexture = RenderTexture.Create(new Vector2I(300, 300), TextureFormat.R8G8B8A8_UNORM, false);

            var camera = new CameraNode()
            {
                Group = 1,
                IsColorCleared = true,
                ClearColor = new Color(0, 0, 0, 0),
                TargetTexture = renderTexture,
            };
            Engine.AddNode(camera);

            var camera2 = new CameraNode()
            {
                IsColorCleared = true,
                Group = 2,
            };
            Engine.AddNode(camera2);

            var particlesNode = new ParticleNode();
            particlesNode.Material = Material.Create();
            var blend = AlphaBlend.Add;
            blend.BlendEquationRGB = BlendEquation.Max;
            particlesNode.Material.AlphaBlend = blend;
            particlesNode.CameraGroup = 1;

            var psCode = @"
struct PS_INPUT
{
    float4  Position : SV_POSITION;
    float4  Normal : NORMAL0;
    float4  Color    : COLOR0;
    float2  UV1 : UV0;
    float2  UV2 : UV1;
};
float4 main(PS_INPUT input) : SV_TARGET
{
    float4 c;
    float r = length((input.UV1 - 0.5) * 2);
    float a = r > 1 ? 0 : (-4 / 9 * pow(r, 6) + 17 / 9 * pow(r, 4) - 22 / 9 * pow(r, 2) + 1);
    c = float4(input.Color.rgb, input.Color.a * a);
    return c;
}";
            particlesNode.Material.SetShader(Shader.Create("ps", psCode, ShaderStage.Pixel));
            particlesNode.VertexBuffer = particleVertex;
            particlesNode.IndexBuffer = particleIndex;
            Engine.AddNode(particlesNode);

            var finalizeNode = new SpriteNode()
            {
                Texture = renderTexture,
                Position = Engine.WindowSize / 2 - renderTexture.Size / 2,
                CameraGroup = 2,
            };
            finalizeNode.Material = Material.Create();
            var psCode2 = @"
Texture2D mainTex : register(t0);
SamplerState mainSamp : register(s0);
struct PS_INPUT
{
    float4  Position : SV_POSITION;
    float4  Color    : COLOR0;
    float4  Normal : NORMAL0;
    float2  UV1 : UV0;
    float2  UV2 : UV1;
};
float4 main(PS_INPUT input) : SV_TARGET
{
    float4 c;
    if (mainTex.Sample(mainSamp, input.UV1).a < 0.5)
        discard;
    c = float4(mainTex.Sample(mainSamp, input.UV1).rgb, 1);
    return c;
}";
            finalizeNode.Material.SetShader(Shader.Create("ps2", psCode2, ShaderStage.Pixel));
            finalizeNode.Material.SetTexture("mainTex", renderTexture);
            Engine.AddNode(finalizeNode);

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            var text = new TextNode() { Font = font, FontSize = 30, Text = "", ZOrder = 10, CameraGroup = 2 };
            Engine.AddNode(text);

            Engine.Graphics.CommandList.Begin();
            Engine.Graphics.CommandList.UploadBuffer(particles);
            Engine.Graphics.CommandList.End();
            Engine.Graphics.ExecuteCommandList();
            Engine.Graphics.WaitFinish();

            bool step = false;
            while (Engine.DoEvents())
            {
                Engine.Graphics.CommandList.Begin();

                Engine.Graphics.CommandList.BeginComputePass();

                if (step)
                {
                    Engine.Graphics.CommandList.ComputePipelineState = calcExternalPipeline;
                    Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                    Engine.Graphics.CommandList.Dispatch(particles.Count, 1, 1);

                    for (int l = 0; l < 8; l++)
                    {
                        Engine.Graphics.CommandList.ComputePipelineState = buildGridPipeline;
                        Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridTable, 1);
                        Engine.Graphics.CommandList.Dispatch(gridTable.Count, 1, 1);

                        Engine.Graphics.CommandList.SetComputeBuffer(gridTable, 0);

                        int nlog = (int)MathF.Log(gridTable.Count, 2);
                        int inc;

                        for (int i = 0; i < nlog; i++)
                        {
                            inc = 1 << i;
                            for (int j = 0; j < i + 1; j++)
                            {
                                bitonicSortPipeline.SetVector4F("Dir", new Vector4F(2 << i, 0, 0, 0));
                                bitonicSortPipeline.SetVector4F("Inc", new Vector4F(inc, 0, 0, 0));
                                Engine.Graphics.CommandList.ComputePipelineState = bitonicSortPipeline;
                                Engine.Graphics.CommandList.Dispatch(gridTable.Count / 2 / 64, 1, 1);
                                inc /= 2;
                            }
                        }

                        Engine.Graphics.CommandList.ComputePipelineState = clearGridIndicesPipeline;
                        Engine.Graphics.CommandList.SetComputeBuffer(gridIndicesTable, 0);
                        Engine.Graphics.CommandList.Dispatch(gridIndicesTable.Count, 1, 1);

                        Engine.Graphics.CommandList.ComputePipelineState = buildGridIndicesPipeline;
                        Engine.Graphics.CommandList.SetComputeBuffer(gridTable, 0);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridIndicesTable, 1);
                        Engine.Graphics.CommandList.Dispatch(gridTable.Count, 1, 1);

                        Engine.Graphics.CommandList.ComputePipelineState = calcScalingFactorPipeline;
                        Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridTable, 1);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridIndicesTable, 2);
                        Engine.Graphics.CommandList.Dispatch(particles.Count, 1, 1);

                        Engine.Graphics.CommandList.ComputePipelineState = calcCorrectPositionPipeline;
                        Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridTable, 1);
                        Engine.Graphics.CommandList.SetComputeBuffer(gridIndicesTable, 2);
                        Engine.Graphics.CommandList.Dispatch(particles.Count, 1, 1);
                    }

                    Engine.Graphics.CommandList.ComputePipelineState = integratePipeline;
                    Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                    Engine.Graphics.CommandList.Dispatch(particles.Count, 1, 1);
                }

                Engine.Graphics.CommandList.ComputePipelineState = buildVBIB;
                Engine.Graphics.CommandList.SetComputeBuffer(particles, 0);
                Engine.Graphics.CommandList.SetComputeBuffer(particleComputeVertex, 1);
                Engine.Graphics.CommandList.SetComputeBuffer(particleComputeIndex, 2);
                Engine.Graphics.CommandList.Dispatch(particles.Count, 1, 1);

                Engine.Graphics.CommandList.EndComputePass();

                Engine.Graphics.CommandList.CopyBuffer(particleComputeIndex, particleIndex);
                Engine.Graphics.CommandList.CopyBuffer(particleComputeVertex, particleVertex);

                Engine.Graphics.CommandList.End();
                Engine.Graphics.ExecuteCommandList();
                Engine.Graphics.WaitFinish();

                text.Text = Engine.CurrentFPS.ToString();

                if (Engine.Keyboard.GetKeyState(Key.Space) == ButtonState.Push)
                {
                    renderTexture.Save("aa.png");
                    step = !step;
                }

                Engine.Update();
            }

            Engine.Terminate();
        }
    }
}
