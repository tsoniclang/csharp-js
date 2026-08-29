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
        {
            if (!double.IsFinite(byteLength) || byteLength < 0 || byteLength > int.MaxValue)
                throw new RangeError("ArrayBuffer byteLength is outside the supported range.");
            _buffer = new byte[checked((int)System.Math.Truncate(byteLength))];
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
            var integer = TypedArrayNumbers.IntegerOrInfinity(value);
            if (integer == long.MaxValue) return _buffer.Length;
            if (integer == long.MinValue) return 0;
            var resolved = integer < 0 ? _buffer.Length + integer : integer;
            return checked((int)System.Math.Clamp(resolved, 0, _buffer.Length));
        }
    }
}
