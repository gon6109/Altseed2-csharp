using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Altseed2
{
    public partial class CommandList
    {
        /// <summary>
        /// GPUへデータをアップロードする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        public void UploadBuffer<T>(Buffer<T> buffer) where T : struct
        {
            UploadBuffer(buffer.InternalBuffer);
        }

        /// <summary>
        /// GPUからデータを読み込む
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        public void ReadbackBuffer<T>(Buffer<T> buffer) where T : struct
        {
            ReadbackBuffer(buffer.InternalBuffer);
        }

        /// <summary>
        /// GPUのデータをコピーする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void CopyBuffer<T>(Buffer<T> src, Buffer<T> dst) where T : struct
        {
            CopyBuffer(src.InternalBuffer, dst.InternalBuffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexBuffer"></param>
        /// <param name="offset"></param>
        public void SetIndexBuffer<T>(Buffer<T> indexBuffer, int offset = 0) where T : struct
        {
            int stride = Marshal.SizeOf<T>();
            SetIndexBuffer(indexBuffer.InternalBuffer, stride, offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vertexBuffer"></param>
        /// <param name="offset"></param>
        public void SetVertexBuffer<T>(Buffer<T> vertexBuffer, int offset = 0) where T : struct
        {
            int stride = Marshal.SizeOf<T>();
            SetVertexBuffer(vertexBuffer.InternalBuffer, stride, offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="computeBuffer"></param>
        /// <param name="stride"></param>
        /// <param name="unit"></param>
        public void SetComputeBuffer<T>(Buffer<T> computeBuffer, int unit) where T : struct
        {
            int stride = Marshal.SizeOf<T>();
            SetComputeBuffer(computeBuffer.InternalBuffer, stride, unit);
        }

        public Material Material
        {
            set
            {
                value.SetMatrix44F("matView", Engine.CurrentCamera.ViewMatrix);
                value.SetMatrix44F("matProjection", Engine.CurrentCamera.ProjectionMatrix);
                cbg_CommandList_SetMaterial(this.selfPtr, value != null ? value.selfPtr : IntPtr.Zero);
            }
        }
    }
}
