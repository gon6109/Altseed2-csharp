﻿using System;

namespace Altseed2
{
    /// <summary>
    /// 三角形を描画するノードのクラス
    /// </summary>
    [Serializable]
    public class TriangleNode : PolygonNode
    {
        private bool _RequireUpdateVertexes = false;

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get => _Color;
            set
            {
                if (_Color == value) return;

                _Color = value;
                OverwriteVertexColor(value);
            }
        }
        private Color _Color = new Color(255, 255, 255);

        /// <summary>
        /// 頂点1を取得または設定します。
        /// </summary>
        public Vector2F Point1
        {
            get => _Point1;
            set
            {
                if (_Point1 == value) return;

                _Point1 = value;
                _RequireUpdateVertexes = true;
            }
        }
        private Vector2F _Point1;

        /// <summary>
        /// 頂点2を取得または設定します。
        /// </summary>
        public Vector2F Point2
        {
            get => _Point2;
            set
            {
                if (_Point2 == value) return;

                _Point2 = value;
                _RequireUpdateVertexes = true;
            }
        }
        private Vector2F _Point2;

        /// <summary>
        /// 頂点3を取得または設定します。
        /// </summary>
        public Vector2F Point3
        {
            get => _Point3;
            set
            {
                if (_Point3 == value) return;
                _Point3 = value;
                _RequireUpdateVertexes = true;
            }
        }
        private Vector2F _Point3;

        internal void UpdateInheritedTransform()
        {
            if (_RequireUpdateVertexes)
            {
                UpdateVertexes();
                _RequireUpdateVertexes = false;
            }

            var array = _RenderedPolygon.Vertexes;
            MathHelper.GetMinMax(out var min, out var max, array);
            var size = max - min;

            var mat = new Matrix44F();
            switch (ScalingMode)
            {
                case ScalingMode.Fill:
                    mat = Matrix44F.GetScale2D(Size / size);
                    break;
                case ScalingMode.KeepAspect:
                    var scale = Size;

                    if (Size.X / Size.Y > size.X / size.Y)
                        scale.X = size.X * Size.Y / size.Y;
                    else
                        scale.Y = size.Y * Size.X / size.X;

                    scale /= size;

                    mat = Matrix44F.GetScale2D(scale);
                    break;
                case ScalingMode.Manual:
                    mat = Matrix44F.Identity;
                    break;
                default:
                    break;
            }
            mat *= Matrix44F.GetTranslation2D(-min);
        }

        private void UpdateVertexes()
        {
            var positions = new Vector2F[3];
            positions[0] = _Point1;
            positions[1] = _Point2;
            positions[2] = _Point3;

            var array = Vector2FArray.Create(positions.Length);
            array.FromArray(positions);
            _RenderedPolygon.CreateVertexesByVector2F(array);
            _RenderedPolygon.OverwriteVertexesColor(_Color);
        }

        internal override void Update()
        {
            if (_RequireUpdateVertexes)
            {
                UpdateVertexes();
                _RequireUpdateVertexes = false;
            }

            base.Update();
        }
    }
}
