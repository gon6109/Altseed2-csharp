using System;

namespace Altseed2
{
    /// <summary>
    /// テクスチャを描画するノードを表します。
    /// </summary>
    [Serializable]
    public class Sprite3DNode : Transform3DNode, IDrawn3D/*, ISized*/
    {
        private readonly RenderedSprite3D _RenderedSprite;

        /// <summary>
        /// <see cref="SpriteNode"/>の新しいインスタンスを生成します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Graphics機能が初期化されていない。</exception>
        public Sprite3DNode()
        {
            if (!Engine.Config.EnabledCoreModules.HasFlag(CoreModules.Graphics))
            {
                throw new InvalidOperationException("Graphics機能が初期化されていません。");
            }

            _RenderedSprite = RenderedSprite3D.Create();
        }

        #region IDrawn

        Rendered3D IDrawn3D.Rendered => _RenderedSprite;

        void IDrawn3D.Draw()
        {
            Engine.Renderer3D.DrawSprite(_RenderedSprite);
        }

        /// <summary>
        /// カメラグループを取得または設定します。
        /// </summary>
        public ulong CameraGroup
        {
            get => _CameraGroup;
            set
            {
                if (_CameraGroup == value) return;
                var old = _CameraGroup;
                _CameraGroup = value;

                if (IsRegistered)
                    Engine.UpdateDrawnCamera3DGroup(this, old);
            }
        }
        private ulong _CameraGroup;

        /// <summary>
        /// 描画時の重ね順を取得または設定します。
        /// </summary>
        public int ZOrder
        {
            get => _ZOrder;
            set
            {
                if (value == _ZOrder) return;

                var old = _ZOrder;
                _ZOrder = value;

                if (IsRegistered)
                    Engine.UpdateDrawn3DZOrder(this, old);
            }
        }
        private int _ZOrder;

        /// <summary>
        /// このノードを描画するかどうかを取得または設定します。
        /// </summary>
        public bool IsDrawn
        {
            get => _IsDrawn;
            set
            {
                if (_IsDrawn == value) return;
                _IsDrawn = value;
                this.UpdateIsDrawn3DActuallyOfDescendants();
            }
        }
        private bool _IsDrawn = true;

        /// <summary>
        /// 先祖の<see cref="IsDrawn"/>を考慮して、このノードを描画するかどうかを取得します。
        /// </summary>
        public bool IsDrawnActually => (this as ICullableDrawn).IsDrawnActually;
        bool IDrawn3D.IsDrawnActually { get; set; } = true;

        #endregion

        #region Node

        internal override void Registered()
        {
            base.Registered();
            Engine.RegisterDrawn3D(this);
        }

        internal override void Unregistered()
        {
            base.Unregistered();
            Engine.UnregisterDrawn3D(this);
        }

        /// <inheritdoc/>
        public override void FlushQueue()
        {
            base.FlushQueue();
            this.UpdateIsDrawn3DActuallyOfDescendants();
        }

        #endregion

        #region RenderedSprite

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get => _RenderedSprite.Color;
            set
            {
                if (_RenderedSprite.Color == value) return;
                _RenderedSprite.Color = value;
            }
        }

        /// <summary>
        /// ブレンドモードを取得または設定します。
        /// </summary>
        public AlphaBlend AlphaBlend
        {
            get => _RenderedSprite.AlphaBlend;
            set
            {
                if (_RenderedSprite.AlphaBlend == value) return;
                _RenderedSprite.AlphaBlend = value;
            }
        }

        /// <summary>
        /// 描画範囲を取得または設定します。
        /// </summary>
        public RectF Src
        {
            get => _RenderedSprite.Src;
            set
            {
                if (_RenderedSprite.Src == value) return;
                _RenderedSprite.Src = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 描画するテクスチャを取得または設定します。
        /// </summary>
        public TextureBase Texture
        {
            get => _RenderedSprite.Texture;
            set
            {
                if (_RenderedSprite.Texture == value) return;
                _RenderedSprite.Texture = value;

                if (value != null)
                    Src = new RectF(0, 0, value.Size.X, value.Size.Y);
            }
        }

        /// <summary>
        /// 使用するマテリアルを取得または設定します。
        /// </summary>
        public Material Material
        {
            get => _RenderedSprite.Material;
            set
            {
                if (_RenderedSprite.Material == value) return;

                _RenderedSprite.Material = value;
            }
        }

        /// <inheritdoc/>
        public sealed override Matrix44F InheritedTransform
        {
            get => base.InheritedTransform;
            private protected set
            {
                base.InheritedTransform = value;
                AbsoluteTransform = value * Matrix44F.GetTranslation3D(-CenterPosition) * Matrix44F.GetScale3D(new Vector3F(1, 1, 1) * 0.01f);
                _RenderedSprite.Transform = AbsoluteTransform;
            }
        }

        #endregion

        /// <inheritdoc/>
        public sealed override Vector3F ContentSize => new Vector3F(Src.Size.X, Src.Size.Y, 0);
    }
}
