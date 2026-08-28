using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint8ClampedArray : TypedArray<Uint8ClampedArray, byte>
    {
        public Uint8ClampedArray(double length) : base(length) { }
        public Uint8ClampedArray(IEnumerable<double> values) : base(values) { }
        public Uint8ClampedArray(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override byte ToElement(double value) => TypedArrayNumbers.ToUint8Clamp(value);
        protected override double FromElement(byte value) => value;
        protected override Uint8ClampedArray CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint8ClampedArray CreateCopy(IEnumerable<double> values) => new(values);
    }
}
