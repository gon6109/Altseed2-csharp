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
        public struct Particle : IEquatable<Particle>
        {
            public Vector2F Current;
            public Vector2F Next;
            public Vector2F Velocity;

            public readonly bool Equals(Particle other) => Current == other.Current && Next == other.Next && Velocity == other.Velocity;

            public readonly override bool Equals(object obj) => obj is Vertex v && Equals(v);

            public readonly override int GetHashCode() => HashCode.Combine(Current, Next, Velocity);

            public static bool operator ==(Particle v1, Particle v2) => v1.Equals(v2);
            public static bool operator !=(Particle v1, Particle v2) => !v1.Equals(v2);
        };

        [Test, Apartment(ApartmentState.STA)]
        public void SpriteNodeWithMaterial()
        {
            var tc = new TestCore();
            tc.Init();

            var texture = Texture2D.Load(@"TestData/IO/AltseedPink.png");
            Assert.NotNull(texture);

            var buffer = Buffer<Particle>.Create(BufferUsageType.Compute, 10000);
            {
                var data = buffer.Lock();
                for (int i = 0; i < buffer.Count; i++)
                {
                    data[i] = new Particle()
                    {
                        Current = new Vector2F(i / 100 * 10, i % 100 * 10),
                        Velocity = new Vector2F(0, 0),
                    };
                }
                buffer.Unlock();
            }

            var 

            tc.LoopBody(null, null);

            tc.End();
        }
    }
}
