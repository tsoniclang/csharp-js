using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint8Array : TypedArray<Uint8Array, byte>
    {
        public Uint8Array(double length) : base(length) { }
        public Uint8Array(IEnumerable<double> values) : base(values) { }
        public Uint8Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override byte ToElement(double value) => checked((byte)TypedArrayNumbers.ToUnsigned(value, 8));
        protected override double FromElement(byte value) => value;
        protected override Uint8Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint8Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
