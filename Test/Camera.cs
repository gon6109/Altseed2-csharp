using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace Altseed2.Test
{
    [TestFixture]
    class Camera
    {
        [Test, Apartment(ApartmentState.STA)]
        public void NoRenderTexture()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.CenterPosition = texture.Size / 2;
            node.CameraGroup = 0b1;
            Engine.AddNode(node);

            var camera = new CameraNode();
            camera.Position = new Vector2F(100, 0);
            camera.Scale = new Vector2F(2, 2);
            camera.Group = 0b1;
            Engine.AddNode(camera);

            tc.LoopBody(c =>
            {
                node.Angle++;
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void _RenderTexture()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            node.Texture = texture;
            node.CenterPosition = texture.Size / 2;
            node.Scale = new Vector2F(0.5f, 0.5f);
            node.CameraGroup = 0b1;
            Engine.AddNode(node);

            var renderTexture = RenderTexture.Create(new Vector2I(200, 200), TextureFormat.R8G8B8A8_UNORM, false);
            var camera = new CameraNode();
            camera.Group = 0b1;
            camera.TargetTexture = renderTexture;
            Engine.AddNode(camera);

            var node2 = new SpriteNode();
            node2.CameraGroup = 0b10;
            node2.Texture = renderTexture;
            node2.Position = new Vector2F(300, 300);
            Engine.AddNode(node2);

            var camera2 = new CameraNode();
            camera2.Group = 0b10;
            Engine.AddNode(camera2);

            tc.LoopBody(c =>
            {
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void NoRenderTexture3D()
        {
            var tc = new TestCore();
            tc.Init();

            Polygon3DNode node = new Polygon3DNode();
            Engine.AddNode(node);

            node.CameraGroup = 0b1;
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

            var camera = new Camera3DNode();
            camera.Position = new Vector3F(0, 0, -5);
            camera.Group = 0b1;
            Engine.AddNode(camera);
            tc.Duration = 1000;

            var font = Font.LoadDynamicFont("TestData/Font/mplus-1m-regular.ttf");
            Assert.NotNull(font);

            var text = new TextNode();
            text.Font = font;
            text.FontSize = 30;
            text.Color = new Color(255, 255, 255);
            text.ZOrder = 15;

            Engine.AddNode(text);

            tc.LoopBody(c =>
            {
                text.Text = $"{camera.Quaternion.EulerAngles}";

                var euler = camera.Quaternion.EulerAngles;
                if (Engine.Keyboard.GetKeyState(Key.D) == ButtonState.Hold) camera.Position += new Vector3F(0.1f, 0, 0);
                if (Engine.Keyboard.GetKeyState(Key.A) == ButtonState.Hold) camera.Position -= new Vector3F(0.1f, 0, 0);
                if (Engine.Keyboard.GetKeyState(Key.W) == ButtonState.Hold) camera.Position += new Vector3F(0, 0, 0.1f);
                if (Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Hold) camera.Position -= new Vector3F(0, 0, 0.1f);
                if (Engine.Keyboard.GetKeyState(Key.LeftShift) == ButtonState.Hold) camera.Position -= new Vector3F(0, 0.1f, 0);
                if (Engine.Keyboard.GetKeyState(Key.Space) == ButtonState.Hold) camera.Position += new Vector3F(0, 0.1f, 0);
                if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) euler += new Vector3F(0, 1, 0);
                if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) euler -= new Vector3F(0, 1, 0);
                if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) euler += new Vector3F(1, 0, 0);
                if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) euler -= new Vector3F(1, 0, 0);

                camera.Quaternion = Quaternion.Euler(euler);
            }
            , null);

            tc.End();
        }
    }
}
