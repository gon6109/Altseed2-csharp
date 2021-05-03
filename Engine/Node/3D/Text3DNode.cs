using System;

namespace Altseed2
{
    /// <summary>
    /// テキストを描画するノードのクラス
    /// </summary>
    [Serializable]
    public class Text3DNode : Transform3DNode, IDrawn3D
    {
        private readonly RenderedText3D _RenderedText;

        /// <summary>
        /// 新しいインスタンスを生成します。
        /// </summary>
        /// <exception cref="InvalidOperationException">Graphics機能が初期化されていない。</exception>
        public Text3DNode()
        {
            if (!Engine.Config.EnabledCoreModules.HasFlag(CoreModules.Graphics))
            {
                throw new InvalidOperationException("Graphics機能が初期化されていません。");
            }

            _RenderedText = RenderedText3D.Create();
        }

        #region IDrawn3D

        Rendered3D IDrawn3D.Rendered => _RenderedText;

        void IDrawn3D.Draw()
        {
            Engine.Renderer3D.DrawText(_RenderedText);
        }

        /// <summary>
        /// カメラグループを取得または設定します。
        /// </summary>
        public ulong CameraGroup
        {
            get => _CameraGroup; set
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

        /// <inheritdoc/>
        public sealed override Matrix44F InheritedTransform
        {
            get => base.InheritedTransform;
            private protected set
            {
                base.InheritedTransform = value;
                AbsoluteTransform = value * Matrix44F.GetTranslation3D(-CenterPosition) * Matrix44F.GetScale3D(new Vector3F(1, 1, 1) * 0.01f);
                _RenderedText.Transform = AbsoluteTransform;
            }
        }

        #endregion

        #region RenderedText

        /// <summary>
        /// 色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get => _RenderedText.Color;
            set
            {
                if (_RenderedText.Color == value) return;
                _RenderedText.Color = value;
            }
        }

        /// <summary>
        /// ブレンドモードを取得または設定します。
        /// </summary>
        public AlphaBlend AlphaBlend
        {
            get => _RenderedText.AlphaBlend;
            set
            {
                if (_RenderedText.AlphaBlend == value) return;
                _RenderedText.AlphaBlend = value;
            }
        }

        /// <summary>
        /// 描画する文字列を取得または設定します。
        /// </summary>
        public string Text
        {
            get => _RenderedText.Text;
            set
            {
                if (_RenderedText.Text == value) return;

                _RenderedText.Text = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 文字列の描画に使用するフォントを取得または設定します。
        /// </summary>
        public Font Font
        {
            get => _RenderedText.Font;
            set
            {
                if (_RenderedText.Font == value) return;

                _RenderedText.Font = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 文字の描画に使用するマテリアルを取得または設定します。
        /// </summary>
        public Material MaterialGlyph
        {
            get => _RenderedText.MaterialGlyph;
            set
            {
                if (_RenderedText.MaterialGlyph == value) return;
                _RenderedText.MaterialGlyph = value;
            }
        }

        /// <summary>
        /// 文字テクスチャの描画に使用するマテリアルを取得または設定します。
        /// </summary>
        public Material MaterialImage
        {
            get => _RenderedText.MaterialImage;
            set
            {
                if (_RenderedText.MaterialImage == value) return;
                _RenderedText.MaterialImage = value;
            }
        }

        /// <summary>
        /// 文字の大きさを取得または設定します。
        /// </summary>
        public float FontSize
        {
            get => _RenderedText.FontSize;
            set
            {
                if (_RenderedText.FontSize == value) return;

                _RenderedText.FontSize = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 字間をピクセル単位で取得または設定します。
        /// </summary>
        public float CharacterSpace
        {
            get => _RenderedText.CharacterSpace;
            set
            {
                if (_RenderedText.CharacterSpace == value) return;

                _RenderedText.CharacterSpace = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 行間をピクセル単位で取得または設定します。
        /// Nullを指定するとフォント標準の行間を使用します。
        /// </summary>
        public float? LineGap
        {
            get => _LineGap;
            set
            {
                if (_LineGap == value) return;

                var gap = value ?? Font?.LineGap ?? 0;
                _LineGap = gap;
                _RenderedText.LineGap = gap;
                _RequireCalcTransform = true;
            }
        }
        private float? _LineGap = null;

        /// <summary>
        /// カーニングの有無を取得または設定します。
        /// </summary>
        public bool IsEnableKerning
        {
            get => _RenderedText.IsEnableKerning;
            set
            {
                if (_RenderedText.IsEnableKerning == value) return;

                _RenderedText.IsEnableKerning = value;
                _RequireCalcTransform = true;
            }
        }

        /// <summary>
        /// 行の方向を取得または設定します。
        /// </summary>
        public WritingDirection WritingDirection
        {
            get => _RenderedText.WritingDirection;
            set
            {
                if (_RenderedText.WritingDirection == value) return;

                _RenderedText.WritingDirection = value;
                _RequireCalcTransform = true;
            }
        }

        #endregion

        /// <inheritdoc/>
        public sealed override Vector3F ContentSize => new Vector3F(_RenderedText.RenderingSize.X, _RenderedText.RenderingSize.Y, 0);
    }
}
