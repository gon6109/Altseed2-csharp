using System;

namespace Altseed2
{
    /// <summary>
    /// カメラとして機能するノードのクラス
    /// </summary>
    [Serializable]
    public class Camera3DNode : Transform3DNode
    {
        private readonly RenderedCamera3D _Camera;
        internal RenderedCamera3D RenderedCamera => _Camera;

        /// <summary>
        /// 描画対象のグループを取得または設定します。
        /// </summary>
        public ulong Group
        {
            get => _Group;
            set
            {
                if (_Group == value) return;

                var old = _Group;
                _Group = value;

                if (Status == RegisteredStatus.Registered)
                    Engine.UpdateCamera3DNodeGroup(this, old);
            }
        }
        private ulong _Group = 0;

        /// <summary>
        /// 視野角(度数法)を取得または設定します。
        /// </summary>
        public float FoV
        {
            get => _FoV;
            set
            {
                if (_FoV == value) return;

                _FoV = value;
                _RequireCalcTransform = true;
            }
        }
        private float _FoV = 60.0f;

        /// <summary>
        /// 描画する最短距離を取得または設定します。
        /// </summary>
        public float ClippingStart
        {
            get => _ClippingStart;
            set
            {
                if (_ClippingStart == value) return;

                _ClippingStart = value;
                _RequireCalcTransform = true;
            }
        }
        private float _ClippingStart = 0.1f;

        /// <summary>
        /// 描画する最遠距離を取得または設定します。
        /// </summary>
        public float ClippingEnd
        {
            get => _ClippingEnd;
            set
            {
                if (_ClippingEnd == value) return;

                _ClippingEnd = value;
                _RequireCalcTransform = true;
            }
        }
        private float _ClippingEnd = 100f;

        /// <summary>
        /// 何も描画されていない部分の色を取得または設定します。
        /// </summary>
        public Color ClearColor
        {
            get => _ClearColor;
            set
            {
                if (_ClearColor == value) return;
                _ClearColor = value;
                _Camera.RenderPassParameter = new RenderPassParameter(_ClearColor, _IsColorCleared, true);
            }
        }
        private Color _ClearColor;

        /// <summary>
        /// 描画開始時に<see cref="ClearColor"/>で描画先を塗りつぶすかどうかを取得または設定します。
        /// </summary>
        public bool IsColorCleared
        {
            get => _IsColorCleared;
            set
            {
                if (_IsColorCleared == value) return;
                _IsColorCleared = value;
                _Camera.RenderPassParameter = new RenderPassParameter(_ClearColor, _IsColorCleared, true);
            }
        }
        private bool _IsColorCleared;

        /// <summary>
        /// <see cref="CameraNode"/>新しいインスタンスを生成します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Graphics機能が初期化されていない。</exception>
        public Camera3DNode()
        {
            if (!Engine.Config.EnabledCoreModules.HasFlag(CoreModules.Graphics))
            {
                throw new InvalidOperationException("Graphics機能が初期化されていません。");
            }

            _Camera = RenderedCamera3D.Create();

            ClearColor = new Color(50, 50, 50);
            IsColorCleared = false;
        }

        /// <summary>
        /// 描画先のテクスチャを取得または設定します。
        /// </summary>
        public RenderTexture TargetTexture
        {
            get => _Camera.TargetTexture;
            set
            {
                if (value == _Camera.TargetTexture) return;
                _Camera.TargetTexture = value;
            }
        }

        private Vector2I _TargetSize = default;

        internal override void Update()
        {
            base.Update();

            if (_RequireCalcTransform || _TargetSize != (TargetTexture?.Size ?? Engine.WindowSize))
            {
                _TargetSize = (TargetTexture?.Size ?? Engine.WindowSize);

                var at = new Vector3F(0, 0, 1) - CenterPosition;
                at = Matrix44F.GetQuaternion(Quaternion).Transform3D(at);

                var up = new Vector3F(0, 1, 0);
                up = Matrix44F.GetQuaternion(Quaternion).Transform3D(up);

                RenderedCamera.ViewMatrix =
                Matrix44F.GetPerspectiveFovLH(MathHelper.DegreeToRadian(FoV), (float)_TargetSize.X / _TargetSize.Y, ClippingStart, ClippingEnd)
                * Matrix44F.GetLookAtLH(Position - CenterPosition, Position + at, up);

                _RequireCalcTransform = false;
            }
        }

        #region Node

        internal override void Registered()
        {
            base.Registered();
            Engine.RegisterCamera3DNode(this);
        }

        internal override void Unregistered()
        {
            base.Unregistered();
            Engine.UnregisterCamera3DNode(this);
        }

        #endregion
    }
}
