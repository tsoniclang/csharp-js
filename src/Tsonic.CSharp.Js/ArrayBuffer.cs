/**
 * JavaScript ArrayBuffer implementation
 * Fixed-length binary data buffer backed by native byte[]
 */

using System;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript ArrayBuffer - fixed-length raw binary data buffer
    /// </summary>
    public class ArrayBuffer
    {
        private readonly byte[] _buffer;

        internal byte[] Bytes => _buffer;

        /// <summary>
        /// Create ArrayBuffer with specified byte length
        /// </summary>
        public ArrayBuffer(double byteLength)
            : this(NativeInteger.Length(byteLength))
        {
        }

        internal ArrayBuffer(int byteLength)
        {
            if (byteLength < 0) throw new RangeError("ArrayBuffer length must be non-negative.");
            _buffer = new byte[byteLength];
        }

        /// <summary>
        /// Length of the buffer in bytes
        /// </summary>
        public double byteLength => _buffer.Length;

        internal int ByteLength => _buffer.Length;

        /// <summary>
        /// Create new ArrayBuffer containing a copy of bytes from begin to end
        /// </summary>
        public ArrayBuffer slice(double begin = 0, double? end = null)
        {
            var start = NormalizeIndex(begin);
            var finish = NormalizeIndex(end ?? _buffer.Length);
            var length = System.Math.Max(0, finish - start);
            var result = new ArrayBuffer(length);
            if (length > 0)
                System.Array.Copy(_buffer, start, result._buffer, 0, length);
            return result;
        }

        private int NormalizeIndex(double value)
        {
            var integer = NativeInteger.Index(value);
            var resolved = integer < 0 ? _buffer.Length + integer : integer;
            return checked((int)System.Math.Clamp(resolved, 0, _buffer.Length));
        }
    }
}
