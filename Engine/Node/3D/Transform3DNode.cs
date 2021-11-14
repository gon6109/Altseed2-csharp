using System;

namespace Altseed2
{
    /// <summary>
    /// 3D変形行列を備えたノードのクラス
    /// </summary>
    [Serializable]
    public class Transform3DNode : Node
    {
        /// <summary>
        /// <see cref="Transform3DNode"/>の新しいインスタンスを生成します。
        /// </summary>
        public Transform3DNode()
        {
        }

        internal Matrix44F Transform
        {
            get => _Transform;
            set
            {
                _Transform = value;
            }
        }
        [NonSerialized]
        private Matrix44F _Transform = Matrix44F.Identity;

        private protected bool _RequireCalcTransform = true;

        /// <summary>
        /// 先祖の変形を加味した変形行列を取得します。
        /// </summary>
        public virtual Matrix44F InheritedTransform
        {
            get => _InheritedTransform;
            private protected set
            {
                _InheritedTransform = value;
                _AbsoluteTransform = value;
            }
        }
        private Matrix44F _InheritedTransform = Matrix44F.Identity;

        /// <summary>
        /// 先祖の変形および<see cref="CenterPosition"/>を加味した最終的な変形行列を取得します。
        /// </summary>
        public Matrix44F AbsoluteTransform
        {
            get => _AbsoluteTransform;
            set
            {
                _AbsoluteTransform = value;
            }
        }
        private Matrix44F _AbsoluteTransform = Matrix44F.Identity;

        /// <summary>
        /// クォータニオンを取得または設定します。
        /// </summary>
        public Quaternion Quaternion
        {
            get => _Quaternion;
            set
            {
                if (_Quaternion == value) return;

                _Quaternion = value;
                _RequireCalcTransform = true;
            }
        }
        private Quaternion _Quaternion = Quaternion.Identity;

        /// <summary>
        /// 座標を取得または設定します。
        /// </summary>
        public Vector3F Position
        {
            get => _Position;
            set
            {
                if (_Position == value) return;

                _Position = value;
                _RequireCalcTransform = true;
            }
        }
        private Vector3F _Position = new Vector3F();

        /// <summary>
        /// 中心となる座標をピクセル単位で取得または設定します。
        /// </summary>
        public Vector3F CenterPosition
        {
            get => _CenterPosition;
            set
            {
                if (_CenterPosition == value) return;

                _CenterPosition = value;
                _RequireCalcTransform = true;
            }
        }
        private Vector3F _CenterPosition;

        /// <summary>
        /// 拡大率を取得または設定します。
        /// </summary>
        public Vector3F Scale
        {
            get => _Scale;
            set
            {
                if (value == _Scale) return;

                _Scale = value;
                _RequireCalcTransform = true;
            }
        }
        private Vector3F _Scale = new Vector3F(1.0f, 1.0f, 1.0f);

        /// <summary>
        /// コンテンツのサイズを取得します。
        /// </summary>
        public virtual Vector3F ContentSize { get; }

        internal override void Registered()
        {
            UpdateTransform();
            base.Registered();
        }

        internal override void Update()
        {
            base.Update();

            if (_RequireCalcTransform)
                UpdateTransform();
        }

        /// <summary>
        /// <see cref="InheritedTransform"/>を再計算します。
        /// 直近先祖の<see cref="InheritedTransform"/>も考慮した上で最終的な変形を計算し、
        /// 既存の子孫ノードにも伝播します。
        /// </summary>
        private void UpdateTransform()
        {
            CalcTransform();

            var ancestor = GetAncestorSpecificNode<TransformNode>();
            PropagateTransform(this, ancestor?.InheritedTransform ?? Matrix44F.Identity);
        }

        /// <summary>
        /// <see cref="Transform"/> を再計算します。
        /// </summary>
        private protected virtual void CalcTransform()
        {
            Transform = MathHelper.CalcTransform3D(Position, Quaternion, Scale);

            _RequireCalcTransform = false;
        }

        /// <summary>
        /// 子孫ノードのうち<see cref="Transform3DNode"/>に対して変換行列を伝播させます。
        /// </summary>
        private void PropagateTransform(Node node, Matrix44F matrix)
        {
            if (node is Transform3DNode s)
            {
                matrix = matrix * s.Transform;
                s.InheritedTransform = matrix;
            }

            foreach (var child in node.Children)
            {
                PropagateTransform(child, matrix);
            }
        }
    }
}
