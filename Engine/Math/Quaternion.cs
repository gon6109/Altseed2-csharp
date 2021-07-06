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
        /// オイラー角からクォータニオンを取得します
        /// </summary>
        /// <param name="euler"></param>
        /// <returns></returns>
        public static Quaternion Euler(Vector3F euler)
        {
            var radian = new Vector3F(MathHelper.DegreeToRadian(euler.X), MathHelper.DegreeToRadian(euler.Y), MathHelper.DegreeToRadian(euler.Z));

            var cX = MathF.Cos(radian.X / 2);
            var sX = MathF.Sin(radian.X / 2);

            var cY = MathF.Cos(radian.Y / 2);
            var sY = MathF.Sin(radian.Y / 2);

            var cZ = MathF.Cos(radian.Z / 2);
            var sZ = MathF.Sin(radian.Z / 2);

            var qX = new Quaternion(cX, sX, 0, 0);
            var qY = new Quaternion(cY, 0, sY, 0);
            var qZ = new Quaternion(cZ, 0, 0, sZ);

            return qY * qX * qZ;
        }

        /// <summary>
        /// fromDirection から toDirection への回転を作成します。
        /// </summary>
        /// <param name="fromDirection"></param>
        /// <param name="toDirection"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Vector3F fromDirection, Vector3F toDirection)
        {
            Vector3F axis = Vector3F.Cross(fromDirection, toDirection);
            float angle = Vector3F.Angle(fromDirection, toDirection);
            return AngleAxis(angle, axis.Normal);
        }

        /// <summary>
        /// axis の周りを angle 度回転する回転を作成します。
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Quaternion AngleAxis(float angle, Vector3F axis)
        {
            axis.Normalize();
            float rad = MathHelper.DegreeToRadian(angle) * 0.5f;
            axis *= MathF.Sin(rad);
            return new Quaternion(MathF.Cos(rad), axis.X, axis.Y, axis.Z);
        }

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
            get
            {
                var mat = Matrix44F.GetQuaternion(this);
                return MathHelper.MatrixToEuler(mat) * MathHelper.RadianToDegree(1f);
            }

            set
            {
                this = Euler(value);
            }
        }

        readonly Vector3F ImaginaryPart => new Vector3F(X, Y, Z);


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
            return new Quaternion(q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z,
                                  q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,
                                  q1.W * q2.Y + q1.Y * q2.W + q1.Z * q2.X - q1.X * q2.Z,
                                  q1.W * q2.Z + q1.Z * q2.W + q1.X * q2.Y - q1.Y * q2.X);
        }

        /// <summary>
        /// クォータニオンと3次元ベクトルを積算します．
        /// </summary>
        /// <param name="rotation">クォータニオン</param>
        /// <param name="point">3次元ベクトル</param>
        /// <returns></returns>
        public static Vector3F operator *(Quaternion rotation, Vector3F point)
        {
            var num = rotation.X * 2;
            var num2 = rotation.Y * 2;
            var num3 = rotation.Z * 2;
            var num4 = rotation.X * num;
            var num5 = rotation.Y * num2;
            var num6 = rotation.Z * num3;
            var num7 = rotation.X * num2;
            var num8 = rotation.X * num3;
            var num9 = rotation.Y * num3;
            var num10 = rotation.W * num;
            var num11 = rotation.W * num2;
            var num12 = rotation.W * num3;
            Vector3F result;
            result.X = (1f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z;
            result.Y = (num7 + num12) * point.X + (1f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z;
            result.Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1f - (num4 + num5)) * point.Z;
            return result;
        }

        /// <summary>
        /// 回転行列からクオータニオンに変換する
        /// </summary>
        /// <returns></returns>
        public static Quaternion FromMatrix44F(Matrix44F rotation)
        {
            Quaternion q = new Quaternion();

            // 最大成分を検索
            float[] elem = new float[4]; // 0:x, 1:y, 2:z, 3:w
            elem[0] = rotation[0, 0] - rotation[1, 1] - rotation[2, 2] + 1.0f;
            elem[1] = -rotation[0, 0] + rotation[1, 1] - rotation[2, 2] + 1.0f;
            elem[2] = -rotation[0, 0] - rotation[1, 1] + rotation[2, 2] + 1.0f;
            elem[3] = rotation[0, 0] + rotation[1, 1] + rotation[2, 2] + 1.0f;

            int biggestIndex = 0;
            for (int i = 1; i < 4; i++)
            {
                if (elem[i] > elem[biggestIndex])
                    biggestIndex = i;
            }

            if (elem[biggestIndex] < 0.0f)
            {
                Engine.Log.Warn(LogCategory.Engine, "Is not rotation matrix.");
                return Identity;
            }

            // 最大要素の値を算出
            float v = MathF.Sqrt(elem[biggestIndex]) * 0.5f;
            switch (biggestIndex)
            {
                case 0:
                    q.X = v;
                    break;
                case 1:
                    q.Y = v;
                    break;
                case 2:
                    q.Z = v;
                    break;
                case 3:
                    q.W = v;
                    break;
                default:
                    break;
            }
            float mult = 0.25f / v;

            switch (biggestIndex)
            {
                case 0: // x
                    q.Y = (rotation[0, 1] + rotation[1, 0]) * mult;
                    q.Z = (rotation[2, 0] + rotation[0, 2]) * mult;
                    q.W = (rotation[1, 2] - rotation[2, 1]) * mult;
                    break;
                case 1: // y
                    q.X = (rotation[0, 1] + rotation[1, 0]) * mult;
                    q.Z = (rotation[1, 2] + rotation[2, 1]) * mult;
                    q.W = (rotation[2, 0] - rotation[0, 2]) * mult;
                    break;
                case 2: // z
                    q.X = (rotation[2, 0] + rotation[0, 2]) * mult;
                    q.Y = (rotation[1, 2] + rotation[2, 1]) * mult;
                    q.W = (rotation[0, 1] - rotation[1, 0]) * mult;
                    break;
                case 3: // w
                    q.X = (rotation[1, 2] - rotation[2, 1]) * mult;
                    q.Y = (rotation[2, 0] - rotation[0, 2]) * mult;
                    q.Z = (rotation[0, 1] - rotation[1, 0]) * mult;
                    break;
            }

            return q;
        }

        #endregion

    }
}
