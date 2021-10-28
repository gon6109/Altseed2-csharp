using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Altseed2
{
    /// <summary>
    /// CPU/GPUバッファ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Buffer<T> where T : struct
    {
        internal Buffer InternalBuffer { get; }

        /// <summary>
        /// バッファ内の要素数
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// バッファのサイズ
        /// </summary>
        public int Size => InternalBuffer.Size;

        /// <summary>
        /// バッファの使途
        /// </summary>
        public BufferUsageType BufferUsage => InternalBuffer.BufferUsage;

        Buffer(Buffer internalBuffer, int count)
        {
            InternalBuffer = internalBuffer;
            Count = count;
        }

        /// <summary>
        /// バッファを生成します
        /// </summary>
        /// <param name="usage">バッファの使途</param>
        /// <param name="count">要素数</param>
        /// <returns></returns>
        public static Buffer<T> Create(BufferUsageType usage, int count)
        {
            var internalBuffer = Buffer.Create(usage, Marshal.SizeOf<T>() * count);
            if (internalBuffer == null)
            {
                Engine.Log.Error(LogCategory.Engine, "Buffer<T>::Create: can't create internal buffer.");
                return null;
            }

            return new Buffer<T>(internalBuffer, count);
        }

        /// <summary>
        /// バッファへの書き込みを開始します
        /// </summary>
        /// <returns></returns>
        public Span<T> Lock()
        {
            unsafe
            {
                return new Span<T>(InternalBuffer.Lock().ToPointer(), Count);
            }
        }

        /// <summary>
        /// バッファへの書き込みを終了します
        /// </summary>
        public void Unlock()
        {
            InternalBuffer.Unlock();
        }

        /// <summary>
        /// バッファから値を取得します
        /// </summary>
        /// <returns></returns>
        public Span<T> Read()
        {
            unsafe
            {
                var readback = new Span<T>(InternalBuffer.Read().ToPointer(), Count);
                return readback.ToArray();
            }
        }
    }
}
