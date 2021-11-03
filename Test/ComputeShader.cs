using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Altseed2.Test
{
    [TestFixture]
    class ComputeShader
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct InputData
        {
            public float value1;
            public float value2;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct OutputData
        {
            public float value;
        };

        [Test, Apartment(ApartmentState.STA)]
        public void ComputeShaderBasic()
        {
            var tc = new TestCore();
            tc.Init();

            string csCode1 = @"
struct CS_INPUT{
    float value1;
    float value2;
};

struct CS_OUTPUT{
    float value;
};

cbuffer CB : register(b0)
{
  float4 offset;
};

RWStructuredBuffer<CS_INPUT> read : register(u0);
RWStructuredBuffer<CS_OUTPUT> write : register(u1);

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
   write[dtid.x].value = read[dtid.x].value1 * read[dtid.x].value2 + offset.x;
}
";

            string csCode2 = @"
struct CS_INPUT{
    float value1;
    float value2;
};

struct CS_OUTPUT{
    float value;
};

cbuffer CB : register(b0)
{
  float4 offset;
};

RWStructuredBuffer<CS_INPUT> read : register(u0);
RWStructuredBuffer<CS_OUTPUT> write : register(u1);

[numthreads(1, 1, 1)]
void main(uint3 dtid : SV_DispatchThreadID)
{
   write[dtid.x].value = read[dtid.x].value1 * read[dtid.x].value2 * offset.x;
}
";

            var pip1 = ComputePipelineState.Create();
            pip1.Shader = Shader.Create("cs1", csCode1, ShaderStage.Compute);
            int offset = 100;
            pip1.SetVector4F("offset", new Vector4F(offset, 0, 0, 0));

            var pip2 = ComputePipelineState.Create();
            pip2.Shader = Shader.Create("cs2", csCode2, ShaderStage.Compute);
            pip2.SetVector4F("offset", new Vector4F(offset, 0, 0, 0));

            int dataSize = 256;

            var read = Buffer<InputData>.Create(BufferUsageType.Compute, dataSize);
            {
                var data = read.Lock();
                for (int i = 0; i < read.Count; i++)
                {
                    data[i] = new InputData()
                    {
                        value1 = i * 2,
                        value2 = i * 2 + 1,
                    };
                }
                read.Unlock();
            }

            var write1 = Buffer<OutputData>.Create(BufferUsageType.Compute, dataSize);
            var write2 = Buffer<OutputData>.Create(BufferUsageType.Compute, dataSize);

            Engine.Graphics.CommandList.Begin();
            Engine.Graphics.CommandList.UploadBuffer(read);

            Engine.Graphics.CommandList.BeginComputePass();

            Engine.Graphics.CommandList.SetComputeBuffer(read, 0);
            Engine.Graphics.CommandList.SetComputeBuffer(write1, 1);
            Engine.Graphics.CommandList.ComputePipelineState = pip1;
            Engine.Graphics.CommandList.Dispatch(dataSize, 1, 1);

            Engine.Graphics.CommandList.SetComputeBuffer(read, 0);
            Engine.Graphics.CommandList.SetComputeBuffer(write2, 1);
            Engine.Graphics.CommandList.ComputePipelineState = pip2;
            Engine.Graphics.CommandList.Dispatch(dataSize, 1, 1);

            Engine.Graphics.CommandList.EndComputePass();

            Engine.Graphics.CommandList.ReadbackBuffer(read);
            Engine.Graphics.CommandList.ReadbackBuffer(write1);
            Engine.Graphics.CommandList.ReadbackBuffer(write2);

            Engine.Graphics.CommandList.End();
            Engine.Graphics.ExecuteCommandList();
            Engine.Graphics.WaitFinish();


            var readValues = read.Read();
            var write1Values = write1.Read();
            var write2Values = write2.Read();

            for (int i = 0; i < readValues.Length; i++)
            {
                Assert.AreEqual(write1Values[i].value, readValues[i].value1 * readValues[i].value2 + offset);
                Assert.AreEqual(write2Values[i].value, readValues[i].value1 * readValues[i].value2 * offset);
            }

            tc.End();
        }
    }
}
