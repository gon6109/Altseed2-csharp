﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Altseed2.Test
{
    [TestFixture]
    class DrawnNode
    {
        [Test, Apartment(ApartmentState.STA)]
        public void SpriteNode()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            //node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            //node.Pivot = new Vector2F(0.5f, 0.5f);
            //node.AdjustSize();
            Engine.AddNode(node);

            tc.LoopBody(c =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) node.Position += new Vector2F(1.5f, 0);
                if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) node.Position -= new Vector2F(1.5f, 0);
                if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) node.Position += new Vector2F(0, 1.5f);
                if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) node.Position -= new Vector2F(0, 1.5f);
                if (Engine.Keyboard.GetKeyState(Key.B) == ButtonState.Hold) node.Scale += new Vector2F(0.01f, 0.01f);
                if (Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Hold) node.Scale -= new Vector2F(0.01f, 0.01f);
                if (Engine.Keyboard.GetKeyState(Key.R) == ButtonState.Hold) node.Angle += 3;
                if (Engine.Keyboard.GetKeyState(Key.L) == ButtonState.Hold) node.Angle -= 3;
                if (Engine.Keyboard.GetKeyState(Key.X) == ButtonState.Hold) node.Src = new RectF(node.Src.X, node.Src.Y, node.Src.Width - 2.0f, node.Src.Height - 2.0f);
                if (Engine.Keyboard.GetKeyState(Key.Z) == ButtonState.Hold) node.Src = new RectF(node.Src.X, node.Src.Y, node.Src.Width + 2.0f, node.Src.Height + 2.0f);
                if (Engine.Keyboard.GetKeyState(Key.C) == ButtonState.Hold) node.Src = new RectF(node.Src.X - 2.0f, node.Src.Y - 2.0f, node.Src.Width, node.Src.Height);
                if (Engine.Keyboard.GetKeyState(Key.V) == ButtonState.Hold) node.Src = new RectF(node.Src.X + 2.0f, node.Src.Y + 2.0f, node.Src.Width, node.Src.Height);
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Sprite3DNode()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new Sprite3DNode();
            node.Texture = texture;
            //node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            //node.Pivot = new Vector2F(0.5f, 0.5f);
            //node.AdjustSize();
            node.Quaternion = Quaternion.Euler(new Vector3F(0, 45, 45));
            Engine.AddNode(node);

            tc.LoopBody(c =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) node.Position += new Vector3F(0.15f, 0, 0);
                if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) node.Position -= new Vector3F(0.15f, 0, 0);
                if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) node.Position += new Vector3F(0, 0, 0.15f);
                if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) node.Position -= new Vector3F(0, 0, 0.15f);
                if (Engine.Keyboard.GetKeyState(Key.B) == ButtonState.Hold) node.Scale += new Vector3F(0.01f, 0.01f, 0);
                if (Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Hold) node.Scale -= new Vector3F(0.01f, 0.01f, 0);
                if (Engine.Keyboard.GetKeyState(Key.X) == ButtonState.Hold) node.Src = new RectF(node.Src.X, node.Src.Y, node.Src.Width - 2.0f, node.Src.Height - 2.0f);
                if (Engine.Keyboard.GetKeyState(Key.Z) == ButtonState.Hold) node.Src = new RectF(node.Src.X, node.Src.Y, node.Src.Width + 2.0f, node.Src.Height + 2.0f);
                if (Engine.Keyboard.GetKeyState(Key.C) == ButtonState.Hold) node.Src = new RectF(node.Src.X - 2.0f, node.Src.Y - 2.0f, node.Src.Width, node.Src.Height);
                if (Engine.Keyboard.GetKeyState(Key.V) == ButtonState.Hold) node.Src = new RectF(node.Src.X + 2.0f, node.Src.Y + 2.0f, node.Src.Width, node.Src.Height);
            }
            , null);

            tc.End();
        }


        [Test, Apartment(ApartmentState.STA)]
        public void Text3DNode()
        {
            var tc = new TestCore();
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            var font2 = Font.LoadDynamicFont("TestData/Font/GenYoMinJP-Bold.ttf", 64);
            Assert.NotNull(font);
            Assert.NotNull(font2);
            var imageFont = Font.CreateImageFont(font);
            imageFont.AddImageGlyph('〇', Texture2D.Load(@"TestData/IO/AltseedPink.png"));

            Engine.AddNode(new Text3DNode() { Font = font, FontSize = 80, Text = "Hello, world! こんにちは" });
            Engine.AddNode(new Text3DNode() { Font = font, FontSize = 80, Text = "色を指定します。", Position = new Vector3F(0.0f, -1.0f, 0.5f), Color = new Color(0, 0, 255) });
            Engine.AddNode(new Text3DNode() { Font = font2, FontSize = 80, Text = "𠀋 𡈽 𡌛 𡑮 𡢽 𠮟 𡚴 𡸴 𣇄 𣗄 𣜿 𣝣 𣳾", Position = new Vector3F(0.0f, 1.0f, -1.0f) });
            Engine.AddNode(new Text3DNode() { Font = imageFont, FontSize = 80, Text = "Altseed〇Altseed", Position = new Vector3F(0.0f, 1.0f, 1.0f) });
            var rotated = new Text3DNode() { Font = font, FontSize = 80, Text = "回転します。", Position = new Vector3F(-2.0f, -2.0f, 0.1f) };
            Engine.AddNode(rotated);

            tc.LoopBody(c =>
            {
                rotated.Quaternion = Quaternion.Euler(rotated.Quaternion.EulerAngles + new Vector3F(0, 1, 1));
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void SpriteNodeWithMaterial()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            node.CenterPosition = new Vector2F(100, 100);
            Engine.AddNode(node);

            var node2 = new SpriteNode();
            node2.Texture = texture;
            node2.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            node2.CenterPosition = new Vector2F(100, 100);
            node2.Angle = 45;
            node2.ZOrder = 1;

            node2.Material = Material.Create();
            node2.Material.AlphaBlend = AlphaBlend.Multiply;

            var psCode = @"
Texture2D mainTex : register(t0);
SamplerState mainSamp : register(s0);
struct PS_INPUT
{
    float4  Position : SV_POSITION;
    float4  Color    : COLOR0;
    float2  UV1 : UV0;
    float2  UV2 : UV1;
};
float4 main(PS_INPUT input) : SV_TARGET
{
    float4 c;
    c = mainTex.Sample(mainSamp, input.UV1) * input.Color;
    return c;
}";
            node2.Material.SetShader(Shader.Create("ps", psCode, ShaderStage.Pixel));
            node2.Material.SetTexture("mainTex", texture);
            Engine.AddNode(node2);

            tc.LoopBody(null, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void TextNode()
        {
            var tc = new TestCore();
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            var font2 = Font.LoadDynamicFont("TestData/Font/GenYoMinJP-Bold.ttf", 64);
            Assert.NotNull(font);
            Assert.NotNull(font2);
            var imageFont = Font.CreateImageFont(font);
            imageFont.AddImageGlyph('〇', Texture2D.Load(@"TestData/IO/AltseedPink.png"));

            Engine.AddNode(new TextNode() { Font = font, FontSize = 80, Text = "Hello, world! こんにちは" });
            Engine.AddNode(new TextNode() { Font = font, FontSize = 80, Text = "色を指定します。", Position = new Vector2F(0.0f, 100.0f), Color = new Color(0, 0, 255) });
            Engine.AddNode(new TextNode() { Font = font2, FontSize = 80, Text = "𠀋 𡈽 𡌛 𡑮 𡢽 𠮟 𡚴 𡸴 𣇄 𣗄 𣜿 𣝣 𣳾", Position = new Vector2F(0.0f, 300.0f) });
            Engine.AddNode(new TextNode() { Font = imageFont, FontSize = 80, Text = "Altseed〇Altseed", Position = new Vector2F(0.0f, 500.0f) });
            var rotated = new TextNode() { Font = font, FontSize = 80, Text = "回転します。", Position = new Vector2F(400.0f, 400.0f) };
            Engine.AddNode(rotated);

            tc.LoopBody(c =>
            {
                rotated.Angle += 1.0f;
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void TextNode2()
        {
            var tc = new TestCore();
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            var font2 = Font.LoadDynamicFont("TestData/Font/GenYoMinJP-Bold.ttf", 64);
            Assert.NotNull(font);
            Assert.NotNull(font2);
            var imageFont = Font.CreateImageFont(font);
            imageFont.AddImageGlyph('〇', Texture2D.Load(@"TestData/IO/AltseedPink.png"));

            TextNode node = new TextNode() { Font = font2, FontSize = 40, Text = "Hello, world! こんにちは カーニングあるよ！！", IsEnableKerning = false, Color = new Color(255, 0, 0, 200) };
            Engine.AddNode(node);
            Engine.AddNode(new TextNode() { Font = font2, FontSize = 40, Text = "Hello, world! こんにちは カーニングないです", Color = new Color(0, 255, 0, 200) });
            Engine.AddNode(new TextNode() { Font = font, FontSize = 40, Text = node.ContentSize.ToString(), Position = new Vector2F(0.0f, 50.0f) });
            Engine.AddNode(new TextNode() { Font = font, FontSize = 40, Text = "字間5です。\n行間標準です。", CharacterSpace = 10, Position = new Vector2F(0.0f, 150.0f) });
            Engine.AddNode(new TextNode() { Font = font, FontSize = 40, Text = "字間10です。\n行間70です。", CharacterSpace = 50, LineGap = 200, Position = new Vector2F(0.0f, 250.0f) });
            tc.LoopBody(c =>
            {
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void StaticFont()
        {
            var tc = new TestCore();
            tc.Init();

            Assert.IsTrue(Font.GenerateFontFile("TestData/Font/mplus-1m-regular.ttf", "test.a2f", "Hello, world! こんにちは", 64));

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            var font2 = Font.LoadStaticFont("test.a2f");
            Assert.NotNull(font);
            Assert.NotNull(font2);
            var imageFont = Font.CreateImageFont(font);
            imageFont.AddImageGlyph('〇', Texture2D.Load(@"TestData/IO/AltseedPink.png"));

            Engine.AddNode(new TextNode() { Font = font, FontSize = 80, Text = "Hello, world! こんにちは" });
            Engine.AddNode(new TextNode() { Font = font2, FontSize = 80, Text = "Hello, world! こんにちは", Position = new Vector2F(0.0f, 100.0f), Color = new Color(0, 0, 255) });

            tc.LoopBody(c => { }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Culling()
        {
            var tc = new TestCore();
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            Assert.NotNull(font);

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var text = new TextNode() { Font = font, FontSize = 30, Text = "", ZOrder = 10 };
            Engine.AddNode(text);

            var parent = new Altseed2.Node();
            Engine.AddNode(parent);

            tc.LoopBody(c =>
            {
                text.Text = "Drawing Object : " + Engine.CullingSystem.DrawingRenderedCount + " FPS: " + Engine.CurrentFPS.ToString();

                var node = new SpriteNode();
                node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
                node.Texture = texture;
                node.Position = new Vector2F(200, -500);
                parent.AddChildNode(node);

                foreach (var item in parent.Children.OfType<SpriteNode>())
                {
                    item.Position += new Vector2F(0, 10);
                }

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void MassSpriteNode()
        {
            var tc = new TestCore(new Configuration() { WaitVSync = false });
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink256.png");
            Assert.NotNull(texture);

            var ws = Engine.WindowSize;
            var size = 10;
            for (var x = 0; x < ws.X / size; x++)
            {
                for (var y = 0; y < ws.Y / size; y++)
                {
                    var node = new SpriteNode();
                    node.Texture = texture;
                    node.Src = new RectF(new Vector2F(128 * (x % 2), 128 * (y % 2)), new Vector2F(128, 128));
                    node.Scale = new Vector2F(1, 1) * size / 128f;
                    node.Position = new Vector2F(x, y) * size;
                    Engine.AddNode(node);
                }
            }

            tc.LoopBody(null, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void PolygonNode_SetVertexesByVector2F()
        {
            var tc = new TestCore(new Configuration() { WaitVSync = false });
            tc.Init();

            var node = new PolygonNode();
            Engine.AddNode(node);

            tc.LoopBody(c =>
            {
                var sin = MathF.Sin(MathHelper.DegreeToRadian(c)) * 50;
                var cos = MathF.Cos(MathHelper.DegreeToRadian(c)) * 50;

                Span<Vector2F> span = stackalloc Vector2F[] {
                    new Vector2F(100 + cos, 100 - sin),
                    new Vector2F(100 - sin, 100 - cos),
                    new Vector2F(100 - cos, 100 + sin),
                    new Vector2F(100 + sin, 100 + cos),
                };

                node.SetVertexes(span, new Color(255, c % 255, 255, 255));
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void PolygonNode()
        {
            var tc = new TestCore();
            tc.Init();

            PolygonNode node = new PolygonNode()
            {
                Position = new Vector2F(250, 250)
            };
            Engine.AddNode(node);

            Span<Vector2F> span = stackalloc Vector2F[]
            {
                new Vector2F(-100, -100),
                new Vector2F(100, -100),
                new Vector2F(100, 100),
                new Vector2F(-100, 100),
            };

            node.SetVertexes(span, new Color(255, 0, 0));

            foreach (var current in node.Buffers) System.Diagnostics.Debug.WriteLine(current);

            tc.LoopBody(c => { }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void IBPolygonNode()
        {
            var tc = new TestCore();
            tc.Init();

            var node = new PolygonNode()
            {
                Position = new Vector2F(250, 250)
            };
            Engine.AddNode(node);

            //Span<Vector2F> span = stackalloc Vector2F[]
            //{
            //    new Vector2F(-100, -100),
            //    new Vector2F(-50, -100),
            //    new Vector2F(-50, -50),
            //    new Vector2F(-100, -50),
            //    new Vector2F(50, 50),
            //    new Vector2F(100, 50),
            //    new Vector2F(100, 100),
            //    new Vector2F(50, 100),
            //};

            //node.SetVertexes(span, new Color(255, 0, 0));

            //node.Buffers = new[]
            //{
            //    new IndexBuffer(0, 1, 2),
            //    new IndexBuffer(0, 2, 3),
            //    new IndexBuffer(4, 5, 6),
            //    new IndexBuffer(4, 6, 7),
            //};

            node.SetVertexGroupsFromPositions(new[]
            {
                new[]
                {
                    new Vector2F(-100, -100),
                    new Vector2F(-50, -100),
                    new Vector2F(-50, -50),
                    new Vector2F(-100, -50),
                },
                new[]
                {
                    new Vector2F(50, 50),
                    new Vector2F(100, 50),
                    new Vector2F(100, 100),
                    new Vector2F(50, 100),
                },
            }, new Color(255, 0, 0));

            tc.LoopBody(null, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void IBPolygonNodeWithTexture()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new PolygonNode()
            {
                Position = new Vector2F(250, 250),
                Texture = texture
            };
            Engine.AddNode(node);

            var basePositions = new[]
            {
                new Vector2F(-100f, -100f),
                new Vector2F(100f, 100f),
                new Vector2F(-150f, 250f),
                new Vector2F(250f, -150f),
            };
            var array = new Vector2F[basePositions.Length][];
            for (int i = 0; i < basePositions.Length; i++)
            {
                array[i] = new[]
                {
                    basePositions[i],
                    basePositions[i] + new Vector2F(50f, 0f),
                    basePositions[i] + new Vector2F(50f, 50f),
                    basePositions[i] + new Vector2F(0f, 50f),
                };
            }

            node.SetVertexGroupsFromPositions(array, new Color(255, 255, 255));

            tc.LoopBody(null, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void CenterPosition()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.CenterPosition = texture.Size / 2;
            node.Position = Engine.WindowSize / 2;
            Engine.AddNode(node);

            var child = new SpriteNode();
            child.Texture = texture;
            child.CenterPosition = texture.Size / 2;
            //child.Position = texture.Size / 2;
            node.AddChildNode(child);

            var child2 = new SpriteNode();
            child2.Texture = texture;
            child2.CenterPosition = texture.Size / 2;
            child2.Position = texture.Size / 2;
            node.AddChildNode(child2);

            tc.LoopBody(c =>
            {
                node.Angle += 1.0f;
                child.Angle += 1.0f;
                child2.Angle += 1.0f;
            }
            , null);

            tc.End();
        }


        [Test, Apartment(ApartmentState.STA)]
        public void Pivot()
        {
            var tc = new TestCore();
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            Assert.NotNull(font);

            var rotated = new AnchorTransformerNode()
            {
                Position = new Vector2F(300.0f, 300.0f),
                Pivot = new Vector2F(0.5f, 0.5f),
            };
            var text = new TextNode();
            text.Font = font;
            text.FontSize = 80;
            text.Text = "中心で回転します";
            rotated.Size = text.ContentSize;
            Engine.AddNode(text);
            text.AddChildNode(rotated);

            tc.LoopBody(c =>
            {
                text.Text = "中心で回転します" + c.ToString();
                rotated.Size = text.ContentSize;

                rotated.Angle += 1.0f;
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Anchor()
        {
            var tc = new TestCore(new Configuration() { VisibleTransformInfo = true });
            tc.Init();

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf", 64);
            Assert.NotNull(font);

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var sprite = new SpriteNode();
            sprite.Texture = texture;
            sprite.ZOrder = 5;

            Vector2F rectSize = texture.Size;
            var parent = new AnchorTransformerNode();
            parent.Position = Engine.WindowSize / 2;
            parent.Size = rectSize;
            parent.AnchorMode = AnchorMode.Fill;
            Engine.AddNode(sprite);
            sprite.AddChildNode(parent);

            var sprite2 = new SpriteNode();
            sprite2.Texture = texture;
            sprite2.Color = new Color(255, 0, 0, 200);
            sprite2.ZOrder = 10;

            var child = new AnchorTransformerNode();
            child.Position = rectSize / 2;
            child.Pivot = new Vector2F(0.5f, 0.5f);
            child.AnchorMin = new Vector2F(0.0f, 0.0f);
            child.AnchorMax = new Vector2F(0.5f, 1f);
            child.HorizontalAlignment = HorizontalAlignment.Left;
            child.VerticalAlignment = VerticalAlignment.Center;
            child.Size = sprite2.ContentSize;
            child.AnchorMode = AnchorMode.KeepAspect;
            sprite.AddChildNode(sprite2);
            sprite2.AddChildNode(child);

            var text = new TextNode();
            text.Font = font;
            text.FontSize = 20;
            text.Color = new Color(0, 0, 0);
            text.Text = "あいうえお";
            text.ZOrder = 15;

            var childText = new AnchorTransformerNode();
            childText.Pivot = new Vector2F(0.5f, 0.5f);
            childText.AnchorMin = new Vector2F(0.5f, 0.5f);
            childText.AnchorMax = new Vector2F(0.5f, 0.5f);
            childText.Size = text.ContentSize;
            //childText.HorizontalAlignment = HorizontalAlignment.Center;
            //childText.VerticalAlignment = VerticalAlignment.Center;
            childText.AnchorMode = AnchorMode.ContentSize;
            sprite2.AddChildNode(text);
            text.AddChildNode(childText);

            var text2 = new TextNode()
            {
                Font = font,
                FontSize = 20,
                Text = "",
                ZOrder = 10,
                Scale = new Vector2F(0.8f, 0.8f),
                Color = new Color(255, 128, 0)
            };
            Engine.AddNode(text2);

            tc.Duration = 10000;

            string infoText(AnchorTransformerNode n) =>
                    $"Scale:{n.Scale}\n" +
                    $"Position:{n.Position}\n" +
                    $"Pivot:{n.Pivot}\n" +
                    $"Size:{n.Size}\n" +
                    $"Margin: LT:{n.LeftTop} RB:{n.RightBottom}\n" +
                    $"Anchor: {n.AnchorMin} {n.AnchorMax}\n";

            tc.LoopBody(c =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) rectSize.X += 1.5f;
                if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) rectSize.X -= 1.5f;
                if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) rectSize.Y += 1.5f;
                if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) rectSize.Y -= 1.5f;

                if (Engine.Keyboard.GetKeyState(Key.D) == ButtonState.Hold) parent.Position += new Vector2F(1.5f, 0);
                if (Engine.Keyboard.GetKeyState(Key.A) == ButtonState.Hold) parent.Position += new Vector2F(-1.5f, 0);
                if (Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Hold) parent.Position += new Vector2F(0, 1.5f);
                if (Engine.Keyboard.GetKeyState(Key.W) == ButtonState.Hold) parent.Position += new Vector2F(0, -1.5f);

                if (Engine.Keyboard.GetKeyState(Key.Q) == ButtonState.Hold) child.Angle += 1.5f;
                if (Engine.Keyboard.GetKeyState(Key.E) == ButtonState.Hold) child.Angle -= 1.5f;

                if (Engine.Keyboard.GetKeyState(Key.Z) == ButtonState.Hold) parent.Scale += new Vector2F(0.1f, 0);
                if (Engine.Keyboard.GetKeyState(Key.C) == ButtonState.Hold) parent.Scale -= new Vector2F(0.1f, 0);

                parent.Size = rectSize;

                text2.Text = infoText(parent) + '\n' + infoText(child) + '\n' + infoText(childText);
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void IsDrawn()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.CenterPosition = texture.Size / 2;
            node.Position = new Vector2F(100, 100);
            Engine.AddNode(node);

            var node2 = new SpriteNode();
            node2.Texture = texture;
            node2.CenterPosition = texture.Size / 2;
            node2.Position = new Vector2F(200, 200);

            var node3 = new RectangleNode();
            node3.RectangleSize = texture.Size;
            node3.Texture = texture;
            node3.CenterPosition = texture.Size / 2;
            node3.Position = new Vector2F(300, 300);

            tc.LoopBody(c =>
            {
                if (c == 2)
                {
                    node.AddChildNode(node2);
                }
                else if (c == 4)
                {
                    node.IsDrawn = false;
                    Assert.IsFalse(node.IsDrawnActually);
                    Assert.IsFalse(node2.IsDrawnActually);
                }

                else if (c == 6)
                {
                    node2.AddChildNode(node3);
                }

                else if (c == 8)
                {
                    node.IsDrawn = true;

                    Assert.IsTrue(node.IsDrawnActually);
                    Assert.IsTrue(node2.IsDrawnActually);
                    Assert.IsTrue(node3.IsDrawnActually);
                }
                else if (c == 10)
                {
                    node2.IsDrawn = false;

                    Assert.IsTrue(node.IsDrawnActually);
                    Assert.IsFalse(node2.IsDrawnActually);
                    Assert.IsFalse(node3.IsDrawnActually);
                }
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void ZOrder()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.CenterPosition = texture.Size / 2;
            node.Position = new Vector2F(100, 100);
            node.Color = new Color(255, 0, 0);
            node.ZOrder = 300;
            Engine.AddNode(node);

            var node2 = new SpriteNode();
            node2.Texture = texture;
            node2.CenterPosition = texture.Size / 2;
            node2.Position = new Vector2F(200, 200);
            node2.ZOrder = 200;
            node2.Color = new Color(0, 0, 255);
            Engine.AddNode(node2);

            var node3 = new SpriteNode();
            node3.Texture = texture;
            node3.CenterPosition = texture.Size / 2;
            node3.Position = new Vector2F(300, 300);
            node3.ZOrder = 150;
            node3.Color = new Color(0, 255, 0);
            Engine.AddNode(node3);

            tc.LoopBody(c =>
            {
                node3.ZOrder++;
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void TransformNodeInfo()
        {
            var tc = new TestCore(new Configuration() { VisibleTransformInfo = true });
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.Position = new Vector2F(100, 200);
            node.Src = new RectF(0, 0, 128, 128);
            Engine.AddNode(node);

            var node2 = new SpriteNode();
            node2.Texture = texture;
            node2.CenterPosition = texture.Size / 2;
            node2.Position = new Vector2F(200, 200);
            node2.Angle = 68;
            node2.ZOrder = 200;
            node2.Scale = new Vector2F(0.8f, 0.5f);
            node2.Color = new Color(0, 0, 255);
            node.AddChildNode(node2);

            var node3 = new SpriteNode();
            node3.Texture = texture;
            node3.CenterPosition = texture.Size / 2;
            node3.Position = new Vector2F(300, 300);
            node3.ZOrder = 150;
            node3.Color = new Color(0, 255, 0);
            node2.AddChildNode(node3);

            tc.Duration = 600;
            tc.LoopBody(c =>
            {
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Polygon3DNode()
        {
            var tc = new TestCore();
            tc.Init();

            Polygon3DNode node = new Polygon3DNode();
            Engine.AddNode(node);

            Vertex[] vertexes = new Vertex[4];
            vertexes[0] = new Vertex() { Position = new Vector3F(0, 1, 0), Color = new Color(255, 255, 255) };
            vertexes[1] = new Vertex() { Position = new Vector3F(0, 0, 1), Color = new Color(255, 0, 0) };
            vertexes[2] = new Vertex() { Position = new Vector3F(1.73f * 0.5f, 0, -0.5f), Color = new Color(0, 0, 255) };
            vertexes[3] = new Vertex() { Position = new Vector3F(-1.73f * 0.5f, 0, -0.5f), Color = new Color(0, 255, 0) };

            node.SetVertexes(vertexes.AsSpan());

            node.Buffers = new int[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 1,
                1, 2, 3
            };
            //node.Position = new Vector3F(0, 0, -5);

            foreach (var current in node.Buffers) System.Diagnostics.Debug.WriteLine(current);
            foreach (var current in node.Vertexes) System.Diagnostics.Debug.WriteLine(current.Position);
            float r = 0;
            tc.Duration = 1000;
            tc.LoopBody(c =>
            {
                if (c % 60 == 0)
                    r += 45;

                var q = node.Quaternion;
                q.EulerAngles = new Vector3F(0, r, 0);
                node.Quaternion = q;
            }, null);

            tc.End();
        }


        [Test, Apartment(ApartmentState.STA)]
        public void Model3DBunny()
        {
            var tc = new TestCore(new Configuration() { IsResizable = true });
            tc.Init();

            var house = new Transform3DNode();

            foreach (var node in Model3D.LoadObjFile(@"TestData/test.obj").Select(model => model.ToPolygon3DNode(new Color(200, 200, 200), true)))
            {
                house.AddChildNode(node);
                var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
                node.AddChildNode(lighting);
            }

            house.Scale *= 0.01f;
            Engine.AddNode(house);

            var deer = new Transform3DNode();

            foreach (var node in Model3D.LoadObjFile(@"TestData/test2.obj").Select(model => model.ToPolygon3DNode(new Color(200, 200, 200))))
            {
                deer.AddChildNode(node);
                var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
                node.AddChildNode(lighting);
            }

            deer.Scale *= 0.01f;
            deer.Position = new Vector3F(1, 0, 0f);
            Engine.AddNode(deer);

            var bugatti = new Transform3DNode();

            foreach (var node in Model3D.LoadObjFile(@"TestData/skyline.obj").Select(model => model.ToPolygon3DNode(new Color(200, 200, 200))))
            {
                bugatti.AddChildNode(node);
                var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
                node.AddChildNode(lighting);
            }

            bugatti.Scale *= 0.3f;
            bugatti.Position = new Vector3F(0, 0, -1f);
            Engine.AddNode(bugatti);

            tc.Duration = 1000;

            tc.LoopBody(c =>
            {
                bugatti.Quaternion = Quaternion.Euler(new Vector3F(0, c, 0));
                deer.Quaternion = Quaternion.Euler(new Vector3F(0, c, 0));
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Model3DBox()
        {
            var tc = new TestCore(new Configuration() { IsResizable = true });
            tc.Init();

            var model = Model3D.CreateBox(new Vector3F(1, 1, 1));
            var node = model.ToPolygon3DNode(new Color(200, 200, 200));
            var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
            node.AddChildNode(lighting);
            node.AddChildNode(model.ToPolygon3DNodeLine(new Color(0, 0, 0, 255)));
            Engine.AddNode(node);
            node.Position = new Vector3F();

            tc.Duration = 1000;
            foreach (var current in node.Vertexes) Console.WriteLine(current.Position);

            tc.LoopBody(c =>
            {
                node.Quaternion = Quaternion.Euler(new Vector3F(c, c, 0));
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Model3DSphere()
        {
            var tc = new TestCore(new Configuration() { IsResizable = true });
            tc.Init();

            Model3D model = Model3D.CreateUVSphere(1, 10, 10);
            var node = model.ToPolygon3DNode(new Color(200, 200, 200));
            var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
            node.AddChildNode(lighting);
            Engine.AddNode(node);

            Polygon3DNode line = model.ToPolygon3DNodeLine(new Color(0, 0, 0, 255));
            Engine.AddNode(line);

            tc.Duration = 1000;
            foreach (var current in node.Vertexes) Console.WriteLine(current.Position);

            tc.LoopBody(c =>
            {
                if (c % 60 == 0)
                    node.IsDrawn = false;
                else if (c % 30 == 0)
                    node.IsDrawn = true;

                node.Quaternion = Quaternion.Euler(new Vector3F(c, c, 0));
                line.Quaternion = Quaternion.Euler(new Vector3F(c, c, 0));
            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Model3DBunnyWithMaterial()
        {
            var tc = new TestCore();
            tc.Init();

            var screen = RenderTexture.Create(Engine.WindowSize, TextureFormat.R8G8B8A8_UNORM, true);
            var camera = new Camera3DNode();
            camera.Group = 1;
            camera.TargetTexture = screen;
            Engine.AddNode(camera);

            var deer = new Transform3DNode();

            foreach (var node in Model3D.LoadObjFile(@"TestData/test2.obj").Select(model => model.ToPolygon3DNode(new Color(200, 200, 200))))
            {
                deer.AddChildNode(node);
                var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
                node.CameraGroup = 1;
                node.AddChildNode(lighting);
            }

            deer.Scale *= 0.005f;
            deer.Position = new Vector3F(0, 0, 0f);
            Engine.AddNode(deer);

            var node2 = new SpriteNode();
            node2.Texture = screen;
            node2.ZOrder = 1;

            node2.Material = Material.Create();

            var psCode = @"
Texture2D mainTex : register(t0);
Texture2D mainTex_depth : register(t1);
SamplerState mainSamp : register(s0);
SamplerState mainSampDepth : register(s1);
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
    c = mainTex_depth.Sample(mainSampDepth, input.UV1).rrrr;
    return c;
}";
            node2.Material.SetShader(Shader.Create("ps", psCode, ShaderStage.Pixel));
            node2.Material.SetTexture("mainTex", screen);
            Engine.AddNode(node2);

            tc.LoopBody(null, null);
        }

        public void VisibleTransformNodeInfo()
        {
            var tc = new TestCore(new Configuration() { VisibleTransformInfo = true });
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node4 = new SpriteNode();
            node4.Texture = texture;
            node4.Position = new Vector2F(400, 200);
            node4.Src = new RectF(0, 0, 128, 128);
            node4.VisibleTransformNodeInfo = false;
            Engine.AddNode(node4);

            var node5 = new SpriteNode();
            node5.Texture = texture;
            node5.CenterPosition = texture.Size / 2;
            node5.Position = new Vector2F(200, 200);
            node5.Angle = 68;
            node5.ZOrder = 200;
            node5.Scale = new Vector2F(0.8f, 0.5f);
            node5.Color = new Color(0, 0, 255);
            node5.VisibleTransformNodeInfo = false;
            node4.AddChildNode(node5);

            var node6 = new SpriteNode();
            node6.Texture = texture;
            node6.CenterPosition = texture.Size / 2;
            node6.Position = new Vector2F(300, 300);
            node6.ZOrder = 150;
            node6.Color = new Color(0, 255, 0);
            node6.VisibleTransformNodeInfo = false;
            node5.AddChildNode(node6);

            tc.Duration = 600;
            tc.LoopBody(c =>
            {
                node4.VisibleTransformNodeInfo = (c / 10) % 2 == 0;
                node5.VisibleTransformNodeInfo = !node4.VisibleTransformNodeInfo;
                node6.VisibleTransformNodeInfo = !node5.VisibleTransformNodeInfo;
            }
            , null);

            tc.End();
        }
    }
}
