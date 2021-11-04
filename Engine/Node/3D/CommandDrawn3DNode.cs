using System;
using System.Collections.Generic;
using System.Text;

namespace Altseed2
{
    /// <summary>
    /// <see cref="CommandList"/>を呼び出すことで描画するノードを表します。
    /// </summary>
    [Serializable]
    public class CommandDrawn3DNode : Node, IDrawn3D
    {
        #region IDrawn3D
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
        public int ZOrder {
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

        Rendered3D IDrawn3D.Rendered => null;

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

        Vector3F IDrawn3D.ContentSize => new Vector3F();

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

        /// <summary>
        /// 描画時に実行されます．
        /// </summary>
        /// <param name="commandList">現在の<see cref="CommandList"/></param>
        protected virtual void Draw(CommandList commandList) { }

        void IDrawn3D.Draw()
        {
            Draw(Engine.Graphics.CommandList);
        }
    }
}
