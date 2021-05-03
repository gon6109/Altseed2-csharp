using System;
using System.Security.Cryptography.X509Certificates;

namespace Altseed2
{
    /// <summary>
    /// 数学の演算を補助するクラス
    /// </summary>
    public static class MathHelper
    {
        private const float PiPer180 = (MathF.PI / 180f);
        /// <summary>
        /// 行列で使用
        /// 誤差
        /// </summary>
        internal const float MatrixError = 0.0001f;

        /// <summary>
        /// 指定した値を決められた範囲に丸めます。
        /// </summary>
        /// <param name="v">丸める値</param>
        /// <param name="max">最大値</param>
        /// <param name="min">最小値</param>
        /// <returns><paramref name="v"/>が<paramref name="max"/>以上であったり<paramref name="min"/>未満であった場合はその値が，それ以外では<paramref name="v"/>そのまま返される</returns>
        public static int Clamp(int v, int max, int min)
        {
            if (v > max) return max;
            if (v < min) return min;
            return v;
        }

        /// <summary>
        /// 指定した値を決められた範囲に丸めます。
        /// </summary>
        /// <param name="v">丸める値</param>
        /// <param name="max">最大値</param>
        /// <param name="min">最小値</param>
        /// <returns><paramref name="v"/>が<paramref name="max"/>以上であったり<paramref name="min"/>未満であった場合はその値が，それ以外では<paramref name="v"/>そのまま返される</returns>
        public static float Clamp(float v, float max, float min)
        {
            if (v > max) return max;
            if (v < min) return min;
            return v;
        }

        /// <summary>
        /// 指定した値を決められた範囲に丸めます。
        /// </summary>
        /// <param name="v">丸める値</param>
        /// <param name="max">最大値</param>
        /// <param name="min">最小値</param>
        /// <returns><paramref name="v"/>が<paramref name="max"/>以上であったり<paramref name="min"/>未満であった場合はその値が，それ以外では<paramref name="v"/>そのまま返される</returns>
        public static T Clamp<T>(T v, T max, T min) where T : IComparable<T>
        {
            if (v.CompareTo(max) > 0) return max;
            if (v.CompareTo(min) < 0) return min;
            return v;
        }

        /// <summary>
        /// 度数法の値を弧度法の値に変換します。
        /// </summary>
        /// <param name="degree">変換したい度数法の値</param>
        /// <returns>弧度法としての<paramref name="degree"/>の値</returns>
        public static float DegreeToRadian(float degree) => degree * PiPer180;

        /// <summary>
        /// 弧度法の値を度数法の値に変換します。
        /// </summary>
        /// <param name="radian">変換したい弧度法の値</param>
        /// <returns>度数法としての<paramref name="radian"/>の値</returns>
        public static float RadianToDegree(float radian) => radian / PiPer180;

        /// <summary>
        /// Transformを計算します。
        /// </summary>
        /// <param name="position">座標</param>
        /// <param name="angle">角度（弧度法）</param>
        /// <param name="scale">拡大率</param>
        /// <returns></returns>
        internal static Matrix44F CalcTransform(Vector2F position, float angle, Vector2F scale)
        {
            var matPosition = Matrix44F.GetTranslation2D(position);
            var matRotate = Matrix44F.GetRotationZ(angle);
            var matScale = Matrix44F.GetScale2D(scale);

            return matPosition * matRotate * matScale;
            // NOTE: 一気に計算したほうがよさそう
        }

        /// <summary>
        /// Transform3Dを計算します。
        /// </summary>
        /// <param name="position">座標</param>
        /// <param name="quaternion">回転</param>
        /// <param name="scale">拡大率</param>
        /// <returns></returns>
        internal static Matrix44F CalcTransform3D(Vector3F position, Quaternion quaternion, Vector3F scale)
        {
            var matPosition = Matrix44F.GetTranslation3D(position);
            var matRotate = Matrix44F.GetQuaternion(quaternion);
            var matScale = Matrix44F.GetScale3D(scale);

            return matPosition * matRotate * matScale;
            // NOTE: 一気に計算したほうがよさそう
        }

        /// <summary>
        /// 指定した頂点を全て含む長方形のうち左上と右下の座標を割り出します。
        /// </summary>
        /// <param name="min">左上の座標</param>
        /// <param name="max">右下の座標</param>
        /// <param name="positions">計算する座標</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/>がnull</exception>
        internal static void GetMinMax(out Vector2F min, out Vector2F max, VertexArray positions)
        {
            var min_x = float.MaxValue;
            var min_y = float.MaxValue;
            var max_x = float.MinValue;
            var max_y = float.MinValue;
            if (positions == null) throw new ArgumentNullException(nameof(positions), "引数がnullです");
            var count = positions.Count;
            if (count == 0)
            {
                min = max = default;
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var current = positions[i].Position;
                if (min_x > current.X) min_x = current.X;
                if (min_y > current.Y) min_y = current.Y;
                if (max_x < current.X) max_x = current.X;
                if (max_y < current.Y) max_y = current.Y;
            }
            min = new Vector2F(min_x, min_y);
            max = new Vector2F(max_x, max_y);
        }

        /// <summary>
        /// 指定した頂点を全て含む長方形のうち左上と右下の座標を割り出します。
        /// </summary>
        /// <param name="min">左上の座標</param>
        /// <param name="max">右下の座標</param>
        /// <param name="positions">計算する座標</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/>がnull</exception>
        internal static void GetMinMax(out Vector2F min, out Vector2F max, Vector2FArray positions)
        {
            var min_x = float.MaxValue;
            var min_y = float.MaxValue;
            var max_x = float.MinValue;
            var max_y = float.MinValue;
            if (positions == null) throw new ArgumentNullException(nameof(positions), "引数がnullです");
            var count = positions.Count;
            if (count == 0)
            {
                min = max = default;
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var current = positions[i];
                if (min_x > current.X) min_x = current.X;
                if (min_y > current.Y) min_y = current.Y;
                if (max_x < current.X) max_x = current.X;
                if (max_y < current.Y) max_y = current.Y;
            }
            min = new Vector2F(min_x, min_y);
            max = new Vector2F(max_x, max_y);
        }

        /// <summary>
        /// 指定した頂点を全て含む長方形のうち左上と右下の座標を割り出します。
        /// </summary>
        /// <param name="min">左上の座標</param>
        /// <param name="max">右下の座標</param>
        /// <param name="positions">計算する座標</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/>がnull</exception>
        internal static void GetMinMax(out Vector2F min, out Vector2F max, params Vector2F[] positions)
        {
            var min_x = float.MaxValue;
            var min_y = float.MaxValue;
            var max_x = float.MinValue;
            var max_y = float.MinValue;
            if (positions == null) throw new ArgumentNullException(nameof(positions), "引数がnullです");
            if (positions.Length == 0)
            {
                min = max = default;
                return;
            }
            foreach (var current in positions)
            {
                if (min_x > current.X) min_x = current.X;
                if (min_y > current.Y) min_y = current.Y;
                if (max_x < current.X) max_x = current.X;
                if (max_y < current.Y) max_y = current.Y;
            }
            min = new Vector2F(min_x, min_y);
            max = new Vector2F(max_x, max_y);
        }

        /// <summary>
        /// 指定した頂点を全て含む長方形のうち左上と右下の座標を割り出します。
        /// </summary>
        /// <param name="min">左上の座標</param>
        /// <param name="max">右下の座標</param>
        /// <param name="positions">計算する座標</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/>がnull</exception>
        internal static void GetMinMaxVector3F(out Vector3F min, out Vector3F max, VertexArray positions)
        {
            var min_x = float.MaxValue;
            var min_y = float.MaxValue;
            var min_z = float.MaxValue;
            var max_x = float.MinValue;
            var max_y = float.MinValue;
            var max_z = float.MinValue;
            if (positions == null) throw new ArgumentNullException(nameof(positions), "引数がnullです");
            var count = positions.Count;
            if (count == 0)
            {
                min = max = default;
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var current = positions[i].Position;
                if (min_x > current.X) min_x = current.X;
                if (min_y > current.Y) min_y = current.Y;
                if (min_z > current.Z) min_z = current.Z;
                if (max_x < current.X) max_x = current.X;
                if (max_y < current.Y) max_y = current.Y;
                if (max_z < current.Z) max_z = current.Z;
            }
            min = new Vector3F(min_x, min_y, min_z);
            max = new Vector3F(max_x, max_y, max_z);
        }

        /// <summary>
        /// 指定した頂点を全て含む長方形のうち左上と右下の座標を割り出します。
        /// </summary>
        /// <param name="min">左上の座標</param>
        /// <param name="max">右下の座標</param>
        /// <param name="positions">計算する座標</param>
        /// <exception cref="ArgumentNullException"><paramref name="positions"/>がnull</exception>
        internal static void GetMinMaxVector3F(out Vector3F min, out Vector3F max, params Vector3F[] positions)
        {
            var min_x = float.MaxValue;
            var min_y = float.MaxValue;
            var min_z = float.MaxValue;
            var max_x = float.MinValue;
            var max_y = float.MinValue;
            var max_z = float.MinValue;
            if (positions == null) throw new ArgumentNullException(nameof(positions), "引数がnullです");
            if (positions.Length == 0)
            {
                min = max = default;
                return;
            }
            foreach (var current in positions)
            {
                if (min_x > current.X) min_x = current.X;
                if (min_y > current.Y) min_y = current.Y;
                if (min_z > current.Z) min_z = current.Z;
                if (max_x < current.X) max_x = current.X;
                if (max_y < current.Y) max_y = current.Y;
                if (max_z < current.Z) max_z = current.Z;
            }
            min = new Vector3F(min_x, min_y, min_z);
            max = new Vector3F(max_x, max_y, max_z);
        }

        /// <summary>
        /// <see cref="Matrix44F"/>から2次元座標，拡大率，角度を算出します。
        /// </summary>
        /// <param name="transform">計算元となる4x4行列</param>
        /// <param name="absolutePosition">出力される座標</param>
        /// <param name="scale">出力される拡大率</param>
        /// <param name="angle">出力される角度(度数法)</param>
        public static void CalcFromTransform2D(Matrix44F transform, out Vector2F absolutePosition, out Vector2F scale, out float angle)
        {
            absolutePosition = new Vector2F(transform[0, 3], transform[1, 3]);
            var sx = new Vector3F(transform[0, 0], transform[1, 0], transform[2, 0]).Length;
            var sy = new Vector3F(transform[0, 1], transform[1, 1], transform[2, 1]).Length;
            scale = new Vector2F(sx, sy);
            transform *= Matrix44F.GetScale2D(new Vector2F(sx == 0 ? 1f : (1f / sx), sy == 0 ? 1f : (1f / sy)));
            angle = new Vector2F(transform[0, 0], transform[1, 0]).Degree;
        }

        /// <summary>
        /// <see cref="Matrix44F"/>から3次元座標，拡大率，角度を算出します。
        /// </summary>
        /// <param name="transform">計算元となる4x4行列</param>
        /// <param name="absolutePosition">出力される座標</param>
        /// <param name="scale">出力される拡大率</param>
        /// <param name="rotation">出力される回転行列</param>
        public static void CalcFromTransform3D(Matrix44F transform, out Vector3F absolutePosition, out Vector3F scale, out Matrix44F rotation)
        {
            absolutePosition = new Vector3F(transform[0, 3], transform[1, 3], transform[2, 3]);
            var sx = new Vector3F(transform[0, 0], transform[1, 0], transform[2, 0]).Length;
            var sy = new Vector3F(transform[0, 1], transform[1, 1], transform[2, 1]).Length;
            var sz = new Vector3F(transform[0, 2], transform[1, 2], transform[2, 2]).Length;
            scale = new Vector3F(sx, sy, sz);
            transform = Matrix44F.GetTranslation3D(-absolutePosition) * transform;
            rotation = transform * Matrix44F.GetScale3D(new Vector3F(sx == 0 ? 1f : (1f / sx), sy == 0 ? 1f : (1f / sy), sz == 0 ? 1f : (1f / sz)));
        }

        /// <summary>
        /// 回転行列からオイラー角を取得します．
        /// </summary>
        /// <param name="m">回転行列</param>
        /// <returns>オイラー角</returns>
        public static Vector3F MatrixToEuler(Matrix44F m)
        {
            Vector3F v = new Vector3F();
            if (m[1, 2] < 1)
            {
                if (m[1, 2] > -1)
                {
                    v.X = MathF.Asin(-m[1, 2]);
                    v.Y = MathF.Atan2(m[0, 2], m[2, 2]);
                    v.Z = MathF.Atan2(m[1, 0], m[1, 1]);
                }
                else
                {
                    v.X = MathF.PI * 0.5f;
                    v.Y = MathF.Atan2(m[0, 1], m[0, 0]);
                    v.Z = 0;
                }
            }
            else
            {
                v.X = -MathF.PI * 0.5f;
                v.Y = MathF.Atan2(-m[0, 1], m[0, 0]);
                v.Z = 0;
            }

            var array = new float[] { v.X, v.Y, v.Z };
            for (int i = 0; i < 3; i++)
            {
                if (array[i] < 0)
                {
                    array[i] += 2 * MathF.PI;
                }
                else if (array[i] > 2 * MathF.PI)
                {
                    array[i] -= 2 * MathF.PI;
                }
            }

            return new Vector3F(array[0], array[1], array[2]);
        }
    }
}
