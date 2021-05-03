using System;
using System.Runtime.Serialization;

namespace Altseed2
{
    internal partial class RenderedCamera3D
    {
        partial void OnGetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue($"{S_RenderPassParameter}_Fix", RenderPassParameter);
        }

        partial void OnDeserialize_Constructor(SerializationInfo info, StreamingContext context)
        {
            RenderPassParameter = info.GetValue<RenderPassParameter>($"{S_RenderPassParameter}_Fix");
        }

        partial void Deserialize_GetPtr(ref IntPtr ptr, SerializationInfo info)
        {
            ptr = cbg_RenderedCamera3D_Create();
        }
    }

    internal partial class RenderedPolygon3D
    {
        /// <summary>
        /// インデックスバッファーを取得または設定します。
        /// </summary>
        /// <exception cref="ArgumentNullException">設定しようとした値がnull</exception>
        public Int32Array Buffers
        {
            get => _buffers ??= Int32Array.TryGetFromCache(cbg_RenderedPolygon3D_GetBuffers(selfPtr));
            set
            {
                _buffers = value ?? throw new ArgumentNullException(nameof(value), "引数がnullです");
                cbg_RenderedPolygon3D_SetBuffers(selfPtr, value.selfPtr);
            }
        }
        private Int32Array _buffers;

        /// <summary>
        /// インデックスバッファーを既定のもの設定します。
        /// </summary>
        public void SetDefaultIndexBuffer()
        {
            cbg_RenderedPolygon3D_SetDefaultIndexBuffer(selfPtr);
            _buffers = null;
        }

        partial void Deserialize_GetPtr(ref IntPtr ptr, SerializationInfo info)
        {
            ptr = cbg_RenderedPolygon3D_Create();
        }
    }

    internal partial class RenderedSprite3D
    {
        partial void Deserialize_GetPtr(ref IntPtr ptr, SerializationInfo info)
        {
            ptr = cbg_RenderedSprite3D_Create();
        }
    }

    internal partial class RenderedText3D
    {
        partial void Deserialize_GetPtr(ref IntPtr ptr, SerializationInfo info)
        {
            ptr = cbg_RenderedText3D_Create();
        }
    }
}
