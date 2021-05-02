using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Altseed2
{
    /// <summary>
    /// クォータニオン
    /// </summary>
    public struct Quaternion
    {
        /// <summary>
        /// 単位クオータニオンを取得します。
        /// </summary>
        public static Quaternion Identity => new Quaternion(1, 0, 0, 0);

        /// <summary>
        /// W成分
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float W;

        /// <summary>
        /// X成分
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float X;

        /// <summary>
        /// Y成分
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Y;

        /// <summary>
        /// Z成分
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Z;

        /// <summary>
        /// <see cref="Vector4F"/>の新しいインスタンスを生成します。
        /// </summary>
        /// <param name="x">X成分</param>
        /// <param name="y">Y成分</param>
        /// <param name="z">Z成分</param>
        /// <param name="w">W成分</param>
        public Quaternion(float w, float x, float y, float z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// オイラー角を設定取得する
        /// </summary>
        public Vector3F EulerAngles
        {
            get => new Vector3F(
                MathHelper.RadianToDegree(MathF.Atan(2 * (W * X + Y * Z) / (W * W - X * X - Y * Y + Z * Z))),
                MathHelper.RadianToDegree(MathF.Asin(2 * (W * Y + X * Z))),
                MathHelper.RadianToDegree(MathF.Atan(2 * (W * Z + X * Y) / (W * W + X * X - Y * Y - Z * Z))));

            set
            {
                var radian = new Vector3F(MathHelper.DegreeToRadian(value.X), MathHelper.DegreeToRadian(value.Y), MathHelper.DegreeToRadian(value.Z));

                var cosRoll = MathF.Cos(radian.X / 2.0f);
                var sinRoll = MathF.Sin(radian.X / 2.0f);
                var cosPitch = MathF.Cos(radian.Y / 2.0f);
                var sinPitch = MathF.Sin(radian.Y / 2.0f);
                var cosYaw = MathF.Cos(radian.Z / 2.0f);
                var sinYaw = MathF.Sin(radian.Z / 2.0f);

                W = cosRoll * cosPitch * cosYaw + sinRoll * sinPitch * sinYaw;
                X = sinRoll * cosPitch * cosYaw - cosRoll * sinPitch * sinYaw;
                Y = cosRoll * sinPitch * cosYaw + sinRoll * cosPitch * sinYaw;
                Z = cosRoll * cosPitch * sinYaw - sinRoll * sinPitch * cosYaw;
            }
        }

        readonly Vector3F ImaginaryPart => new Vector3F(X, Y, X);


        #region Equivalence
        /// <summary>
        /// 2つの<see cref="Quaternion"/>間の等価性を判定します。
        /// </summary>
        /// <param name="q1">等価性を判定するベクトル1</param>
        /// <param name="q2">等価性を判定するベクトル2</param>
        /// <returns><paramref name="q1"/> と <paramref name="q2"/> の間に等価性が認められたらtrue、それ以外でfalse</returns>
        public static bool Equals(Quaternion q1, Quaternion q2) => q1.Equals(q2);

        /// <summary>
        /// もう1つの<see cref="Quaternion"/>との等価性を判定します。
        /// </summary>
        /// <param name="other">比較する<see cref="Quaternion"/>のインスタンス</param>
        /// <returns><paramref name="other"/>等価性をが認められたらtrue、それ以外でfalse</returns>
        public readonly bool Equals(Quaternion other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        /// <summary>
        /// 指定したオブジェクトとの等価性を判定します。
        /// </summary>
        /// <param name="obj">等価性を判定するオブジェクト</param>
        /// <returns><paramref name="obj"/>との間に等価性が認められたらtrue、それ以外でfalse</returns>
        public readonly override bool Equals(object obj) => obj is Quaternion v && Equals(v);

        /// <summary>
        /// このオブジェクトのハッシュコードを返します。
        /// </summary>
        /// <returns>このオブジェクトのハッシュコード</returns>
        public readonly override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

        /// <summary>
        /// 二つの<see cref="Quaternion"/>の間の等価性を判定します。
        /// </summary>
        /// <param name="q1">等価性を判定する<see cref="Quaternion"/>のインスタンス</param>
        /// <param name="q2">等価性を判定する<see cref="Quaternion"/>のインスタンス</param>
        /// <returns><paramref name="q1"/> と <paramref name="q2"/> の間との等価性が認められたらtrue，それ以外でfalse</returns>
        public static bool operator ==(Quaternion q1, Quaternion q2) => Equals(q1, q2);

        /// <summary>
        /// 二つの<see cref="Quaternion"/>の間の非等価性を判定します。
        /// </summary>
        /// <param name="q1">非等価性を判定する<see cref="Quaternion"/>のインスタンス</param>
        /// <param name="q2">非等価性を判定する<see cref="Quaternion"/>のインスタンス</param>
        /// <returns><paramref name="q1"/> と <paramref name="q2"/> の間との非等価性が認められたらtrue，それ以外でfalse</returns>
        public static bool operator !=(Quaternion q1, Quaternion q2) => !Equals(q1, q2);
        #endregion

        #region CalOperators

        /// <summary>
        /// 2つのクォータニオンを積算します。
        /// </summary>
        /// <param name="q1">積算するクォータニオン1</param>
        /// <param name="q2">積算するクォータニオン2</param>
        /// <returns>積算結果</returns>
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            var imaginary = q1.W * q2.ImaginaryPart + q2.W * q1.ImaginaryPart + Vector3F.Cross(q1.ImaginaryPart, q2.ImaginaryPart);
            return new Quaternion(
                q1.W * q2.W - Vector3F.Dot(q1.ImaginaryPart, q2.ImaginaryPart),
                imaginary.X, imaginary.Y, imaginary.Z);
        }

        #endregion

    }
}
