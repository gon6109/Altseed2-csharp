﻿using NUnit.Framework;

using System;
using System.Threading;

namespace Altseed2.Test
{
    [TestFixture]
    class Math
    {
        [Test, Apartment(ApartmentState.STA)]
        public void CalcFromTransform2D()
        {
            Calc(new Vector2F(30, 30), 0, new Vector2F(1f, 1f));
            Calc(new Vector2F(30, 30), 30, new Vector2F(1f, 1f));
            Calc(new Vector2F(30, 30), 0, new Vector2F(2f, 2f));
            Calc(new Vector2F(30, 30), 30, new Vector2F(2f, 2f));
            Calc(new Vector2F(30, 30), 0, new Vector2F(3f, 2f));
            Calc(new Vector2F(30, 30), 30, new Vector2F(3f, 2f));

            static void Calc(Vector2F position, float angle, Vector2F scale)
            {
                var transform = MathHelper.CalcTransform(position, MathHelper.DegreeToRadian(angle), scale);
                MathHelper.CalcFromTransform2D(transform, out var p, out var s, out var a);
                TestValue(position, p);
                TestValue(scale, s);
                TestValue(angle, a);
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void CalcFromTransform3D()
        {
            Calc(new Vector3F(30, 30, 30), 0, new Vector3F(1f, 1f, 1f));
            Calc(new Vector3F(30, 30, 30), 30, new Vector3F(1f, 1f, 1f));
            Calc(new Vector3F(30, 30, 30), 0, new Vector3F(2f, 2f, 2f));
            Calc(new Vector3F(30, 30, 30), 30, new Vector3F(2f, 2f, 2f));
            Calc(new Vector3F(30, 30, 30), 0, new Vector3F(3f, 2f, 5f));
            Calc(new Vector3F(30, 30, 30), 30, new Vector3F(3f, 2f, 5f));

            static void Calc(Vector3F position, float angle, Vector3F scale)
            {
                var radian = MathHelper.DegreeToRadian(angle);
                var rotation = Matrix44F.GetRotationX(radian) * Matrix44F.GetRotationY(radian) * Matrix44F.GetRotationZ(radian);
                var transform = Matrix44F.GetTranslation3D(position) * rotation * Matrix44F.GetScale3D(scale);

                MathHelper.CalcFromTransform3D(transform, out var p, out var s, out var r);
                TestValue(position, p);
                TestValue(scale, s);
                TestValue(rotation, r);
            }
        }

        [TestCase(5f, 5f, 10f)]
        [TestCase(5f, 5f, 0f)]
        [TestCase(0f, 0f, 12f)]
        public void Length2D(float sourceX, float sourceY, float newLength)
        {
            var sourceVector = new Vector2F(sourceX, sourceY);
            sourceVector.Length = newLength;
            TestValue(newLength, sourceVector.Length);
        }

        [TestCase(5f, 5f, 2f, 10f)]
        [TestCase(5f, 5f, 2f, 0f)]
        [TestCase(0f, 0f, 0f, 12f)]
        public void Length3D(float sourceX, float sourceY, float sourceZ, float newLength)
        {
            var sourceVector = new Vector3F(sourceX, sourceY, sourceZ);
            sourceVector.Length = newLength;
            TestValue(newLength, sourceVector.Length);
        }

        [TestCase(5f, 5f, 2f, 2f, 10f)]
        [TestCase(5f, 5f, 2f, 2f, 0f)]
        [TestCase(0f, 0f, 0f, 0f, 12f)]
        public void Length4D(float sourceX, float sourceY, float sourceZ, float sourceW, float newLength)
        {
            var sourceVector = new Vector4F(sourceX, sourceY, sourceZ, sourceW);
            sourceVector.Length = newLength;
            TestValue(newLength, sourceVector.Length);
        }


        [TestCase(45, 0, 0)]
        [TestCase(45, 60, 0)]
        [TestCase(25, 0, 130)]
        public void QuaternionEuler(float x, float y, float z)
        {
            var euler = new Vector3F(x, y, z);
            var q = Quaternion.Euler(euler);
            TestValue(euler, q.EulerAngles);
        }


        [TestCase(45, 0, 0)]
        [TestCase(45, 60, 0)]
        [TestCase(45, -125, 0)]
        public void QuaternionRotate(float x, float y, float z)
        {
            var euler = new Vector3F(x, y, z);
            var q = Quaternion.Euler(euler);
            var qmat = Matrix44F.GetQuaternion(q);

            var mat = (Matrix44F.GetRotationX(MathHelper.DegreeToRadian(-euler.X)) *
                Matrix44F.GetRotationY(MathHelper.DegreeToRadian(-euler.Y)) *
                Matrix44F.GetRotationZ(MathHelper.DegreeToRadian(-euler.Z))).Transposed;

            TestValue(mat, qmat);
        }

        [Test]
        public void Inverse()
        {
            var mat = new Matrix33F();
            mat[0, 0] = 1;
            mat[1, 0] = 2;
            mat[2, 0] = 1;
            mat[0, 1] = 2;
            mat[1, 1] = 1;
            mat[2, 1] = 1;
            mat[0, 2] = 1;
            mat[1, 2] = 0;
            mat[2, 2] = 2;
            var ans = new Matrix33F();
            ans[0, 0] = -0.4f;
            ans[1, 0] = 0.8f;
            ans[2, 0] = -0.2f;
            ans[0, 1] = 0.6f;
            ans[1, 1] = -0.2f;
            ans[2, 1] = -0.2f;
            ans[0, 2] = 0.2f;
            ans[1, 2] = -0.4f;
            ans[2, 2] = 0.6f;
            var matinv = mat.Inversion;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    InRange(matinv[x, y], ans[x, y], 0.01f);
                }
            }
            var i = mat * matinv;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    InRange(i[x, y], Matrix33F.Identity[x, y], 0.01f);
                }
            }
            var matinvinv = matinv.Inversion;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    InRange(matinvinv[x, y], mat[x, y], 0.01f);
                }
            }
        }

        static void InRange(float actual, float target, float eps)
        {
            Assert.LessOrEqual(actual, target + eps);
            Assert.GreaterOrEqual(actual, target - eps);
        }

        public static void TestValue(float left, float right)
        {
            if (float.IsNaN(left) || float.IsNaN(right))
            {
                throw new AssertionException($"left: {left}\nright: {right}");
            }

            if (MathF.Abs(left - right) >= MathHelper.MatrixError) throw new AssertionException($"left: {left}\nright: {right}");
        }
        public static void TestValue(Vector2F left, Vector2F right)
        {
            if (MathF.Abs(left.X - right.X) >= MathHelper.MatrixError) throw new AssertionException($"At X:\nleft: {left.X}\nright: {right.X}");
            if (MathF.Abs(left.Y - right.Y) >= MathHelper.MatrixError) throw new AssertionException($"At Y:\nleft: {left.Y}\nright: {right.Y}");
        }
        public static void TestValue(Vector3F left, Vector3F right)
        {
            if (MathF.Abs(left.X - right.X) >= MathHelper.MatrixError) throw new AssertionException($"At X:\nleft: {left.X}\nright: {right.X}");
            if (MathF.Abs(left.Y - right.Y) >= MathHelper.MatrixError) throw new AssertionException($"At Y:\nleft: {left.Y}\nright: {right.Y}");
            if (MathF.Abs(left.Z - right.Z) >= MathHelper.MatrixError) throw new AssertionException($"At Z:\nleft: {left.Z}\nright: {right.Z}");
        }
        public static void TestValue(Matrix33F left, Matrix33F right)
        {
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (MathF.Abs(left[x, y] - right[x, y]) >= MathHelper.MatrixError)
                        throw new AssertionException($"At {x}, {y}:\nleft: {left[x, y]}\nright: {right[x, y]}");
        }
        public static void TestValue(Matrix44F left, Matrix44F right)
        {
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (MathF.Abs(left[x, y] - right[x, y]) >= MathHelper.MatrixError)
                        throw new AssertionException($"At {x}, {y}:\nleft: {left[x, y]}\nright: {right[x, y]}");
        }
    }
}
