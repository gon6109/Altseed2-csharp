using System;
using System.Threading;

using NUnit.Framework;

namespace Altseed2.Test
{
    [TestFixture]
    class Collider
    {
        [Test, Apartment(ApartmentState.STA)]
        public void SpriteNode()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var node = new SpriteNode();
            //node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            node.Texture = texture;
            node.Position = new Vector2F(200, 200);
            node.CenterPosition = texture.Size / 2;
            node.Scale = new Vector2F(0.2f, 0.2f);
            Engine.AddNode(node);

            var col = new CircleCollider();
            col.Radius = 200 * 0.2f;
            col.Position = node.Position;

            var node2 = new SpriteNode();
            //node.Src = new RectF(new Vector2F(100, 100), new Vector2F(200, 200));
            node2.Texture = texture;
            node2.Position = new Vector2F(200, 200);
            node2.CenterPosition = texture.Size / 2;
            node2.Scale = new Vector2F(0.2f, 0.2f);
            Engine.AddNode(node2);
            var col2 = new CircleCollider();
            col2.Radius = 200 * 0.2f;
            col2.Position = node.Position;

            tc.LoopBody(c =>
            {
                node2.Position = col2.Position = Engine.Mouse.Position;
                if (col.GetIsCollidedWith(col2))
                {
                    node2.Angle++;
                }
            }
            , null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void AutoCollisionSystem_Circle()
        {
            var tc = new TestCore();
            tc.Init();
            //tc.Duration = int.MaxValue;

            var texture = Texture2D.LoadStrict(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var scene = new Altseed2.Node();
            var manager = new CollisionManagerNode();
            scene.AddChildNode(manager);

            Engine.AddNode(scene);

            var player = new Player_Circle(texture);
            scene.AddChildNode(player);

            var comparison = new SpriteNode()
            {
                Texture = texture,
                CenterPosition = texture.Size / 2,
                Position = new Vector2F(500f, 100f),
                Scale = new Vector2F(0.8f, 0.8f),
            };
            var colliderNode = new CircleColliderNode()
            {
                Radius = texture.Size.X / 2,
            };
            comparison.AddChildNode(colliderNode);
            colliderNode.AddChildNode(ColliderVisualizeNodeFactory.Create(colliderNode));
            scene.AddChildNode(comparison);

            tc.LoopBody(null, x =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push) tc.Duration = 0;
                if (x == 10)
                {
                    Assert.True(manager.ContainsCollider(colliderNode));
                    Assert.AreEqual(manager.ColliderCount, 2);
                }
            });
            tc.End();
        }

        private sealed class Player_Circle : SpriteNode, ICollisionEventReceiver
        {
            private readonly CircleColliderNode node;

            private readonly TextNode text;

            public Player_Circle(Texture2D texture)
            {
                Engine.AddNode(text = new TextNode()
                {
                    Font = Font.LoadDynamicFontStrict("TestData/Font/mplus-1m-regular.ttf", 64),
                    FontSize = 40,
                    Position = new Vector2F(0, 0),
                    Color = new Color(255, 255, 255, 255),
                });

                Texture = texture;
                Position = new Vector2F(0f, 300f);
                CenterPosition = texture.Size / 2;
                node = new CircleColliderNode()
                {
                    Radius = texture.Size.X / 2
                };
                node.AddChildNode(ColliderVisualizeNodeFactory.Create(node));
                AddChildNode(node);
            }

            protected override void OnUpdate()
            {
                Position += new Vector2F(5f, 0f);
                //if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) Position += new Vector2F(0.0f, -2.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) Position += new Vector2F(0.0f, 2.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) Position += new Vector2F(-2.0f, 0.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) Position += new Vector2F(2.0f, 0.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Num1) == ButtonState.Hold) Angle++;
                //if (Engine.Keyboard.GetKeyState(Key.Num2) == ButtonState.Hold) Angle--;
            }

            void ICollisionEventReceiver.OnCollisionEnter(CollisionInfo info)
            {
                text.Text = "Colliding";
            }

            void ICollisionEventReceiver.OnCollisionExit(CollisionInfo info)
            {
                text.Text = string.Empty;
            }

            void ICollisionEventReceiver.OnCollisionStay(CollisionInfo info)
            {

            }
        }

        private readonly static Vector2F[] array = new[] {
            default,
            new Vector2F(-225f, 0f), new Vector2F(-75f, -75f),
            new Vector2F(0f, -225f), new Vector2F(75f, -75f),
            new Vector2F(225f, 0f), new Vector2F(75f, 75f),
            new Vector2F(0f, 225f), new Vector2F(-75f, 75f),
            new Vector2F(-225f, 0f)
        };

        [Test, Apartment(ApartmentState.STA)]
        public void AutoCollisionSystem_Polygon()
        {
            var tc = new TestCore();
            tc.Init();
            //tc.Duration = int.MaxValue;

            var texture = Texture2D.LoadStrict(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var scene = new Altseed2.Node();
            var manager = new CollisionManagerNode();
            scene.AddChildNode(manager);

            Engine.AddNode(scene);

            var player = new Player_Polygon(texture);
            player.Angle += 45f;

            scene.AddChildNode(player);

            var comparison = new SpriteNode()
            {
                Texture = texture,
                CenterPosition = texture.Size / 2,
                Position = new Vector2F(580f, 300f),
            };
            var colliderNode = new PolygonColliderNode
            {
                Vertexes = array,
            };
            colliderNode.AddChildNode(ColliderVisualizeNodeFactory.Create(colliderNode));
            comparison.AddChildNode(colliderNode);

            scene.AddChildNode(comparison);

            tc.LoopBody(null, x =>
            {
                player.Angle++;
                comparison.Angle++;
                if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push) tc.Duration = 0;
                if (x == 10)
                {
                    Assert.True(manager.ContainsCollider(colliderNode));
                    Assert.AreEqual(manager.ColliderCount, 2);
                }
            });
            tc.End();
        }


        [Test, Apartment(ApartmentState.STA)]
        public void AutoCollisionSystem_PolygonIB()
        {
            var tc = new TestCore();
            tc.Init();
            //tc.Duration = int.MaxValue;

            var texture = Texture2D.LoadStrict(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var scene = new Altseed2.Node();
            var manager = new CollisionManagerNode();
            scene.AddChildNode(manager);

            Engine.AddNode(scene);

            var player = new Player_Polygon(texture);
            player.Angle += 45f;

            scene.AddChildNode(player);

            var comparison = new SpriteNode()
            {
                Color = default,
                Texture = texture,
                CenterPosition = texture.Size / 2,
                Position = new Vector2F(580f, 300f),
            };
            var colliderNode = new PolygonColliderNode();
            colliderNode.SetVertexes(stackalloc Vector2F[]
            {
                new Vector2F(-150f, -150f),
                new Vector2F(-200f, -150f),
                new Vector2F(-200f, -100f),
                new Vector2F(-150f, -100f),
                new Vector2F(-200f, 100f),
                new Vector2F(-200f, 150f),
                new Vector2F(-150f, 150f),
                new Vector2F(-150f, 100f),
            }, false);
            colliderNode.Buffers = new[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 };
            colliderNode.AddChildNode((PolygonNode)ColliderVisualizeNodeFactory.Create(colliderNode));
            comparison.AddChildNode(colliderNode);

            scene.AddChildNode(comparison);

            tc.LoopBody(null, x =>
            {
                player.Angle++;
                if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push) tc.Duration = 0;
                if (x == 10)
                {
                    Assert.True(manager.ContainsCollider(colliderNode));
                    Assert.AreEqual(manager.ColliderCount, 2);
                }
            });
            tc.End();
        }
        private sealed class Player_Polygon : SpriteNode, ICollisionEventReceiver
        {
            private readonly PolygonColliderNode node;
            private readonly TextNode text = new TextNode()
            {
                Font = Font.LoadDynamicFontStrict("TestData/Font/mplus-1m-regular.ttf", 64),
                FontSize = 40,
                Position = new Vector2F(0, 0),
                Color = new Color(255, 255, 255, 255),
            };
            public Player_Polygon(Texture2D texture)
            {
                Engine.AddNode(text);
                Texture = texture;
                CenterPosition = texture.Size / 2;
                Position = new Vector2F(220f, 300f);
                node = new PolygonColliderNode()
                {
                    Vertexes = array
                };
                node.AddChildNode(ColliderVisualizeNodeFactory.Create(node));
                AddChildNode(node);
            }
            //protected override void OnUpdate()
            //{
            //    if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) Position += new Vector2F(0.0f, -2.0f);
            //    if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) Position += new Vector2F(0.0f, 2.0f);
            //    if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) Position += new Vector2F(-2.0f, 0.0f);
            //    if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) Position += new Vector2F(2.0f, 0.0f);
            //    if (Engine.Keyboard.GetKeyState(Key.Num1) == ButtonState.Hold) Angle++;
            //    if (Engine.Keyboard.GetKeyState(Key.Num2) == ButtonState.Hold) Angle--;
            //}
            void ICollisionEventReceiver.OnCollisionEnter(CollisionInfo info)
            {
                text.Text = "Colliding";
            }
            void ICollisionEventReceiver.OnCollisionExit(CollisionInfo info)
            {
                text.Text = string.Empty;
            }
            void ICollisionEventReceiver.OnCollisionStay(CollisionInfo info)
            {

            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void AutoCollisionSystem_Rectangle()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.LoadStrict(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var scene = new Altseed2.Node();
            var manager = new CollisionManagerNode();
            scene.AddChildNode(manager);

            Engine.AddNode(scene);

            var player = new Player_Rectangle(texture);

            scene.AddChildNode(player);

            var comparison = new SpriteNode()
            {
                Texture = texture,
                CenterPosition = texture.Size / 2,
                Position = new Vector2F(500f, 200f),
                Scale = new Vector2F(0.8f, 0.8f),
            };
            var colliderNode = new RectangleColliderNode()
            {
                RectangleSize = texture.Size,
                CenterPosition = texture.Size / 2,
            };
            colliderNode.AddChildNode(ColliderVisualizeNodeFactory.Create(colliderNode));
            comparison.AddChildNode(colliderNode);

            scene.AddChildNode(comparison);

            tc.LoopBody(null, x =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push) tc.Duration = 0;
                if (x == 10)
                {
                    Assert.True(manager.ContainsCollider(colliderNode));
                    Assert.AreEqual(manager.ColliderCount, 2);
                }
            });
            tc.End();
        }
        private sealed class Player_Rectangle : SpriteNode, ICollisionEventReceiver
        {
            private readonly RectangleColliderNode node;
            private readonly TextNode text = new TextNode()
            {
                Font = Font.LoadDynamicFontStrict("TestData/Font/mplus-1m-regular.ttf", 64),
                FontSize = 40,
            };
            public Player_Rectangle(Texture2D texture)
            {
                Engine.AddNode(text);
                Texture = texture;
                CenterPosition = texture.Size / 2;
                Position = new Vector2F(0f, 300f);
                node = new RectangleColliderNode()
                {
                    RectangleSize = texture.Size,
                    CenterPosition = texture.Size / 2,
                };
                node.AddChildNode(ColliderVisualizeNodeFactory.Create(node));
                AddChildNode(node);
            }
            protected override void OnUpdate()
            {
                Position += new Vector2F(5f, 0f);
                //if (Engine.Keyboard.GetKeyState(Key.Up) == ButtonState.Hold) Position += new Vector2F(0.0f, -2.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Down) == ButtonState.Hold) Position += new Vector2F(0.0f, 2.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Left) == ButtonState.Hold) Position += new Vector2F(-2.0f, 0.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Right) == ButtonState.Hold) Position += new Vector2F(2.0f, 0.0f);
                //if (Engine.Keyboard.GetKeyState(Key.Num1) == ButtonState.Hold) Angle++;
                //if (Engine.Keyboard.GetKeyState(Key.Num2) == ButtonState.Hold) Angle--;
            }
            void ICollisionEventReceiver.OnCollisionEnter(CollisionInfo info)
            {
                text.Text = "Colliding";
            }
            void ICollisionEventReceiver.OnCollisionExit(CollisionInfo info)
            {
                text.Text = string.Empty;
            }
            void ICollisionEventReceiver.OnCollisionStay(CollisionInfo info)
            {

            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void AutoCollisionSystem_Edge()
        {
            var tc = new TestCore();
            tc.Init();
            //tc.Duration = int.MaxValue;

            var texture = Texture2D.LoadStrict(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var scene = new Altseed2.Node();
            var manager = new CollisionManagerNode();
            scene.AddChildNode(manager);

            Engine.AddNode(scene);

            var player = new Player_Circle(texture);
            scene.AddChildNode(player);

            var comparison = new SpriteNode()
            {
                Texture = texture,
                CenterPosition = texture.Size / 2,
                Position = new Vector2F(500f, 100f),
                Scale = new Vector2F(0.8f, 0.8f),
            };
            var colliderNode = new EdgeColliderNode()
            {
                Point1 = new Vector2F(0, texture.Size.Y / 2),
                Point2 = new Vector2F(0, -texture.Size.Y / 2),
            };
            comparison.AddChildNode(colliderNode);
            colliderNode.AddChildNode(ColliderVisualizeNodeFactory.Create(colliderNode));
            scene.AddChildNode(comparison);

            tc.LoopBody(null, x =>
            {
                if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push) tc.Duration = 0;
                if (x == 10)
                {
                    Assert.True(manager.ContainsCollider(colliderNode));
                    Assert.AreEqual(manager.ColliderCount, 2);
                }
            });
            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Collider3D()
        {
            var tc = new TestCore(new Configuration() { IsResizable = true });
            tc.Init();

            var model = Model3D.CreateBox(new Vector3F(1f, 0.5f, 0.7f));
            var node = model.ToPolygon3DNode(new Color(200, 200, 200));
            var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
            node.AddChildNode(lighting);
            node.AddChildNode(model.ToPolygon3DNodeLine(new Color(0, 0, 0, 255)));
            Engine.AddNode(node);
            node.Position = new Vector3F();

            var sphere = Model3D.CreateUVSphere(0.5f, 10, 10);
            var node2 = sphere.ToPolygon3DNode(new Color(200, 200, 200));
            var lighting2 = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
            node2.AddChildNode(lighting2);
            node2.AddChildNode(sphere.ToPolygon3DNodeLine(new Color(0, 0, 0, 255)));
            Engine.AddNode(node2);
            node2.Position = new Vector3F(-3, 0, 0);
            var col = BoxCollider3D.Create(new Vector3F(1, 0.5f, 1));
            Engine.Collision3DWorld.RegistCollider(col);

            var col2 = SphereCollider3D.Create(0.5f);
            col2.Position = node2.Position;
            Engine.Collision3DWorld.RegistCollider(col2);

            tc.Duration = 1000;
            tc.LoopBody(c =>
            {
                node2.Position += new Vector3F(0.01f, 0, 0);
                col2.Position = node2.Position;

                Engine.Collision3DWorld.Update();

                if (col.GetIsCollidedWith(col2))
                {
                    node.Quaternion = Quaternion.Euler(new Vector3F(c, c, 0));
                }
                col.Rotation = node.Quaternion;

            }, null);

            tc.End();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Raycast()
        {
            var tc = new TestCore(new Configuration() { IsResizable = true });
            tc.Init();

            var model = Model3D.CreateBox(new Vector3F(1f, 0.5f, 0.7f));
            var node = model.ToPolygon3DNode(new Color(200, 200, 200));
            var lighting = new DirectionalLightingNode() { LightDirection = new Vector3F(0, -1, 1) };
            node.AddChildNode(lighting);
            node.AddChildNode(model.ToPolygon3DNodeLine(new Color(0, 0, 0, 255)));
            Engine.AddNode(node);
            node.Position = new Vector3F();

            var node2 = new Polygon3DNode();
            node2.Material = Material.Line;
            Vector3F from = new Vector3F(2, 1, -4);
            Vector3F to = new Vector3F(-3, 0, 2);
            var vertexes = new Vertex[]
            {
                new Vertex(from, new Vector3F(), new Color(255, 0, 0), new Vector2F(), new Vector2F()),
                new Vertex(to, new Vector3F(), new Color(255, 0, 0), new Vector2F(), new Vector2F()),
                new Vertex(from, new Vector3F(), new Color(255, 0, 0), new Vector2F(), new Vector2F())
            };
            node2.Vertexes = vertexes;
            node2.Buffers = new int[] { 0, 1};
            Engine.AddNode(node2);
            var col = BoxCollider3D.Create(new Vector3F(1, 0.5f, 1));
            Engine.Collision3DWorld.RegistCollider(col);


            tc.Duration = 1000;
            tc.LoopBody(c =>
            {
                Engine.Collision3DWorld.Update();
                Collider3D collider = Engine.Collision3DWorld.GetRaycastHitClosestCollider(from, to, out var v);
                if (collider != null)
                {
                    Engine.Log.Info(LogCategory.Engine, $"{col == collider} {v}");
                    node.Quaternion = Quaternion.Euler(new Vector3F(c, c, 0));
                }
                col.Rotation = node.Quaternion;

                vertexes[0] = new Vertex(from, new Vector3F(), new Color(255, 0, 0), new Vector2F(), new Vector2F());
                vertexes[1] = new Vertex(to, new Vector3F(), new Color(255, 0, 0), new Vector2F(), new Vector2F());
                node2.SetVertexes(vertexes.AsSpan(), false);
                to += new Vector3F(0.01f, 0, 0);
            }, null);

            tc.End();
        }
    }
}