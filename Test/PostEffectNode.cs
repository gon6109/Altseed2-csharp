﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Altseed2.Test
{
    [TestFixture]
    class PostEffectNode
    {
        private class TestPostEffect : Altseed2.PostEffectNode
        {

            const string Code = @"
Texture2D mainTex : register(t0);
    SamplerState mainSamp : register(s0);
    cbuffer Consts : register(b1)
    {
        float4 time;
    };
    struct PS_INPUT
    {
        float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 UV1 : UV0;
    float2 UV2 : UV1;
};
    float4 main(PS_INPUT input) : SV_TARGET 
{ 
    if (input.UV1.x > 0.5) {
        return float4(input.UV1, 1.0, 1.0);
}

float x = frac(input.UV1.x + time.x * 0.5 - floor(input.UV1.y * 10) * 0.1);

float4 tex = mainTex.Sample(mainSamp, float2(x, input.UV1.y));
    
    return float4(tex.xyz, 1.0);
}
";
            Material material = Material.Create();

            int count = 0;

            protected override void OnAdded()
            {
                var ps = Shader.Create("postEffectTest", Code, ShaderStage.Pixel);

                material.SetShader(ps);
            }

            protected override void Draw(RenderTexture src, Color clearColor)
            {
                material.SetTexture("mainTex", src);
                count = (count + 1) % 200;
                material.SetVector4F("time", new Vector4F(count / 200.0f, 0.0f, 0.0f, 0.0f));

                Engine.Graphics.CommandList.RenderToRenderTarget(material);
            }
        }


        [Test, Apartment(ApartmentState.STA)]
        public void PostEffect()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Texture = texture,
                ZOrder = 1
            });

            var postEffect = new TestPostEffect()
            {
                ZOrder = 2
            };

            Engine.AddNode(postEffect);

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void GrayScale()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Texture = texture,
                ZOrder = 1
            });

            Engine.AddNode(new PostEffectGrayScaleNode()
            {
                ZOrder = 2
            });

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Sepia()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Texture = texture,
                ZOrder = 1
            });

            Engine.AddNode(new PostEffectSepiaNode()
            {
                ZOrder = 2
            });

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void GaussianBlur()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Texture = texture,
                ZOrder = 1
            });

            Engine.AddNode(new PostEffectGaussianBlurNode()
            {
                ZOrder = 2
            });

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void LightBloom()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Texture = texture,
                ZOrder = 1
            });

            Engine.AddNode(new PostEffectLightBloomNode
            {
                Threshold = 0.1f,
                ZOrder = 2
            });

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void PostEffectWithCamera()
        {
            var tc = new TestCore();
            tc.Init();

            const ulong cameraGroup1 = 0b01;
            const ulong cameraGroup2 = 0b10;

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            Engine.AddNode(new SpriteNode()
            {
                Scale = new Vector2F(1.5f, 1.5f),
                Texture = texture,
                ZOrder = 1,
                CameraGroup = cameraGroup1 | cameraGroup2,
            });

            Engine.AddNode(new PostEffectGaussianBlurNode()
            {
                ZOrder = 2,
                CameraGroup = cameraGroup2,
            });

            var target = RenderTexture.Create((new Vector2F(0.5f, 1.0f) * Engine.Window.Size.To2F()).To2I(), TextureFormat.R8G8B8A8_UNORM, false);

            Engine.AddNode(new CameraNode() { Group = cameraGroup2, TargetTexture = target, IsColorCleared = true, ClearColor = new Color(80, 80, 80) });

            // 表示用
            Engine.AddNode(new CameraNode() { Group = cameraGroup1 });
            Engine.AddNode(new SpriteNode() { Texture = target, CameraGroup = cameraGroup1 });

            tc.LoopBody(c =>
            {

            }, null);

            tc.End();
        }
    }
}
